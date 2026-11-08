using System.Collections;
using UnityEngine;
using Luna.Gameplay;

public class MartinKing : EnemySkillLogic
{
    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");
    private bool isCharge;
    public bool IsCharging => isCharge;
    public override bool HasAutomaticSkill => !isCharge && base.HasAutomaticSkill;
    protected override void ConfigureUnitForSpawn()
    {
        base.ConfigureUnitForSpawn();
        isCharge = false;
        unitState.IsTargetingSkill = false;
        transform.Find("Sprite").gameObject.SetActive(true);
    }
    protected override void ExecuteSkill()
    {
        if (isCharge) return;
        isCharge = true;
        if (UnitSide == eUnitSide.Enemy) StartCoroutine(Charge());
    }
    private IEnumerator Charge()
    {
        SetAnimation("skill", true);
        yield return new WaitForSeconds(40f);
        SetAnimation("boom ready", true);
        yield return new WaitForSeconds(20f);
        SetAnimation("boom");
    }
    protected override void OnAnimationComplete(Spine.TrackEntry track)
    {
        if (track.Animation.Name == "boom") StartCoroutine(Boom());
        else base.OnAnimationComplete(track);
    }
    private IEnumerator Boom()
    {
        transform.Find("Sprite").gameObject.SetActive(false);
        for (int i = 0; i < 50; i++)
        {
            LunaEffects.Show(skillEffect_1, transform.position + new Vector3(Random.Range(-20f, 0f), Random.Range(-0.5f, 2f), 0f), roundId: Data.RoundId);
            yield return new WaitForSeconds(0.1f);
        }
        foreach (var unit in UnitLogic.ActiveUnits)
            if (unit.Data != null && unit.Data.RoundId == Data.RoundId && unit.UnitSide == eUnitSide.Allied && unit.UnitType == eUnitType.Tower)
                unit.stat.Hp = 0f;
    }
}
