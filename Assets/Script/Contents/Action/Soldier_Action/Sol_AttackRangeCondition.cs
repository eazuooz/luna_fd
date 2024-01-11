using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;


public class Sol_AttackRangeCondition : BtCondition
{
    GameObject Obj;
    float attackRange;

    public Sol_AttackRangeCondition(GameObject gmObj)
    {
        Obj = gmObj;
        attackRange = gmObj.GetComponent<Status>().stat.AttackRange;
    }

    public override eStatus Update()
    {
        if (Obj.GetComponent<Status>().AttackTarget != null)
            return eStatus.Success;

        if (Obj.GetComponent<Status>().AttackTarget == null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");

            foreach (GameObject enemy in enemies)
            {
                float distance = enemy.GetComponent<Transform>().position.x - Obj.GetComponent<Transform>().position.x;

                if (0 < distance && attackRange >= distance)
                {
                    Obj.GetComponent<Status>().AttackTarget = enemy;
                    return eStatus.Success;
                }
            }
        }

        return eStatus.Failure;
    }
}
