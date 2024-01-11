using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PoisonSlime : MonoBehaviour 
{
    [HideInInspector] public Status status;
    public float attackDamage = 100.0f;
    private SkeletonAnimation skeletonAnimation;

    void Start()
    {
        status = GetComponent<Status>();
        skeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        skeletonAnimation.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "attack")
            {
                if (status.AttackTarget != null && Defined.CanProbability(100))
                {
                    status.AttackTarget.GetComponent<Status>().TakePoisoning(attackDamage, 5.0f);
                }
            }
        };
    }

    void Update()
    {

    }
}
