using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class BaseAI : MonoBehaviour
{
    private BtRootNode mAIState;
    private SkeletonAnimation mSkeletonAnimation;

    protected bool isWarm = false;
    public bool DebugOn = false;
    protected BtRootNode AIState
    {
        get { return mAIState; }
        set { mAIState = value; }
    }
    protected SkeletonAnimation SkeletonAnimation
    {
        get { return mSkeletonAnimation; }
        set { mSkeletonAnimation = value; }
    }

    
    public void InitializeBaseSkeletonAnimation()
    {
        SkeletonAnimation 
            = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    public void SetAnimation(string animationName, bool loop = false)
    {
        SkeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    }
    public void ResetAnimation()
    {
        if (SkeletonAnimation != null)
        {
            SkeletonAnimation.ClearState();
            SkeletonAnimation.skeletonDataAsset.Clear();
            SkeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
        }
    }

}
