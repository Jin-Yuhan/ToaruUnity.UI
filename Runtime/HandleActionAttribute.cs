using System;

namespace ToaruUnity.UI
{
    /// <summary>
    /// 指示该方法将处理一个操作
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class HandleActionAttribute : Attribute
    {
        /// <summary>
        /// 获取该方法处理的操作的标识号
        /// </summary>
        public int ActionId { get; }

        /// <summary>
        /// 指示该方法将处理一个操作
        /// </summary>
        /// <param name="actionId">方法处理的操作的标识号</param>
        public HandleActionAttribute(int actionId)
        {
            ActionId = actionId;
        }
    }
}