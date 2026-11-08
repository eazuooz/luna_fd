using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : UILuna
{
    private Image tapToStartImage;
    private bool continueRequested;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        continueRequested = false;
        var tapToStart = transform.Find("TabToStart");
        if (tapToStart != null) tapToStartImage = tapToStart.GetComponent<Image>();
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (tapToStartImage != null)
        {
            var color = tapToStartImage.color;
            color.a = Mathf.Clamp01(Mathf.Abs(Mathf.Sin(Time.unscaledTime) * 2f));
            tapToStartImage.color = color;
        }
        if (Input.GetMouseButtonDown(0)) ContinueToLobby();
    }
    public void ContinueToLobby()
    {
        if (continueRequested || !LunaBootstrap.IsReady || LunaScenes.IsTransitioning) return;
        continueRequested = true;
        LunaScenes.FadeIn(1f, () => LunaScenes.Load("Lobby"));
    }
}
