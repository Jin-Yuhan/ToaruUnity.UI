using System;

namespace ToaruUnity.UI
{
    /// <summary>
    /// 界面加载器的抽象基类
    /// </summary>
    public abstract class ViewLoader
    {
        /// <summary>
        /// 加载完成的回调
        /// </summary>
        /// <param name="key">预制体的key</param>
        /// <param name="prefab">加载的预制体</param>
        /// <param name="userData">用户数据</param>
        public delegate void CompleteCallback(object key, AbstractView prefab, object userData);


        protected ViewLoader() { }


        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="key">预制体的key</param>
        /// <param name="callback">加载完成预制体的回调方法</param>
        /// <param name="userData">用户数据</param>
        public void Load(object key, CompleteCallback callback, object userData)
        {
            LoadPrefab(key, v =>
            {
                callback?.Invoke(key, v, userData);
            });
        }

        /// <summary>
        /// 释放预制体
        /// </summary>
        /// <param name="key">预制体的key</param>
        /// <param name="prefab">需要释放的预制体</param>
        public void Release(object key, ref AbstractView prefab)
        {
            ReleasePrefab(key, prefab);
            prefab = null;
        }


        /// <summary>
        /// 重写该方法，来实现加载预制体的逻辑。
        /// </summary>
        /// <param name="key">预制体的key</param>
        /// <param name="callback">加载完成预制体的回调方法</param>
        protected abstract void LoadPrefab(object key, Action<AbstractView> callback);

        /// <summary>
        /// 重写该方法，来实现释放预制体的逻辑。
        /// </summary>
        /// <param name="key">预制体的key</param>
        /// <param name="prefab">需要释放的预制体</param>
        protected abstract void ReleasePrefab(object key, AbstractView prefab);
    }
}