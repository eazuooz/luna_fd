using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Tree : EverydayDevup.Framework.LunaEntityBehaviour
{
    private SkeletonAnimation skeletonAnimation;
    private float delayCon;
    private float delayTmp;
    private bool delay;
    protected override void OnEntityInitialize()
    {
        skeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        delayCon = 0.0f;
        delayTmp = Random.Range(0, 0.1f);
        delay = false;
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (delay == false)
        {
            if (delayCon < delayTmp)
                delayCon += 1.0f * Time.deltaTime;
            else
            {
                skeletonAnimation.AnimationState.SetAnimation(0, "Idle", true);
                delay = true;
            }
        }
    }
}
