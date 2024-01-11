using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;


public class SpineRenderer
{
    public class SpineInfo 
    {
        public SkeletonAnimation skeletonAnimation = null;
        public Transform animationTransform = null;
        public Spine.AnimationState animationState = null;
        public Skeleton skeleton = null;
        public SkeletonData skeletonData = null;
        public TrackEntry trackEntry = null;
    }

    public delegate void DelegateEvent(string eventName, int value);
    public event DelegateEvent eventMain;

    public delegate void DelegateComplete(string animationName);
    public event DelegateComplete eventComplete;

    private SpineInfo spineInfo = new SpineInfo();

    public bool Init(GameObject gameObject)
    {
        if (gameObject == null)
            return false;

        spineInfo.skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
        if (spineInfo.skeletonAnimation == null)
            return false;

        spineInfo.animationTransform = spineInfo.skeletonAnimation.transform;
        if (spineInfo.animationTransform == null)
            return false;

        spineInfo.animationState = spineInfo.skeletonAnimation.state;
        if (spineInfo.animationState == null)
            return false;

        spineInfo.skeleton = spineInfo.skeletonAnimation.skeleton;
        if (spineInfo.skeleton == null)
            return false;

        spineInfo.skeletonData = spineInfo.skeleton.Data;
        if (spineInfo.skeletonData == null)
            return false;


        spineInfo.animationState.Event += OnEvent;
        //spineInfo.animationState.Start += OnStart;
        spineInfo.animationState.Complete += OnComplete;

        return true;
    }


    private void OnEvent(TrackEntry trackEntry, Spine.Event spineEvent)
    {
        if (eventMain == null) return;
        if (null == spineEvent) return;
        if (trackEntry != spineInfo.trackEntry) return;

        EventData eventData = spineEvent.Data;
        if (null == eventData) return;

        eventMain(eventData.Name, spineEvent.Int);
    }

    private void OnComplete(TrackEntry trackEntry)
    {
        if (null == eventComplete) return;
        if (null == trackEntry) return;
        if (trackEntry != spineInfo.trackEntry) return;

        Spine.Animation animation = trackEntry.Animation;
        if (null == animation) return;

        eventComplete(animation.Name);
        spineInfo.trackEntry = null;
    }
}
