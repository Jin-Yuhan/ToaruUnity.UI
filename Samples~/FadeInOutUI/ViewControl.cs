using System;
using ToaruUnity.UI;
using ToaruUnity.UI.Settings;
using System.Collections;
using UnityEngine;

public sealed class ViewControl : MonoBehaviour
{
    [SerializeField] private ToaruUISettings m_Settings;

    private IEnumerator Start()
    {
        UIManager manager = new UIManager(new TestViewLoader(), m_Settings);
        manager.Open("Test View");

        yield return new WaitForSecondsRealtime(1);

        manager.CloseTop();
    }
}


class TestViewLoader : ViewLoader
{
    protected override void LoadPrefab(object key, Action<AbstractView> callback)
    {
        Resources.LoadAsync<AbstractView>(key as string).completed += op =>
        {
            callback?.Invoke((op as ResourceRequest).asset as AbstractView);
        };
    }

    protected override void ReleasePrefab(object key, AbstractView prefab)
    {
        GameObject.Destroy(prefab);
    }
}