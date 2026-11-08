using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using Luna.Gameplay;

public class UIProgressBar : UILuna
{
    private readonly Dictionary<int, GameObject> profiles = new Dictionary<int, GameObject>();
    private readonly List<int> retired = new List<int>();
    private RectTransform minimap;
    private Image titleImage;
    private Text titleText;
    private EventComponent events;
    private bool subscribed;
    private float age;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        age = 0f;
        minimap = transform.Find("MinimapImage") as RectTransform;
        titleImage = transform.Find("TitleImage").GetComponent<Image>();
        titleText = transform.Find("TitleText").GetComponent<Text>();
        titleText.text = "악마의 탑\n지하 " + LunaSession.Current.CurrentStageLevelString();
        transform.Find("MinimapText").GetComponent<Text>().text = "악마의 탑 지하 " + LunaSession.Current.CurrentStageLevelString();
        events = GameEntry.GetComponent<EventComponent>();
        events.Subscribe(ShowEntitySuccessEventArgs.EventId, OnEntityShown);
        events.Subscribe(HideEntityCompleteEventArgs.EventId, OnEntityHidden);
        subscribed = true;
        foreach (var unit in UnitLogic.ActiveUnits) Add(unit);
    }
    public void PushProfile(GameObject instance) => Add(UnitLogic.From(instance));
    public void ReleaseMinimapObject(GameObject instance)
    {
        var unit = UnitLogic.From(instance);
        if (unit != null) Remove(unit.ID);
    }
    private void Add(UnitLogic unit)
    {
        if (unit == null || unit.IsHiding || !unit.Available || unit.SpawnData == null || LunaScenes.CurrentGame == null ||
            unit.SpawnData.RoundId != LunaScenes.CurrentGame.RoundId || profiles.ContainsKey(unit.ID)) return;
        if (!unit.objectData.TypeLunaPlayer && unit.UnitType != eUnitType.Hero && unit.UnitType != eUnitType.Tower &&
            unit.UnitType != eUnitType.TowerAndBoss && unit.UnitType != eUnitType.Boss) return;
        if (unit.objectData.minimapPicture == null) return;
        var prefab = Resources.Load<GameObject>("Prefabs/09UI/minimapObject");
        var icon = Instantiate(prefab, minimap, false);
        icon.transform.localScale = Vector3.one;
        var image = icon.GetComponent<Image>();
        image.sprite = unit.objectData.minimapPicture; image.SetNativeSize();
        var distance = icon.transform.Find("DistanceText");
        if (distance != null) distance.gameObject.SetActive(unit.UnitType == eUnitType.Hero || unit.objectData.TypeLunaPlayer);
        profiles.Add(unit.ID, icon);
    }
    private void OnEntityShown(object sender, GameEventArgs args) => Add(((ShowEntitySuccessEventArgs)args).Entity.Logic as UnitLogic);
    private void OnEntityHidden(object sender, GameEventArgs args) => Remove(((HideEntityCompleteEventArgs)args).EntityId);
    private void Remove(int id)
    {
        if (!profiles.TryGetValue(id, out var icon)) return;
        profiles.Remove(id);
        if (icon != null) Destroy(icon);
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        age += elapsed;
        float alpha = 1f - Mathf.Clamp01(age / 3f);
        var color = titleImage.color; color.a = alpha; titleImage.color = color;
        color = titleText.color; color.a = alpha; titleText.color = color;
        retired.Clear();
        float width = minimap.rect.width * 0.5f;
        foreach (var entry in profiles)
        {
            var unit = LunaSpawn.Find<UnitLogic>(entry.Key);
            if (unit == null || entry.Value == null) { retired.Add(entry.Key); continue; }
            float rate = Mathf.InverseLerp(Defined.AllyTowerPosition.x, Defined.EnemyTowerPosition.x, unit.CachedTransform.position.x);
            entry.Value.transform.localPosition = new Vector3(Mathf.Lerp(-width + 10f, width - 19f, rate), 3f, 0f);
            var distance = entry.Value.transform.Find("DistanceText");
            if (distance != null) distance.GetComponent<Text>().text = Mathf.RoundToInt(50f * rate) + "m";
        }
        foreach (int id in retired) Remove(id);
    }
    protected override void OnClose(bool isShutdown, object userData)
    {
        if (subscribed && events != null)
        {
            events.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnEntityShown);
            events.Unsubscribe(HideEntityCompleteEventArgs.EventId, OnEntityHidden);
        }
        subscribed = false;
        // UILuna owns generated child cleanup; Unity owns them during teardown.
        profiles.Clear(); retired.Clear();
        base.OnClose(isShutdown, userData);
    }
}
