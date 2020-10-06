using System.Collections;
using UnityEngine;

#pragma warning disable CS0414

namespace ToaruUnity.UI
{
    /// <summary>
    /// 淡入淡出UI
    /// </summary>
    public abstract class FadeInOutUGUIView : AbstractUGUIView
    {
        [SerializeField, Range(0.1f, 5f)] private float m_FadeInDuration = 0.2f;
        [SerializeField, Range(0.1f, 5f)] private float m_FadeOutDuration = 0.2f;


        protected FadeInOutUGUIView() { }


        protected override void OnCreate()
        {
            base.OnCreate();
            CanvasGroup.alpha = 0;
        }

        protected override IEnumerator OnOpen(object userData) { return FadeIn(); }

        protected override IEnumerator OnClose(object userData) { return FadeOut(); }

        protected override IEnumerator OnResume(object userData) { return FadeIn(); }

        protected override IEnumerator OnSuspend(object userData) { return FadeOut(); }


        private IEnumerator FadeIn()
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0;
            yield return null;

            float time = 0;

            while (time < m_FadeInDuration)
            {
                time += Time.deltaTime;
                CanvasGroup.alpha = Mathf.Lerp(0, 1, time / m_FadeInDuration);
                yield return null;
            }

            CanvasGroup.blocksRaycasts = true;
        }

        private IEnumerator FadeOut()
        {
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 1;
            yield return null;

            float time = 0;

            while (time < m_FadeInDuration)
            {
                time += Time.deltaTime;
                CanvasGroup.alpha = Mathf.Lerp(1, 0, time / m_FadeInDuration);
                yield return null;
            }
        }
    }
}