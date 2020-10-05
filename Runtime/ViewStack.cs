using System;
using System.Collections;
using System.Collections.Generic;

namespace ToaruUnity.UI
{
    /// <summary>
    /// 表示一个界面的LIFO集合
    /// </summary>
    public sealed class ViewStack : IReadOnlyList<AbstractView>
    {
        private AbstractView[] m_Stack;
        private int m_TopIndex;

        /// <summary>
        /// 向栈中添加元素的事件
        /// </summary>
        public event Action<AbstractView> OnPushView;

        /// <summary>
        /// 移除栈中元素的事件
        /// </summary>
        public event Action<AbstractView> OnPopView;

        /// <summary>
        /// 获取元素的数量
        /// </summary>
        public int Count => m_TopIndex + 1;

        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        /// <param name="index">元素的索引（顶部元素的索引为0，下方元素的索引依次递增）</param>
        /// <returns>如果索引位置有元素，返回该元素；否则返回null</returns>
        public AbstractView this[int index] => Peek(index);


        public ViewStack()
        {
            m_Stack = Array.Empty<AbstractView>();
            m_TopIndex = -1;
        }


        public void Push(AbstractView view, object userData)
        {
            if (m_TopIndex > -1)
            {
                AbstractView last = m_Stack[m_TopIndex];
                last.TransformState(ViewState.Suspended, userData); // 暂停上一个ui
            }

            m_TopIndex++;

            if (m_Stack.Length == m_TopIndex)
            {
                Grow();
            }

            m_Stack[m_TopIndex] = view; // 放到栈顶

            view.OnStateChanged += OnViewStateChanged;

            view.OnBeforeOpen();
            view.TransformState(ViewState.Active, userData);

            OnPushView?.Invoke(view);
        }

        public void Pop(object userData)
        {
            if (m_TopIndex < 0)
            {
                return;
            }

            AbstractView view = m_Stack[m_TopIndex];
            m_Stack[m_TopIndex--] = default;

            view.TransformState(ViewState.Closed, userData);

            if (m_TopIndex > -1)
            {
                AbstractView last = m_Stack[m_TopIndex];
                last.TransformState(ViewState.Active, userData); // 恢复上一个ui
            }

            OnPopView?.Invoke(view);
        }

        public AbstractView Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        /// <param name="index">元素的索引（顶部元素的索引为0，下方元素的索引依次递增）</param>
        /// <returns>如果索引位置有元素，返回该元素；否则返回null</returns>
        public AbstractView Peek(int index)
        {
            int i = m_TopIndex - index;
            return (i > -1 && i <= m_TopIndex) ? m_Stack[i] : default;
        }

        public void Clear(object userData)
        {
            while (m_TopIndex > -1)
            {
                AbstractView ui = m_Stack[m_TopIndex];
                m_Stack[m_TopIndex--] = default;

                ui.TransformState(ViewState.Closed, userData);

                OnPopView?.Invoke(ui);
            }
        }

        public IEnumerator<AbstractView> GetEnumerator()
        {
            for (int i = m_TopIndex; i > -1; i--)
            {
                yield return m_Stack[i];
            }
        }



        private const int MinGrow = 4;

        private void Grow(int grow = MinGrow)
        {
            int newCapacity = m_Stack.Length << 1;
            int minCapacity = m_Stack.Length + grow;

            if (newCapacity < minCapacity)
            {
                newCapacity = minCapacity;
            }

            AbstractView[] array = new AbstractView[newCapacity];
            Array.Copy(m_Stack, 0, array, 0, m_Stack.Length);
            m_Stack = array;
        }

        private void OnViewStateChanged(AbstractView view, ViewState state)
        {
            switch (state)
            {
                case ViewState.Closed:
                    view.enabled = false;
                    view.OnStateChanged -= OnViewStateChanged; // 页面被关闭，取消对事件的监听
                    break;

                case ViewState.Active:
                    view.enabled = true;
                    view.Transform.SetAsLastSibling();
                    break;

                case ViewState.Suspended:
                    view.enabled = false;
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}