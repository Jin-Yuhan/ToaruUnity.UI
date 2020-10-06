using UnityEngine;

namespace ToaruUnity.UI.Settings
{
    [CreateAssetMenu(fileName = "NewUISettings", menuName = "ToaruUnity/UI Settings")]
    public sealed class ToaruUISettings : ScriptableObject
    {
        [SerializeField] private string m_CanvasTag = "Untagged";
        [SerializeField] private int m_UIObjPoolSize = 5;
        [SerializeField] private int m_StackMinGrow = 5;
        [SerializeField] private bool m_AutoClearWhenUnloadingScene = true;

        /// <summary>
        /// 获取场景中Canvas对象的Tag
        /// </summary>
        public string CanvasTag => m_CanvasTag;

        /// <summary>
        /// 获取UI对象池的长度
        /// </summary>
        public int UIObjPoolSize => m_UIObjPoolSize;

        /// <summary>
        /// 获取栈长度不够时，重新分配的栈的长度的最小增长量
        /// </summary>
        public int StackMinGrow => m_StackMinGrow;

        /// <summary>
        /// 获取是否在场景被卸载时，自动清空所有缓存
        /// </summary>
        public bool AutoClearWhenUnloadingScene => m_AutoClearWhenUnloadingScene;
    }
}