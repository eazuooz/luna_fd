using UnityEngine;
using Spine.Unity;
using Luna.Gameplay;

public sealed class SkillBombData : LunaEntityData
{
    public float Damage;
    public float Range = 1.3f;
    public eUnitSide Side;
    public GameObject ImpactEffect;
    public float Lifetime = 10f;
    public static SkillBombData Create(UnitLogic owner, Vector3 position, float damage)
    {
        var data = LunaEntityData.Create<SkillBombData>();
        data.OwnerId = owner.ID; data.RoundId = owner.Data.RoundId;
        data.Position = position; data.Damage = damage; data.Side = owner.UnitSide;
        GameObject prefab = owner.View.GetPrefab("skillEffect");
        UnitView view = prefab != null ? prefab.GetComponent<UnitView>() : null;
        data.ImpactEffect = view != null ? view.GetPrefab("effect") : null;
        return data;
    }
    public override void Clear() { base.Clear(); Damage = 0f; Range = 1.3f; Side = default; ImpactEffect = null; Lifetime = 10f; }
}
public class SkillBomb : EntityLogicBase
{
    private SkeletonAnimation animation;
    private SkillBombData data;
    private float elapsed;
    private bool activated;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        animation = GetComponentInChildren<SkeletonAnimation>(true);
        if (animation == null) throw new System.InvalidOperationException("SkillBomb requires its authored Spine animation.");
        animation.AnimationState.Event += OnAnimationEvent;
        animation.AnimationState.Complete += OnComplete;
    }
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        data = (SkillBombData)userData; elapsed = 0f; activated = false;
        animation.AnimationState.SetAnimation(0, "normal", false);
    }
    protected override void OnUpdate(float dt, float realDt)
    {
        base.OnUpdate(dt, realDt);
        if (data == null || IsHiding) return;
        if ((elapsed += dt) > data.Lifetime || UnitLogic.Find(data.OwnerId) == null) SafeHide();
    }
    private void OnAnimationEvent(Spine.TrackEntry track, Spine.Event e)
    {
        if (data == null || IsHiding || activated || e.Data.Name != "skill") return;
        var owner = UnitLogic.Find(data.OwnerId);
        if (owner == null) { SafeHide(); return; }
        activated = true;
        if (data.ImpactEffect != null) LunaSpawn.Show<EffectLogic>(data.ImpactEffect, EffectData.Create(transform.position, 0, false, data.RoundId));
        int ownerId = data.OwnerId;
        IdentifyFunc.PickingUnits(unit =>
        {
            var target = UnitLogic.From(unit);
            if (target == null || target.UnitSide == data.Side || !IdentifyFunc.IsInDistance(transform.position, unit, data.Range)) return;
            var currentOwner = UnitLogic.Find(ownerId);
            if (currentOwner != null) target.Attack(currentOwner.gameObject, data.Damage, true, true);
        }, IdentifyFunc.IsNotTower);
    }
    private void OnComplete(Spine.TrackEntry track) { if (data != null && !IsHiding && track.Animation.Name == "normal") SafeHide(); }
    protected override void OnHide(bool shutdown, object userData) { data = null; activated = true; base.OnHide(shutdown, userData); }
}
