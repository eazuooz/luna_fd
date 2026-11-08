using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lene : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");

    private float skillDamage => status.stat.AttackDamage * 3f;
    private const float AllyProcChance = 40.0f;
    private float selfProcChance;
    private float coefSkill_2;
    private bool triggerSkill_2;

    private float skillCount;

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        if (Data == null || IsHiding) return;
        if (!LunaCombat.IsInCombat(this)) { base.OnUpdate(elapsed, realElapsed); return; }

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }


        if (triggerSkill_2 == true)
        {
            skillCount -= Time.deltaTime;

            if (skillCount < 0.0f)
            {
                newSkill();
            }
        }
        base.OnUpdate(elapsed, realElapsed);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

        selfProcChance = AllyProcChance;
        coefSkill_2 = 5.0f;
        triggerSkill_2 = false;

        skillCount = 3.0f;
    }

    public override void STAR3()
    {
        STAR1();

        status.coefficient.AttackDamage += 0.05f;
        status.StatusAfterReady();
    }

    public override void STAR5()
    {
        STAR3();
        selfProcChance += 40.0f;
    }

    public override void ULT1()
    {
        STAR5();

        selfProcChance += 20.0f;
    }

    public override void ULT2()
    {
        ULT1();

        triggerSkill_2 = true;
    }

    public override void Skill()
    {
        IdentifyFunc.PickingUnits(unit =>
        {
            float chance = unit == gameObject ? selfProcChance : AllyProcChance;
            if (Defined.CanProbability(chance)) UnitLogic.From(unit).TakeBerserkMode(coefSkill_2);
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsHero, unit => IdentifyFunc.IsInDistance(gameObject, unit, 10f));
    }

    void newSkill()
    {
        IdentifyFunc.PickingUnit(target =>
        {
            status.PlayOneSound(skillSound);
            Defined.EffectCreate(skillEffect, target, Defined.eEffectPosition.UNDERFEET);
            StartCoroutine(DelaySkill(UnitLogic.From(target).ID));
            skillCount = 3f;
        }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower, unit => IdentifyFunc.IsInDistance(gameObject, unit, 3.4f));
    }

    IEnumerator DelaySkill(int targetId)
    {
        yield return new WaitForSeconds(0.36f);

        var target = UnitLogic.Find(targetId);
        if (target != null && target.stat.Hp > 0f)
        {
            target.Attack(gameObject, skillDamage * 2, true, true);
        }
    }
}
