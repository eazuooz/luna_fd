using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SkillBomb : EverydayDevup.Framework.LunaEntityBehaviour
{
    private SkeletonAnimation ani;
    public GameObject effect;
    public float Range = 1.3f;
    public float Damage;

    // Start is called before the first frame update
    protected override void OnEntityInitialize()
    {
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        ani.AnimationState.SetAnimation(0, "normal", false);

        ani.state.Event += delegate (Spine.TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == "skill")
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

                EverydayDevup.Framework.LunaEntities.Spawn(effect, transform.Find("Sprite").position, effect.transform.rotation);

                foreach (GameObject enemy in enemies)
                {
                    if (enemy.GetComponent<Status>().UnitSide == eUnitSide.Enemy)
                    {
                        float distance = enemy.GetComponent<Transform>().position.x - transform.position.x;

                        if (distance >= -Range && Range >= distance)
                        {
                            enemy.GetComponent<Status>().Attack(gameObject, Damage, true, true);
                        }
                    }
                }
            }
        };

        ani.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "normal")
            {
                EverydayDevup.Framework.LunaEntities.Hide(gameObject);
            }
        };
    }
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        
    }
}
