using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity;
using Luna.Gameplay;

public class Lennon : HeroUnitLogic
{
    public GameObject skillEffect => View.GetPrefab("skillEffect");
    public AudioClip skillSound => View.GetSound("skillSound");
    public GameObject minion1 => View.GetPrefab("minion1");

    private int coefSkill_1;

    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private SkeletonAnimation ani;


    

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

        coefSkill_1 = 3;

        triggerSkill_1 = false;
        triggerSkill_2 = false; 
        
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
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

        triggerSkill_1 = true;
    }

    public override void ULT1()
    {
        STAR5();

        triggerSkill_2 = true;
    }

    public override void ULT2()
    {
        ULT1();
        /*
        ani.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "attack")
            {
                if (status.target != null)
                {
                    // 속도 30감소?
                }
            }
        };
        */
    }

    public override void Skill()
    {
        if (triggerSkill_2 == true)
            coefSkill_1 = Defined.CanProbability(75) ? 4 : 3;
        else
            coefSkill_1 = 3;

        if (skill_In_Action) return;
        skill_In_Action = true;
        StartCoroutine(NormalSkill());
    }

    IEnumerator NormalSkill()
    {
        int count = coefSkill_1;
        for (int i = 0; i < count; i++)
        {
            PlayOneSound(skillSound);
            SpawnSummon(minion1, transform.position + Vector3.left, data =>
            {
                if (triggerSkill_1) data.AttackMultiplier = data.ArmourMultiplier = data.HealthMultiplier = 1.1f;
            }, skillEffect);
            yield return new WaitForSeconds(0.2f);
        }
        skill_In_Action = false;
    }
}