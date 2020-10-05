using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToaruUnity.UI
{
    public sealed class UIManager
    {
        private class ViewCache
        {
            private readonly ActionCenter m_Center;
            private readonly int m_PoolSize;
            private AbstractView[] m_ViewPool;
            private int m_ObjCount;
            private bool m_IsCenterUsed;

            public ViewCache(Type viewType, UIManager manager, int poolSize)
            {
                m_Center = ActionCenter.New(viewType, manager);
                m_PoolSize = poolSize;
                m_ViewPool = null;
                m_ObjCount = 0;
                m_IsCenterUsed = false;
            }

            public bool TryGetView(out AbstractView view)
            {
                if (m_ObjCount > 0)
                {
                    view = m_ViewPool[--m_ObjCount];
                    m_ViewPool[m_ObjCount] = default;
                    return true;
                }

                view = null;
                return false;
            }

            public bool TryRecycleView(AbstractView view)
            {
                if (m_PoolSize < 1)
                {
                    return false;
                }

                if (m_ViewPool == null)
                {
                    m_ViewPool = new AbstractView[m_PoolSize];
                }

                if (m_ObjCount < m_ViewPool.Length)
                {
                    m_ViewPool[m_ObjCount++] = view;
                    return true;
                }

                return false;
            }

            public ActionCenter AllocateActionCenter()
            {
                if (m_Center == null)
                {
                    return null;
                }

                if (m_IsCenterUsed)
                {
                    return ActionCenter.Clone(m_Center);
                }

                m_IsCenterUsed = true;
                return m_Center;
            }
        }


        private Transform m_Canvas;
        private readonly ViewStack m_Stack;
        private readonly UIFactory m_Factory;
        private readonly int m_ViewPoolSize;
        private readonly Dictionary<object, ViewCache> m_Cache;


        public int OpenedUICount => m_Stack.Count;

        public Transform Canvas
        {
            get => m_Canvas;
            set => m_Canvas = value ?? throw new ArgumentNullException("canvas");
        }

        public event Action<AbstractView> OnOpenView
        {
            add => m_Stack.OnPushView += value;
            remove => m_Stack.OnPushView -= value;
        }

        public event Action<AbstractView> OnCloseView
        {
            add => m_Stack.OnPopView += value;
            remove => m_Stack.OnPopView -= value;
        }


        public UIManager(UIFactory factory, int viewPoolSize, bool autoClearViewsWhenSceneUnloaded)
            : this(null, factory, viewPoolSize, autoClearViewsWhenSceneUnloaded) { }

        public UIManager(Transform canvas, UIFactory factory, int viewPoolSize, bool autoClearViewsWhenSceneUnloaded)
        {
            m_Canvas = canvas;
            m_Stack = new ViewStack();
            m_Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            m_ViewPoolSize = viewPoolSize;
            m_Cache = new Dictionary<object, ViewCache>();

            if (autoClearViewsWhenSceneUnloaded)
            {
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
        }


        private void OnSceneUnloaded(Scene arg0)
        {
            m_Stack.Clear(null);

            foreach (ViewCache cache in m_Cache.Values)
            {
                while (cache.TryGetView(out AbstractView view))
                {
                    m_Factory.Free(view);
                }
            }

            m_Cache.Clear();
        }

        public void Open(object key, object userData = null)
        {
            if (m_Canvas == null)
            {
                throw new InvalidOperationException("没有设置Canvas");
            }

            if (m_Cache.TryGetValue(key, out ViewCache cache) && cache.TryGetView(out AbstractView view))
            {
                m_Stack.Push(view, userData);
            }
            else
            {
                m_Factory.Allocate(key, m_Canvas, LoadUIAssetCallback, userData);
            }
        }

        private void LoadUIAssetCallback(object key, AbstractView view, object userData)
        {
            Type viewType = view.GetType();
            ViewCache cache = new ViewCache(viewType, this, m_ViewPoolSize);
            m_Cache.Add(key, cache);

            ActionCenter actionCenter = cache.AllocateActionCenter();
            view.Initialize(key, actionCenter);
            view.OnStateChanged += OnViewStateChanged;

            m_Stack.Push(view, userData);
        }

        private void OnViewStateChanged(AbstractView view, ViewState state)
        {
            if (state == ViewState.Closed)
            {
                ViewCache cache = m_Cache[view.InternalKey];

                if (!cache.TryRecycleView(view))
                {
                    m_Factory.Free(view);
                }

                // 不取消注册事件，下次还会使用
            }
        }

        public void CloseTop(object userData = null)
        {
            m_Stack.Pop(userData);
        }

        public void CloseAll(object userData = null)
        {
            m_Stack.Clear(userData);
        }

        public void ReleaseView(object key)
        {
            if (m_Cache.TryGetValue(key, out ViewCache cache))
            {
                while (cache.TryGetView(out AbstractView view))
                {
                    m_Factory.Free(view);
                }

                m_Cache.Remove(key);
            }
        }
    }
}