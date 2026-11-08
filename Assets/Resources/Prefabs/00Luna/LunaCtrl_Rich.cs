using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaCtrl_Rich : EverydayDevup.Framework.LunaEntityBehaviour
{
    private Status status;



    protected override void OnEntityInitialize()
    {
        Defined.SkillTrigger_Goldy *= 1.1f;
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        
    }
}
