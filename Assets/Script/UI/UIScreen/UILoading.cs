using System.Collections;
using EverydayDevup.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILoading : UILuna
{
    public UIReference refData;
    private Image tapToStartImage;
    private AsyncOperation loadingOperation;
    private bool continueRequested;

    public override void OnInit()
    {
        base.OnInit();
        LunaRuntimeData.Initialize();
        var tapToStart = transform.Find("TabToStart");
        if (tapToStart != null) tapToStartImage = tapToStart.GetComponent<Image>();
        StartCoroutine(LoadLobby());
    }

    private IEnumerator LoadLobby()
    {
        loadingOperation = SceneManager.LoadSceneAsync("Lobby");
        if (loadingOperation == null) yield break;
        loadingOperation.allowSceneActivation = false;
        while (!loadingOperation.isDone)
        {
            if (tapToStartImage != null)
            {
                var color = tapToStartImage.color;
                color.a = Mathf.Clamp01(Mathf.Abs(Mathf.Sin(Time.unscaledTime) * 2f));
                tapToStartImage.color = color;
            }
            if (Input.GetMouseButtonDown(0)) ContinueToLobby();
            yield return null;
        }
    }

    // Input and the play-mode smoke test use the same guarded entry point.
    public void ContinueToLobby()
    {
        if (continueRequested || loadingOperation == null || loadingOperation.progress < 0.9f) return;
        continueRequested = true;
        Game.Instance.lunaSceneManager.FadeIn(1f, () => loadingOperation.allowSceneActivation = true);
    }
}
