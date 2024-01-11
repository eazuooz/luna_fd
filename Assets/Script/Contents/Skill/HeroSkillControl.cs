using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class HeroSkillControl : MonoBehaviour
{
    protected Status status;

    protected bool skill_In_Action;
    protected bool skill_In_Move;
    protected virtual void Start()
    {
        status = GetComponent<Status>();
        StartCoroutine(ReadyUlt());
    }

    private void OnEnable()
    {
        if (status != null)
        {
            StartCoroutine(ReadyUlt());
        }
    }

    private void OnDisable()
    {
        if (status != null)
            AfterDeath();
    }

    IEnumerator ReadyUlt()
    {
        while (status.IsStatusDataLoadComplete == false) 
        {
            //InfiniteLoopDetector.Run();
            yield return null; 
        }

        switch (status.ultState)
        {
            case eUltState.ULT2:
                ULT2(); break;  // 2각
            case eUltState.ULT1:
                ULT1(); break;  // 1각
            case eUltState.STAR5:
                STAR5(); break; // 5성
            case eUltState.STAR4:
            case eUltState.STAR3:
                STAR3(); break; // 3성
            case eUltState.STAR2:
            case eUltState.STAR1:
            default:
                STAR1(); break; // 1성
        }

        skill_In_Action = false;
        skill_In_Move = false;
    }

    protected virtual void Update()
    {
        if (status.IsStatusDataLoadComplete)
        {
            if (skill_In_Action == false && status.unitState.IsSkillActivaion == true)
            {
                Skill();

                if(skill_In_Action == false)
                    status.unitState.IsSkillActivaion = false;
            }

            if (skill_In_Move)
            {
                SkillMove();
            }
        }
    }

    public virtual void Skill() { }
    public virtual void SkillMove() { }
    public virtual void STAR1() { }
    public virtual void STAR3() { }
    public virtual void STAR5() { }
    public virtual void ULT1() { }
    public virtual void ULT2() { }
    public virtual void AfterDeath() { }
}
