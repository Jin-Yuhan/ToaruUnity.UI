using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable IDE1006
#pragma warning disable CS0649

namespace ToaruUnity.UI
{
    /// <summary>
    /// 所有页面的抽象基类
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class AbstractView : MonoBehaviour
    {
        [Serializable]
        private sealed class StateChangedEvent : UnityEvent<AbstractView, ViewState> { }

        private readonly struct TransformStateTask
        {
            public readonly ViewState NewState;
            public readonly object UserData;

            public TransformStateTask(ViewState newState, object userData)
            {
                NewState = newState;
                UserData = userData;
            }
        }


        private ViewState m_State;
        private Queue<TransformStateTask> m_TransformTasks; // may be null
        private Transform m_Transform; // may be null

        [SerializeField]
        [Tooltip("当界面的状态发生变化时触发")]
        private StateChangedEvent m_OnStateChanged;

        /// <summary>
        /// 获取当前对象的状态
        /// </summary>
        public ViewState State
        {
            get => m_State;
            private set
            {
                if (m_State != value)
                {
                    m_State = value;
                    m_OnStateChanged.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// 获取是否正在切换状态
        /// </summary>
        internal bool IsTransformingState { get; private set; }

        /// <summary>
        /// 获取加载当前对象使用的Key
        /// </summary>
        internal object InternalKey { get; private set; }

        /// <summary>
        /// 获取注入的<see cref="ActionCenter"/>对象。
        /// 如果没有指定注入类型或者注入失败，将会返回null。
        /// </summary>
        protected internal ActionCenter Actions { get; private set; }

        /// <summary>
        /// 获取当前对象的<see cref="UnityEngine.Transform"/>组件
        /// </summary>
        public Transform Transform => m_Transform ?? (m_Transform = GetComponent<Transform>());

        /// <summary>
        /// 获取剩余的切换状态的任务数量
        /// </summary>
        internal int RemainingTransformStateTaskCount => m_TransformTasks == null ? 0 : m_TransformTasks.Count;

        /// <summary>
        /// 页面的状态发生变化
        /// </summary>
        public event UnityAction<AbstractView, ViewState> OnStateChanged
        {
            add
            {
                if (value != null)
                {
                    m_OnStateChanged.AddListener(value);
                }
            }
            remove
            {
                if (value != null)
                {
                    m_OnStateChanged.RemoveListener(value);
                }
            }
        }


        protected AbstractView() { }


        internal void OnBeforeOpen()
        {
            Actions?.Reset();
        }

        internal void Initialize(object key, ActionCenter actionCenter)
        {
            m_State = ViewState.Closed;
            m_TransformTasks = null;
            m_Transform = null;

            IsTransformingState = false;
            InternalKey = key;
            Actions = actionCenter;
            Actions?.RegisterStateChangeHandler(OnRefreshView);
        }

        internal void TransformState(ViewState newState, object userData)
        {
            if (IsTransformingState)
            {
                if (m_TransformTasks == null)
                {
                    m_TransformTasks = new Queue<TransformStateTask>();
                }

                m_TransformTasks.Enqueue(new TransformStateTask(newState, userData));
            }
            else
            {
                IEnumerator func = GetTransformCoroutine(newState, userData);

                if (func == null)
                {
                    // 非协程
                    State = newState;
                }
                else
                {
                    StartCoroutine(StateTransformer(func, newState));
                }
            }
        }

        private IEnumerator StateTransformer(IEnumerator func, ViewState newState)
        {
            IsTransformingState = true;

            // 先进行一次状态切换
            {
                yield return func;

                State = newState;
            }

            // 如果后续还有需要切换的状态，则继续
            // 避免再次开启一个协程
            if (m_TransformTasks != null)
            {
                while (m_TransformTasks.Count > 0)
                {
                    TransformStateTask task = m_TransformTasks.Dequeue();
                    func = GetTransformCoroutine(task.NewState, task.UserData);

                    yield return func; // null的话，就等一帧

                    State = task.NewState;
                }
            }

            IsTransformingState = false;
        }

        private IEnumerator GetTransformCoroutine(ViewState newState, object userData)
        {
            switch (newState)
            {
                case ViewState.Closed when State == ViewState.Active:
                    return OnClose(userData);

                case ViewState.Suspended when State == ViewState.Active:
                    return OnSuspend(userData);

                case ViewState.Active when State == ViewState.Closed:
                    return OnOpen(userData);

                case ViewState.Active when State == ViewState.Suspended:
                    return OnResume(userData);

                default:
                    throw new InvalidOperationException($"无法从状态{State}切换到{newState}");
            }
        }


        protected virtual void OnCreate() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnRefreshView(IActionState state) { }

        protected virtual void OnUpdate(float deltaTime) { }

        protected virtual IEnumerator OnOpen(object userData) { return null; }

        protected virtual IEnumerator OnClose(object userData) { return null; }

        protected virtual IEnumerator OnResume(object userData) { return null; }

        protected virtual IEnumerator OnSuspend(object userData) { return null; }

        private void Awake()
        {
            OnCreate();
        }

        private void Update()
        {
            Actions?.UpdateCoroutines();
            OnUpdate(Time.deltaTime);
        }
    }
}