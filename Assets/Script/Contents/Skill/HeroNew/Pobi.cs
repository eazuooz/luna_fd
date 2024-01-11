using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity;
using EverydayDevup.Framework;

public class Pobi : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;
    public GameObject minion1;

    private int coefSkill_1;

    private bool triggerSkill_1;
    private bool triggerSkill_2;

    private SkeletonAnimation ani;


    protected override void Start()
    {
        base.Start();

        if (SceneManager.GetActiveScene().name == "Game")
        {
            InitializeWarmPool();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (status.unitState.IsLobbyMove == true)
        {
            return;
        }
    }

    private void InitializeWarmPool()
    {
        PoolManager.WarmPool(minion1, 20);
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

        StartCoroutine(NormalSkill());
    }

    IEnumerator NormalSkill()
    {

        for (int i = 0; i < coefSkill_1; i++)
        {
            status.PlayOneSound(skillSound);

            GameObject tmpMinion = PoolManager.Instance.spawnObject(minion1, GetComponent<Transform>().position + new Vector3(-1.0f, 0, 0.0f), minion1.transform.rotation);

            Defined.EffectCreate(skillEffect, tmpMinion, Defined.eEffectPosition.UNDERFEET, true);

            if(triggerSkill_1 == true)
            {
                tmpMinion.GetComponent<Status>().coefficient.AttackDamage += 0.1f;
                tmpMinion.GetComponent<Status>().coefficient.Armour += 0.1f;
                tmpMinion.GetComponent<Status>().coefficient.HP += 0.1f;
                tmpMinion.GetComponent<Status>().StatusAfterReady();
                tmpMinion.GetComponent<Status>().HpAfterReady();
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}