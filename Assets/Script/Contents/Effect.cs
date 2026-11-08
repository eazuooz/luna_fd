using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class Effect : EverydayDevup.Framework.LunaEntityBehaviour
{
    private SkeletonAnimation effect;
    private ParticleSystem[] particles;
    private AudioSource audioSource;

    public GameObject ChaseTargetObject { get; set; }
    public bool IsChase { get; set; }
    public Vector3 ChaseDirectionVector { get; set; }

    // Start is called before the first frame update
    protected override void OnEntityInitialize()
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
        GameObject effectObject = transform.Find("Sprite") != null ? transform.Find("Sprite").gameObject : gameObject;
        SkeletonAnimation skeletonAnimation = effectObject.GetComponent<SkeletonAnimation>();

        if (skeletonAnimation)
            return true;

        return false;
    }
    private void InitializeSkeletonAnimation()
    {
        GameObject effectObject = transform.Find("Sprite") != null ? transform.Find("Sprite").gameObject : gameObject;
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
        GameObject effectObject = transform.Find("Sprite") != null ? transform.Find("Sprite").gameObject : gameObject;
        // Authored auras can contain several sibling particle systems instead
        // of a root system or a child named Sprite. Preserve every system's tail.
        particles = effectObject.GetComponentsInChildren<ParticleSystem>();

        if (particles.Length > 0)
            StartCoroutine(DestroyParticleEffect());
        else
            DestoryEffectGameObject();
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
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
        EverydayDevup.Framework.LunaEntities.Hide(gameObject);
    }

    protected override void OnEntityHide()
    {
        if (audioSource != null && Game.Instance != null)
            Manager.Data.ObjectSounds.Remove(audioSource);
        if (effect != null && effect.AnimationState != null)
            effect.AnimationState.Complete -= CompleteSkeletonAnimationEffectEvent;
    }

    private bool AreParticlesAlive()
    {
        foreach (var particle in particles)
            if (particle != null && particle.IsAlive(true)) return true;
        return false;
    }

    IEnumerator DestroyParticleEffect()
    {
        while (AreParticlesAlive())
        {
            //InfiniteLoopDetector.Run();
            yield return null;
        }

        DestoryEffectGameObject();
        yield return null;
    }
}
