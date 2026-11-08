using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NormalSkeletonObjControl : EverydayDevup.Framework.LunaEntityBehaviour
{

    private SkeletonAnimation skeleton;
    private AudioSource audioSource;

    protected override void OnEntityInitialize()
    {
        audioSource = GetComponent<AudioSource>();

        if (transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>() != null)
        {
            skeleton = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

            skeleton.AnimationState.SetAnimation(0, "idle", true);
        }
    }
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (audioSource != null)
        {
            audioSource.volume = Defined.EffectVolumeSize;
        }
    }

    
    public void SetAnimationState(string name, bool loop)
    {
        skeleton.AnimationState.SetAnimation(0, name, loop);
    }

    public void DestroyThisObject(float time)
    {
        StartCoroutine(DestroyAfterTime(time));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        EverydayDevup.Framework.LunaEntities.Hide(gameObject);
    }
}
