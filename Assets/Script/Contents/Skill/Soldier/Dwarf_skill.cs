using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Dwarf_skill : MonoBehaviour 
{
    [SerializeField] private string skillMatter = "방어태세";
    [HideInInspector] public Status status;
    SkeletonAnimation ani;
    private bool skillInit;

    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = true;
        skillInit = false;

        ani = gameObject.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    void Update()
    {
        if (status.unitState.IsSkillActivaion == true && skillInit == false)
        {
            skillInit = true;
            StartCoroutine("InstantSkill");
        }
    }
    
    IEnumerator InstantSkill()
    {
        ani.AnimationState.SetAnimation(0, "defense", true);

        while (status.AttackTarget != null && ani.AnimationName != "idle")
        {
            //InfiniteLoopDetector.Run();
            yield return null;
        }

        ani.AnimationState.SetAnimation(0, "idle", true);

        status.unitState.IsSkillActivaion = false;
        skillInit = false;
    }
}
