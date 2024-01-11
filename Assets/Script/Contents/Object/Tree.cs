using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Tree : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;
    private float delayCon;
    private float delayTmp;
    private bool delay;
    void Start()
    {
        skeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        delayCon = 0.0f;
        delayTmp = Random.Range(0, 0.1f);
        delay = false;
    }

    // Update is called once per frame
    void Update()
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
