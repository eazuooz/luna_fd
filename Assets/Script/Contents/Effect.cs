using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class Effect : MonoBehaviour
{
    private SkeletonAnimation effect;
    private ParticleSystem particle;
    private AudioSource audioSource;

    public GameObject ChaseTargetObject { get; set; }
    public bool IsChase { get; set; }
    public Vector3 ChaseDirectionVector { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        InitializedAudioSource();
        InitializedEffectGameObject();
    }
    private void InitializedAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource)
        {
            Manager.Data.ObjectSounds.Add(audioSource);
            audioSource.volume = Defined.EffectVolumeSize;
        }
    }
    private void InitializedEffectGameObject()
    {
        if (IsSkeletonAnimation())
        {
            InitializeSkeletonAnimation();
        }
        else
        {
            InitializeParticleSystem();
        }
    }
    private bool IsSkeletonAnimation()
    {
        GameObject effectObject = transform.Find("Sprite").gameObject;
        SkeletonAnimation skeletonAnimation = effectObject.GetComponent<SkeletonAnimation>();

        if (skeletonAnimation)
            return true;

        return false;
    }
    private void InitializeSkeletonAnimation()
    {
        GameObject effectObject = transform.Find("Sprite").gameObject;
        effect = effectObject.GetComponent<SkeletonAnimation>();
        effect.AnimationState.SetAnimation(0, "normal", false);
        effect.state.Complete += CompleteSkeletonAnimationEffectEvent;
    }

    private void CompleteSkeletonAnimationEffectEvent(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == "normal")
        {
            DestoryEffectGameObject();
        }
    }
    private void InitializeParticleSystem()
    {
        GameObject effectObject = transform.Find("Sprite").gameObject;
        particle = effectObject.GetComponent<ParticleSystem>();

        if (particle)
            StartCoroutine(DestroyParticleEffect());
        else
            DestoryEffectGameObject();
    }

    void Update()
    {
        DoChasing();
    }

    private void DoChasing()
    {
        //if (ChaseTargetObject
        //    && ChaseTargetObject.activeSelf)
        //{
        //    transform.position = ChaseDirectionVector + ChaseTargetObject.transform.position;
        //}
        //else
        //    DestoryEffectGameObject();
        SetPositionChaseTarget();
    }
    private bool IsChaseTarget()
    {
        if (IsChase == false)
            return false;

        if (ChaseTargetObject == null || 
            ChaseTargetObject.activeSelf == false)
        {
            DestoryEffectGameObject();
            return false;
        }

        return true;
    }
    private void SetPositionChaseTarget()
    {
        if (IsChaseTarget())
        {
            Vector3 newPosition
                = ChaseDirectionVector + ChaseTargetObject.transform.position;
            transform.position = newPosition;
        }
    }

    private void DestoryEffectGameObject()
    {
        if (audioSource != null)
        {
            Manager.Data.ObjectSounds.Remove(audioSource);
        }

        Destroy(gameObject);
    }

    IEnumerator DestroyParticleEffect()
    {
        while (particle.IsAlive())
        {
            //InfiniteLoopDetector.Run();
            yield return null;
        }

        DestoryEffectGameObject();
        yield return null;
    }
}
