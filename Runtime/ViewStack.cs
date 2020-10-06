using System;
using System.Collections;
using System.Collections.Generic;

namespace ToaruUnity.UI
{
    /// <summary>
    /// 表示一个界面的LIFO集合，能自动管理各个界面的状态，并提供对应的事件
    /// </summary>
    public class ViewStack : IReadOnlyList<AbstractView>
    {
        private AbstractView[] m_Stack; // 避免元素为null
        private readonly int m_MinGrow;
        private int m_TopIndex;
        private int m_Version;

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
        /// <param name="index">元素的索引（栈顶元素的索引为0，向下依次以1递增）</param>
        /// <returns>如果索引位置有元素，则返回该元素；否则返回null</returns>
        public AbstractView this[int index] => Peek(index);


        /// <summary>
        /// 创建一个新的ViewStack对象
        /// </summary>
        /// <param name="minGrow">栈长度不够时，重新分配的栈的长度的最小增长量，该值必须大于0</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minGrow"/>小于1</exception>
        public ViewStack(int minGrow)
        {
            if (minGrow < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(minGrow));
            }

            m_Stack = Array.Empty<AbstractView>();
            m_MinGrow = minGrow;
            m_TopIndex = -1;
            m_Version = int.MinValue;
        }


        /// <summary>
        /// 在栈顶添加一个新元素并触发一次<see cref="OnPushView"/>事件
        /// </summary>
        /// <param name="view">要添加的元素，该值不能为null</param>
        /// <param name="userData">用户数据</param>
        /// <exception cref="ArgumentNullException"><paramref name="view"/>为null</exception>
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

            m_Stack[m_TopIndex] = view ?? throw new ArgumentNullException(nameof(view)); // 放到栈顶

            view.OnStateChanged += OnViewStateChanged;

            view.OnBeforeOpen();
            view.TransformState(ViewState.Active, userData);

            m_Version++;
            OnPushView?.Invoke(view);
        }

        /// <summary>
        /// 如果栈中有元素，则移除栈顶元素并触发一次<see cref="OnPopView"/>事件
        /// </summary>
        /// <param name="userData">用户数据</param>
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

            m_Version++;
            OnPopView?.Invoke(view);
        }

        /// <summary>
        /// 获取栈顶的元素
        /// </summary>
        /// <returns>如果栈中有元素，则返回栈顶元素；否则返回null</returns>
        public AbstractView Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        /// <param name="index">元素的索引（栈顶元素的索引为0，向下依次以1递增）</param>
        /// <returns>如果索引位置有元素，则返回该元素；否则返回null</returns>
        public AbstractView Peek(int index)
        {
            int i = m_TopIndex - index;
            return (i > -1 && i <= m_TopIndex) ? m_Stack[i] : default;
        }

        /// <summary>
        /// 依次移除栈中的所有元素，并为每一个元素触发<see cref="OnPopView"/>事件
        /// </summary>
        /// <param name="userData">用户数据</param>
        public void Clear(object userData)
        {
            while (m_TopIndex > -1)
            {
                AbstractView view = m_Stack[m_TopIndex];
                m_Stack[m_TopIndex--] = default;
                m_Version++;

                view.TransformState(ViewState.Closed, userData);
                OnPopView?.Invoke(view);
            }
        }

        /// <summary>
        /// 获取栈中是否包含指定元素
        /// </summary>
        /// <param name="item">查询的元素</param>
        /// <returns>如果包含该元素，返回true；否则返回false</returns>
        public bool Contains(AbstractView item)
        {
            if (item == null)
            {
                return false;
            }

            for (int i = m_TopIndex; i > -1; i--)
            {
                if (m_Stack[i] == item)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 将栈中元素依次拷贝至数组中
        /// </summary>
        /// <param name="array">需要填充的数组</param>
        /// <param name="arrayIndex">填充<paramref name="array"/>的起始索引位置</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/>为null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/>超出范围</exception>
        /// <exception cref="ArgumentException"><paramref name="array"/>的长度不足</exception>
        public void CopyTo(AbstractView[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            int count = m_TopIndex + 1;

            if (array.Length - arrayIndex < count)
            {
                throw new ArgumentException("数组长度不足", nameof(array));
            }

            Array.Copy(m_Stack, 0, array, arrayIndex, count);
            Array.Reverse(array, arrayIndex, count);
        }

        public AbstractView[] ToArray()
        {
            AbstractView[] array = new AbstractView[m_TopIndex + 1];

            for (int i = m_TopIndex; i > -1; i--)
            {
                array[m_TopIndex - i] = m_Stack[i];
            }

            return array;
        }

        /// <summary>
        /// 获取当前实例的迭代器。迭代器将从栈顶开始向下依次遍历。
        /// </summary>
        /// <returns>当前实例的枚举器</returns>
        /// <exception cref="InvalidOperationException">迭代时修改集合</exception>
        public IEnumerator<AbstractView> GetEnumerator()
        {
            int version = m_Version;

            for (int i = m_TopIndex; i > -1; i--)
            {
                if (m_Version != version)
                {
                    throw new InvalidOperationException("迭代时修改集合");
                }

                yield return m_Stack[i];
            }
        }



        private void Grow()
        {
            int newCapacity = m_Stack.Length << 1;
            int minCapacity = m_Stack.Length + m_MinGrow;

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