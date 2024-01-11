using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Sol_AttackAction : BtAction
{
    private GameObject thisObject;
    private GameObject projectTile;
    private SkeletonAnimation animation;

    //SkeletonAnimation.state.Event += OnUserDefinedEvent;
    //public void OnUserDefinedEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    //{
    //    if (e.Data.Name == "attack")
    //    {
    //        int a = 0;
    //    }
    //}


    public GameObject ProjectTile
    {
        get { return projectTile; }
        set { projectTile = value; }
    }

    public Sol_AttackAction(GameObject thisGameObject)
    {
        thisObject = thisGameObject;
        animation = thisObject.GetComponent<SkeletonAnimation>();
        animation.state.Event += AttackEventFunction;

    }

    public override void Initialize()
    {

    }

    public override void Terminate()
    {

    }

    public override eStatus Update()
    {
        if (thisObject.GetComponent<Status>().AttackTarget != null)
        {
            if (thisObject.GetComponent<SkeletonAnimation>().AnimationName != "attack" && thisObject.GetComponent<SkeletonAnimation>().AnimationName != "skill" && thisObject.GetComponent<SkeletonAnimation>().AnimationName != "damage")
            {
                thisObject.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "attack", false);
                thisObject.GetComponent<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", false, 2);
            }
        }

        return eStatus.Success;
    }

    public void AttackEventFunction(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "attack")
        {
            if (projectTile != null)
            {
                CreateProjectTile();
            }
        }
    }

    private void CreateProjectTile()
    {
        GameObject arrow 
            = PoolManager.SpawnObject(projectTile, thisObject.transform.position, Quaternion.identity);
        arrow.GetComponent<Arrow>().ShooterObject = thisObject;
        
        //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        //foreach (GameObject enemy in enemies)
        //{
        //    float distance = enemy.GetComponent<Transform>().position.x - thisObject.GetComponent<Transform>().position.x;

        //    if (0 < distance && thisObject.GetComponent<Status>().attackRange >= distance)
        //    {
        //        //Obj.GetComponent<SoldierAI>().target = enemy;
        //        //arrow.GetComponent<Arrow>().PathEnd = enemy.transform.position;
        //        arrow.GetComponent<Arrow>().AttackRange = thisObject.GetComponent<Status>().attackRange;
        //        break;
        //    }
        //}


    }
}
