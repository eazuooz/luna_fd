using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scalet : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");

    private float skillDamage => status.stat.AttackDamage * 5f;
    private float coefSkill_1;
    private float coefSkill_2;
    private bool triggerSkill_1;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }

        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = true;

        coefSkill_1 = 1.0f;
        coefSkill_2 = 8.0f;
        triggerSkill_1 = false;
    }

    public override void STAR3()
    {
        STAR1();

        coefSkill_1 = 1.05f;
    }

    public override void STAR5()
    {
        STAR3();

        coefSkill_2 = 24.0f;
    }

    public override void ULT1()
    {
        STAR5();

        status.stat.SkillResistance += 0.2f;
        status.StatusAfterReady();
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_1 = true;
    }

    public override void Skill()
    {
        Vector3 attackPos;
        float splashRange;

        if (IdentifyFunc.IsAlive(status.AttackTarget))
        {
            attackPos = status.AttackTarget.transform.position;
            splashRange = 3.0f;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if (IdentifyFunc.IsInDistance(status.AttackTarget, unit, splashRange))
                {
                    StartCoroutine(SequanceSkill(UnitLogic.From(unit).ID));
                }
            }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
        }
    }

    IEnumerator SequanceSkill(int targetId)
    {
        yield return new WaitForSeconds(Random.Range(0,0.5f));

        var targetActor = UnitLogic.Find(targetId);
        if (targetActor != null && targetActor.stat.Hp > 0f)
        {
            Defined.EffectCreate(skillEffect, targetActor.gameObject);
            targetActor.Attack(gameObject, skillDamage * coefSkill_1 * coefSkill_2, true, true);

            if (triggerSkill_1 == true && Defined.CanProbability(80))
            {
                if (targetActor != null && targetActor.stat.Hp > 0f)
                { // юЗаж
                    targetActor.Curse();
                }
            }
        }
    }
}
