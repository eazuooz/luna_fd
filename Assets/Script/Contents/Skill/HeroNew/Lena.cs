using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Spine.Unity;
using EverydayDevup.Framework;

public class Lena : HeroSkillControl
{
    public GameObject skillEffect;
    public AudioClip skillSound;
    public GameObject NormalMinion;
    public GameObject SkillMinion;
    public GameObject UltMinion;

    private bool triggerSkill_1;
    private bool triggerSkill_2;
    private bool triggerSkill_3;

    private SkeletonAnimation ani;
    private float counter;

    private GameObject curMinion;
    private bool triggerSkill_In_Action;

    protected override void Start()
    {
        base.Start();

        counter = 0.0f;

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

        if (triggerSkill_In_Action == false)
        {
            counter += Time.deltaTime;

            if (triggerSkill_1 == true && counter > 12.0f)
            {
                counter = 0.0f;
                UltSkill();
            }
        }
        else
        {
            if (curMinion == null || curMinion.activeSelf == false)
                triggerSkill_In_Action = false;
        }
    }
    
    private void InitializeWarmPool()
    {
        PoolManager.WarmPool(NormalMinion, 20);
        PoolManager.WarmPool(SkillMinion, 20);
        PoolManager.WarmPool(UltMinion, 5);
    }

    public override void STAR1()
    {
        status.unitState.IsTargetingSkill = false;

        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        ani.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "attack")
            {
                NormalSkill();
            }
        };
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

        triggerSkill_3 = true;
    }

    public override void Skill()
    {
        status.PlayOneSound(skillSound);

        GameObject Slime = PoolManager.Instance.spawnObject(SkillMinion, GetComponent<Transform>().position + new Vector3(-0.5f, 0, 0.0f), SkillMinion.transform.rotation);

        Defined.EffectCreate(skillEffect, Slime, Defined.eEffectPosition.UNDERFEET, true);

        if (triggerSkill_2 == true && Defined.CanProbability(65))
        {
            Slime.GetComponent<Status>().TakeBerserkMode(5.0f);
        }
    }

    void NormalSkill()
    {
        status.PlayOneSound(skillSound);

        GameObject Slime = PoolManager.Instance.spawnObject(NormalMinion, GetComponent<Transform>().position + new Vector3(-0.5f, 0, 0.0f), NormalMinion.transform.rotation);

        Defined.EffectCreate(skillEffect, Slime, Defined.eEffectPosition.UNDERFEET, true);

        if (triggerSkill_1 == true && Defined.CanProbability(65))
        {
            Slime.GetComponent<Status>().coefficient.AttackDamage += 0.1f;
            Slime.GetComponent<Status>().StatusAfterReady();
        }
    }

    void UltSkill()
    {
        status.PlayOneSound(skillSound);

        GameObject Slime = PoolManager.Instance.spawnObject(UltMinion, GetComponent<Transform>().position + new Vector3(-0.5f, 0, 0.0f), UltMinion.transform.rotation);

        Defined.EffectCreate(skillEffect, Slime, Defined.eEffectPosition.UNDERFEET, true);

        curMinion = Slime;
        triggerSkill_In_Action = true;
    }
}