using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Skeleton_ax_ally : MonoBehaviour 
{
    [SerializeField] private string skillMatter = "타겟 주위 범위 공격";
    [HideInInspector] public Status status;
    public GameObject effect;
    private GameObject tmpEffect;
    public float attackDamage = 200.0f;
    public float splashRange = 2.0f;
    private SkeletonAnimation skeletonAnimation;
    private GameObject attackPos;

    void Start()
    {
        status = GetComponent<Status>();
        skeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        attackPos = new GameObject();

        skeletonAnimation.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "attack")
            {
                if (status.AttackTarget != null)
                {
                    attackPos.transform.position = transform.position + new Vector3(status.stat.AttackRange, 0, 0);

                    Defined.EffectCreate(effect, attackPos, Defined.eEffectPosition.UNDERFEET);

                    IdentifyFunc.PickingUnits((unit) =>
                    {
                        if (IdentifyFunc.IsInDistance(attackPos, unit, splashRange))
                        {
                            unit.GetComponent<Status>().Attack(gameObject, attackDamage, true, true);
                        }
                    }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
                }
            }
        };
    }

    void Update()
    {

    }
}
