using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaCtrl_Slime : EverydayDevup.Framework.LunaEntityBehaviour
{
    private Status status;



    protected override void OnEntityInitialize()
    {
        status = GetComponent<Status>();
        SlimePassiveSkill(1.1f);
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        
    }

    public void SlimePassiveSkill(float coefficient)
    {
        if(status != null)
        {
            status.MultipleArmour(coefficient);
            //status.MultipleSkillResistance(coefficient);
        }
    }
}
