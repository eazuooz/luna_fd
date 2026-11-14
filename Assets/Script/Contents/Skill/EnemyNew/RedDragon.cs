using System.Collections;
using UnityEngine;
using Luna.Gameplay;

public class RedDragon : EnemySkillLogic
{
    private int skillControlCounter;
    private bool casting;
    public override bool HasAutomaticSkill => base.HasAutomaticSkill && !casting;
    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");
    public GameObject skillEffect_2 => View.GetPrefab("skillEffect_2");

    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
        skillControlCounter = 0;
        casting = false;
    }

    protected override void ExecuteSkill()
    {
        if (casting) return;
        skillControlCounter++;
        StartCoroutine(Flame(skillControlCounter % 4 == 0));
    }

    private IEnumerator Flame(bool barrage)
    {
        casting = true;
        int pulses = barrage ? 6 : 3;
        float damage = barrage ? 100f : 40f;
        var effect = barrage ? skillEffect_2 : skillEffect_1;
        for (int i = 0; i < pulses; i++)
        {
            GameObject target = null;
            IdentifyFunc.PickingUnit(unit => target = unit, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
            if (target == null) break;
            Vector3 point = target.transform.position;
            MarkAttack(point, 1f, 1.1f, barrage ? "화염 폭격" : "용의 불꽃");
            yield return new WaitForSeconds(1.1f);
            ClearAttackWarning();
            if (!LunaCombat.IsInCombat(this)) break;
            LunaEffects.Show(effect, point, roundId: Data.RoundId);
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).Attack(gameObject, damage, false, true),
                IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
                unit => IdentifyFunc.IsInDistance(point, unit, 1f));
            yield return new WaitForSeconds(0.2f);
        }
        ClearAttackWarning();
        casting = false;
        RecoverAfterSkill();
    }
}
