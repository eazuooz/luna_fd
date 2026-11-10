using System.Collections;
using UnityEngine;
using Luna.Gameplay;

public class Rugani : EnemySkillLogic
{
    private int skillControlCounter;
    private bool stormRunning;
    public Vector3? StrikeMarker { get; private set; }

    public GameObject skillEffect_1 => View.GetPrefab("skillEffect_1");
    public GameObject skillEffect_2 => View.GetPrefab("skillEffect_2");
    public GameObject skillEffect_3 => View.GetPrefab("skillEffect_3");

    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
        skillControlCounter = 0;
        stormRunning = false;
        StrikeMarker = null;
    }

    protected override void ExecuteSkill()
    {
        skillControlCounter++;
        if (!stormRunning) StartCoroutine(Storm());

        if (skillControlCounter % 6 == 0)
        {
            Skill_3();
        }
        else if (skillControlCounter % 2 == 0)
        {
            Skill_2();
        }
        else
        {
            Skill_1();
        }
    }


    void Skill_1()
    {
        const float skillDamage = 80f;

        Defined.EffectCreate(skillEffect_1, gameObject, Defined.eEffectPosition.UNDERFEET);

        IdentifyFunc.PickingUnits((unit) =>
        {
            if ((IdentifyFunc.IsInDistanceLeft(gameObject, unit, 5.0f)))
            {
                UnitLogic.From(unit).TakePoisoning(skillDamage, 4.0f);
            }
        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_2()
    {
        const float skillDamage = 80f;

        IdentifyFunc.PickingUnit((unit) =>
        {
            Defined.EffectCreate(skillEffect_2, unit, Defined.eEffectPosition.TARGET);

            IdentifyFunc.PickingUnits((unit2) =>
            {
                if (IdentifyFunc.IsInDistance(unit, unit2, 2.5f))
                {
                    UnitLogic.From(unit2).TakePoisoning(skillDamage, 4.0f);
                }
            }, IdentifyFunc.IsAlly, unit2 => UnitLogic.From(unit2).Data.RoundId == Data.RoundId);

        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }

    void Skill_3()
    {
        const float skillDamage = 120f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(skillEffect_3, unit, Defined.eEffectPosition.TARGET);

            UnitLogic.From(unit).TakePoisoning(skillDamage, 5.0f);

        }, IdentifyFunc.IsAlly, unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
    }
    private IEnumerator Storm()
    {
        stormRunning = true;
        var cloud = View.GetPrefab("Rugani1.skillEffect_2");
        var bolt = View.GetPrefab("Rugani1.skillEffect_2_1");
        if (cloud != null) LunaEffects.Show(cloud, new Vector3(0f, 3f, 0f), roundId: Data.RoundId);
        // Mark one position per pulse. The party can move out before the strike,
        // and the storm ends before the next charged cast.
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1.1f);
            GameObject target = null;
            IdentifyFunc.PickingUnit(unit => target = unit, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId);
            if (target == null) continue;
            Vector3 strike = target.transform.position;
            StrikeMarker = strike;
            if (bolt != null) LunaEffects.Show(bolt, strike, roundId: Data.RoundId);
            yield return new WaitForSeconds(0.65f);
            IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).TakeStun(1.1f),
                IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower,
                unit => UnitLogic.From(unit).Data.RoundId == Data.RoundId,
                unit => IdentifyFunc.IsInDistance(strike, unit, 0.85f));
            StrikeMarker = null;
        }
        stormRunning = false;
    }
}
