using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class MniMartinContol : EverydayDevup.Framework.LunaEntityBehaviour
{
    public float moveSpeed;
    public float range;
    public float damage;
    public bool isEnemy;
    public GameObject explosionEffect;

    private GameObject tmpEffect;

    private SkeletonAnimation ani;
    private GameObject target;
    private float counter;

    private bool stageStop;

    protected override void OnEntityInitialize()
    {

        counter = 0.0f;

        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        ani.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "death")
            {
                EverydayDevup.Framework.LunaEntities.Hide(gameObject);
            }
        };

        ani.state.Start += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "run")
            {
                StartCoroutine("MoveSequence");
            }

            if (trackEntry.Animation.Name == "death")
            {
                StopCoroutine("MoveSequence");

                tmpEffect = EverydayDevup.Framework.LunaEntities.Spawn(explosionEffect, transform.Find("AttackPos").position, explosionEffect.transform.rotation);

                if (target != null)
                    target.GetComponent<Status>().Attack(gameObject, damage, true, true);
            }
        };

        stageStop = true;
        ani.AnimationState.SetAnimation(0, "idle", true);
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        if (Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>() != null)
        {
            if(Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().isStageStop == true && stageStop == false)
            {
                stageStop = true;
                ani.AnimationState.SetAnimation(0, "idle", true);
            }
            else if(Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().isStageStop == false && stageStop == true)
            {
                stageStop = false;
                ani.AnimationState.SetAnimation(0, "run", true);
            }
        }

        if (target == null)
        {
            if (counter < 0.1f)
                counter += 1.0f * Time.deltaTime;
            else
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

                foreach (GameObject enemy in enemies)
                {
                    if ( isEnemy )
                    {
                        if (enemy.GetComponent<Status>().UnitSide == eUnitSide.Allied)
                        {
                            float distance = enemy.GetComponent<Transform>().position.x - transform.position.x;

                            if (distance >= -range && range >= distance)
                            {
                                target = enemy;
                                ani.AnimationState.SetAnimation(0, "death", false);
                                return;
                            }
                        }
                    }
                    else 
                    {
                        if (enemy.GetComponent<Status>().UnitSide == eUnitSide.Enemy)
                        {
                            float distance = enemy.GetComponent<Transform>().position.x - transform.position.x;

                            if (distance >= -range && range >= distance)
                            {
                                target = enemy;
                                ani.AnimationState.SetAnimation(0, "death", false);
                                return;
                            }
                        }
                    }


                }

                counter = 0;
            }
        }
    }

    IEnumerator MoveSequence()
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return null;

            if (isEnemy)
            {
                transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
            
        }
    }
}
