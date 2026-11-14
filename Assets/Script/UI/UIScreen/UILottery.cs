using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;

public class UILottery : UILuna
{
    private bool revealReady;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        revealReady = false;
        GetComponent<Image>().color = new Color(0.03f, 0.02f, 0.04f, 0.96f);
        var scroll = (RectTransform)transform.Find("LotteryScroll View");
        scroll.anchorMin = new Vector2(0.03f, 0.08f);
        scroll.anchorMax = new Vector2(0.97f, 0.88f);
        scroll.offsetMin = scroll.offsetMax = Vector2.zero;
        var viewport = (RectTransform)scroll.Find("LotteryViewport");
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        var content = (RectTransform)viewport.Find("LotteryContent");
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = content.sizeDelta = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        int count = LunaDataTable.Instance.LotteryResult.Count;
        int columns = count == 1 ? 1 : 5;
        int rows = Mathf.CeilToInt((float)count / columns);
        float width = Mathf.Min(count == 1 ? 300f : 220f,
            (viewport.rect.width - 32f - (columns - 1) * 14f) / columns,
            (viewport.rect.height - 16f - (rows - 1) * 12f) / Mathf.Max(1, rows) * 310f / 445f);
        var grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = new Vector2(width, width * 445f / 310f);
        grid.spacing = new Vector2(14f, 12f);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.childAlignment = TextAnchor.UpperCenter;
        var cardPrefab = Resources.Load<GameObject>("Prefabs/09UI/LotteryCard");
        foreach (string key in LunaDataTable.Instance.LotteryResult)
        {
            var card = Instantiate(cardPrefab, content, false);
            card.name = key + "Card";
            card.AddComponent<RectMask2D>();
            var data = LunaDataTable.Instance.UnitTable[key];
            card.GetComponent<Image>().sprite = data.cardPicture;
            var characterPrefab = Resources.Load<GameObject>("Prefabs/09UI/UICharcter/" + key + "UI");
            var character = Instantiate(characterPrefab, card.transform, false);
            var rect = character.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -35f);
            rect.localScale = characterPrefab.transform.localScale * (width / 310f);
            foreach (var canvas in character.GetComponentsInChildren<Canvas>(true)) canvas.overrideSorting = false;
        }
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        AddLabel("RevealTitle", count + "장 소환 결과", font, new Vector2(0.5f, 1f), new Vector2(0f, -40f));
        AddLabel("RevealContinue", "화면을 눌러 계속", font, new Vector2(0.5f, 0f), new Vector2(0f, 36f));
        StartCoroutine(ReadyToClose());
    }
    private void AddLabel(string name, string message, Font font, Vector2 anchor, Vector2 position)
    {
        var label = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.rectTransform.SetParent(transform, false);
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = anchor;
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = new Vector2(600f, 45f);
        label.font = font; label.fontSize = 26; label.alignment = TextAnchor.MiddleCenter;
        label.text = message; label.color = Color.white; label.raycastTarget = false;
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (revealReady && Input.GetMouseButtonDown(0)) Continue();
    }
    public void Continue()
    {
        if (revealReady) LunaUI.Close(type);
    }
    private IEnumerator ReadyToClose()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        revealReady = true;
    }
}
