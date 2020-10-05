namespace ToaruUnity.UI
{
    /// <summary>
    /// 表示界面的状态
    /// </summary>
    public enum ViewState : byte
    {
        /// <summary>
        /// 表示界面处于被关闭状态
        /// </summary>
        Closed = 0,
        /// <summary>
        /// 表示界面处于活动状态
        /// </summary>
        Active = 1,
        /// <summary>
        /// 表示界面处于被暂停状态
        /// </summary>
        Suspended = 2
    }
}