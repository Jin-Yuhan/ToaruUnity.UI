using System;
using System.Collections.Generic;
using ToaruUnity.UI.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ToaruUnity.UI
{
    /// <summary>
    /// UI管理类
    /// </summary>
    public sealed class UIManager
    {
        private readonly ViewStack m_Stack;
        private readonly ViewLoader m_Loader;
        private readonly ToaruUISettings m_Settings;
        private readonly Dictionary<object, ViewCache> m_Cache;
        private Transform m_Canvas;


        /// <summary>
        /// 获取打开的页面数量
        /// </summary>
        public int ViewCount => m_Stack.Count;

        /// <summary>
        /// 获取Canvas
        /// </summary>
        /// <exception cref="InvalidOperationException">场景中没有Canvas对象</exception>
        /// <exception cref="ArgumentNullException">value为null</exception>
        public Transform Canvas
        {
            get
            {
                if (!m_Canvas)
                {
                    GameObject go = GameObject.FindGameObjectWithTag(m_Settings.CanvasTag);

                    if (go == null)
                    {
                        throw new InvalidOperationException("场景中没有Canvas对象");
                    }

                    m_Canvas = go.transform;
                }

                return m_Canvas;
            }
            set => m_Canvas = value ?? throw new ArgumentNullException("canvas");
        }

        /// <summary>
        /// 打开页面事件
        /// </summary>
        public event Action<AbstractView> OnOpenView
        {
            add => m_Stack.OnPushView += value;
            remove => m_Stack.OnPushView -= value;
        }

        /// <summary>
        /// 关闭页面事件
        /// </summary>
        public event Action<AbstractView> OnCloseView
        {
            add => m_Stack.OnPopView += value;
            remove => m_Stack.OnPopView -= value;
        }


        /// <summary>
        /// 创建一个新的UIManager实例
        /// </summary>
        /// <param name="loader">页面的加载器</param>
        /// <param name="settings">设置</param>
        /// <exception cref="ArgumentNullException"><paramref name="loader"/>为null</exception>
        public UIManager(ViewLoader loader, ToaruUISettings settings)
        {
            m_Stack = new ViewStack(settings.StackMinGrow);
            m_Loader = loader ?? throw new ArgumentNullException(nameof(loader));
            m_Settings = settings;
            m_Cache = new Dictionary<object, ViewCache>();
            m_Canvas = null;

            if (settings.AutoClearWhenUnloadingScene)
            {
                SceneManager.sceneUnloaded += (_) =>
                {
                    Clear(true);
                };
            }
        }


        /// <summary>
        /// 打开一个新的页面
        /// </summary>
        /// <param name="key">页面的key</param>
        /// <param name="userData">用户数据</param>
        public void Open(object key, object userData = null)
        {
            if (m_Cache.TryGetValue(key, out ViewCache cache))
            {
                OpenView(cache, userData);
            }
            else
            {
                m_Loader.Load(key, LoadViewCallback, userData);
            }
        }

        private void OpenView(ViewCache cache, object userData)
        {
            AbstractView view = cache.AllocateView(Canvas);
            m_Stack.Push(view, userData);
        }

        private void LoadViewCallback(object key, AbstractView prefab, object userData)
        {
            Type viewType = prefab.GetType();
            ActionCenter center = ActionCenter.New(viewType, this);
            ViewCache cache = new ViewCache(m_Settings, key, prefab, center);

            m_Cache.Add(key, cache);
            OpenView(cache, userData);
        }


        /// <summary>
        /// 关闭顶部页面
        /// </summary>
        /// <param name="userData">用户数据</param>
        public void CloseTop(object userData = null)
        {
            m_Stack.Pop(userData);
        }

        /// <summary>
        /// 关闭所有页面
        /// </summary>
        /// <param name="userData">用户数据</param>
        public void CloseAll(object userData = null)
        {
            m_Stack.Clear(userData);
        }

        /// <summary>
        /// 释放页面的缓存
        /// </summary>
        /// <param name="key">页面的key</param>
        /// <param name="destroy">是否直接销毁缓存</param>
        public void ReleaseViewCache(object key, bool destroy)
        {
            if (m_Cache.TryGetValue(key, out ViewCache cache))
            {
                cache.Release(m_Loader, destroy);

                if (destroy)
                {
                    m_Cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// 清理全部缓存
        /// </summary>
        /// <param name="destroyCache">是否直接销毁缓存</param>
        public void Clear(bool destroyCache)
        {
            m_Canvas = null;
            m_Stack.Clear(null);

            foreach (ViewCache cache in m_Cache.Values)
            {
                cache.Release(m_Loader, destroyCache);
            }

            if (destroyCache)
            {
                m_Cache.Clear();
            }
        }


        private sealed class ViewCache
        {
            private readonly object m_Key;
            private readonly ActionCenter m_Center;
            private readonly AbstractView[] m_ViewPool;
            private AbstractView m_Prefab;
            private int m_ObjectCount;
            private bool m_IsCenterUsed;

            public ViewCache(ToaruUISettings settings, object key, AbstractView prefab, ActionCenter center)
            {
                m_Key = key;
                m_Center = center;
                int size = settings.UIObjPoolSize;
                m_ViewPool = size == 0 ? Array.Empty<AbstractView>() : new AbstractView[size];

                m_Prefab = prefab;
                m_ObjectCount = 0;
                m_IsCenterUsed = false;
            }

            public AbstractView AllocateView(Transform canvas)
            {
                AbstractView view;

                if (m_ObjectCount == 0)
                {
                    ActionCenter center = AllocateCenter();
                    view = Object.Instantiate(m_Prefab, canvas, false);

                    view.Initialize(m_Key, center);
                    view.OnStateChanged += OnViewStateChanged;
                }
                else
                {
                    view = m_ViewPool[--m_ObjectCount];
                    view.Transform.SetParent(canvas, false);

                    m_ViewPool[m_ObjectCount] = default;
                }

                return view;
            }

            private void OnViewStateChanged(AbstractView view, ViewState state)
            {
                // 不取消注册事件，下次还会使用

                if (state == ViewState.Closed)
                {
                    FreeView(view);
                }
            }

            private void FreeView(AbstractView view)
            {
                // 如果prefab为空，那么当前缓存对象已经被销毁了

                if (m_Prefab && m_ObjectCount < m_ViewPool.Length)
                {
                    m_ViewPool[m_ObjectCount++] = view;
                }
                else
                {
                    Object.Destroy(view.gameObject);
                }
            }

            private ActionCenter AllocateCenter()
            {
                if (m_IsCenterUsed)
                {
                    return ActionCenter.Clone(m_Center);
                }

                m_IsCenterUsed = true;
                return m_Center;
            }

            /// <summary>
            /// 如果直接销毁，就无法重用
            /// </summary>
            /// <param name="loader"></param>
            /// <param name="destroy">是否直接销毁</param>
            public void Release(ViewLoader loader, bool destroy)
            {
                for (int i = 0; i < m_ObjectCount; i++)
                {
                    Object.Destroy(m_ViewPool[i]);
                    m_ViewPool[i] = default;
                }

                m_ObjectCount = 0;
                //m_IsCenterUsed = false; // center是一次性的

                if (destroy)
                {
                    loader.Release(m_Key, ref m_Prefab);
                }
            }
        }
    }
}