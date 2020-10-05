using System;
using UnityEngine;
using System.Collections;

#pragma warning disable IDE0032

namespace ToaruUnity.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class AbstractUGUIView : AbstractView
    {
        private CanvasGroup m_CanvasGroup;

        /// <summary>
        /// 获取当前对象的<see cref="UnityEngine.CanvasGroup"/>组件
        /// </summary>
        protected CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = GetComponent<CanvasGroup>());


        protected AbstractUGUIView() { }


        protected override IEnumerator OnOpen(object userData)
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.blocksRaycasts = true;

            return null;
        }

        protected override IEnumerator OnClose(object userData)
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.blocksRaycasts = false;

            return null;
        }

        protected override IEnumerator OnResume(object userData)
        {
            CanvasGroup.blocksRaycasts = true;

            return null;
        }

        protected override IEnumerator OnSuspend(object userData)
        {
            CanvasGroup.blocksRaycasts = false;

            return null;
        }
    }
}