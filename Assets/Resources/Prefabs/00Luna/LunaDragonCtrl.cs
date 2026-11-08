using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaDragonCtrl : EverydayDevup.Framework.LunaEntityBehaviour
{
    private SkeletonAnimation ani;

    protected override void OnEntityInitialize()
    {
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        
    }
}
