using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;

/// <summary>Timed icon presentation owned by one native unit; combat effects retain their own exact deltas.</summary>
public class BuffImp
{
    private readonly Dictionary<eBuffType, int> icons = new Dictionary<eBuffType, int>();
    public GameObject UnitGameObject { get; set; }
    public System.Action OnEventBuffIcons { get; set; }
    public List<Buff> Buffs { get; set; } = new List<Buff>();
    public float IconHeight { get; set; }
    public void BuffIconInit()
    {
        BuffIconOff(); Buffs = new List<Buff>();
        IconHeight = 1.5f;
        var canvas = UnitGameObject != null ? UnitGameObject.transform.Find("Canvas") as RectTransform : null;
        if (canvas != null)
        {
            var corners = new Vector3[4]; canvas.GetWorldCorners(corners);
            IconHeight = Mathf.Max(corners[1].y, corners[2].y) - UnitGameObject.transform.position.y;
        }
    }
    public void AddBuff(eBuffType type, float duration)
    {
        var owner = UnitLogic.From(UnitGameObject);
        if (owner == null || duration <= 0f) return;
        foreach (var existing in Buffs)
            if (existing.BuffType == type) { existing.BuffLeftTime = Mathf.Max(existing.BuffLeftTime, duration); return; }
        var buff = new Buff(type, duration); Buffs.Add(buff);
        var prefabs = owner.GetLunaAssetBundle().BuffIcons;
        if ((int)type < prefabs.Count && prefabs[(int)type] != null)
        {
            var data = BuffIconData.Create(owner.ID, type, owner.Data.RoundId);
            icons[type] = LunaSpawn.Show<BuffIconLogic>(prefabs[(int)type], data);
        }
        OnEventBuffIcons?.Invoke();
    }
    public void Tick(float elapsed)
    {
        for (int i = Buffs.Count - 1; i >= 0; i--)
        {
            Buffs[i].BuffLeftTime -= elapsed;
            if (Buffs[i].BuffLeftTime > 0f) continue;
            if (icons.TryGetValue(Buffs[i].BuffType, out int id)) { LunaSpawn.Cancel(id); icons.Remove(Buffs[i].BuffType); }
            Buffs.RemoveAt(i); OnEventBuffIcons?.Invoke();
        }
    }
    public IEnumerator UpdateBuffs() { while (true) { Tick(Time.deltaTime); yield return null; } }
    public void BuffIconOff()
    {
        foreach (int id in icons.Values) LunaSpawn.Cancel(id);
        icons.Clear(); Buffs.Clear();
    }
}
