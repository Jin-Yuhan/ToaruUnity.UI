using System;
using UnityEngine;

#if TOARU_UI_ENABLE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace ToaruUnity.UI
{
    /// <summary>
    /// UI工厂
    /// </summary>
    public abstract class UIFactory
    {
        protected UIFactory() { }


        public void Allocate(object key, Transform canvas, Action<object, AbstractView, object> callback, object userData)
        {
            Instantiate(key, canvas, go => callback?.Invoke(key, go.GetComponent<AbstractView>(), userData));
        }

        public void Free(AbstractView view)
        {
            Release(view.gameObject);
        }

        protected abstract void Instantiate(object key, Transform canvas, Action<GameObject> callback);

        protected abstract void Release(GameObject go);


#if TOARU_UI_ENABLE_ADDRESSABLES

        public static UIFactory AddressableFactory => new AddressableUIFactory();


        private sealed class AddressableUIFactory : UIFactory
        {
            protected override void Instantiate(object key, Transform canvas, Action<GameObject> callback)
            {
                InstantiationParameters parameters = new InstantiationParameters(canvas, false);
                AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key, parameters, true);

                asyncOperationHandle.Completed += handle =>
                {
                    GameObject go = handle.Result;
                    callback?.Invoke(go);
                };
            }

            protected override void Release(GameObject go)
            {
                if (!Addressables.ReleaseInstance(go))
                {
                    GameObject.Destroy(go);
                }
            }
        }

#endif
    }
}