using Luna.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miu : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");

    private float skillDamage => status.stat.AttackDamage * 2f;
    private float coefSkill_1;

    

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
        status.unitState.IsTargetingSkill = false;

        coefSkill_1 = 1.0f;
    }


    public override void STAR3()
    {
        STAR1();

        status.coefficient.HP += 0.05f;
        status.HpAfterReady();
    }

    public override void STAR5()
    {
        STAR3();

    }

    public override void ULT1()
    {
        STAR5();

        coefSkill_1 = 2.5f;
    }

    public override void ULT2()
    {
        ULT1();

        status.stat.SkillAvoidance = 100f;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        float healingRange = 5.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, healingRange))
            {
                Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.TARGET, true);
                UnitLogic.From(unit).Heal(skillDamage * coefSkill_1);
                if (status.ultState >= eUltState.STAR5) UnitLogic.From(unit).CleanseCurse(10f);
            }
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }
}
