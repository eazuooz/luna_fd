using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class MartinControl : MonoBehaviour
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

    void Start()
    {
        counter = 0.0f;

        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        ani.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "attack")
            {

                tmpEffect = GameObject.Instantiate(explosionEffect, transform.Find("AttackPos").position, explosionEffect.transform.rotation);

                if (target != null)
                    target.GetComponent<Status>().Attack(gameObject, damage, true, true);

                Destroy(gameObject);
            }
        };

        ani.state.Start += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "idle")
            {
                StartCoroutine("MoveSequence");
            }

            if (trackEntry.Animation.Name == "attack")
            {
                StopCoroutine("MoveSequence");
            }
        };

        StartCoroutine(HeightControl());

        ani.AnimationState.SetAnimation(0, "idle", false);
    }

    void Update()
    {
        if (target == null)
        {
            if (counter < 0.1f)
                counter += 1.0f * Time.deltaTime;
            else
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Unit");

                foreach (GameObject enemy in enemies)
                {
                    if (isEnemy)
                    {

                        if (enemy.GetComponent<Status>().UnitSide == eUnitSide.Allied)
                        {
                            float distance = enemy.GetComponent<Transform>().position.x - transform.position.x;

                            if (distance >= -range && range >= distance)
                            {
                                target = enemy;
                                ani.AnimationState.SetAnimation(0, "attack", false);
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
                                ani.AnimationState.SetAnimation(0, "attack", false);
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

    IEnumerator HeightControl()
    {
        bool targetFlag;

        while(true)
        {
            //InfiniteLoopDetector.Run();
            targetFlag = false;

            IdentifyFunc.PickingUnits((unit) =>
            {
                if(IdentifyFunc.IsInDistanceLeft(gameObject,unit,2.0f))
                {
                    targetFlag = true;
                }
            }, IdentifyFunc.IsAlly);

            if (targetFlag == false)
            {
                if (transform.position.y < 2.5f)
                    transform.Translate(Vector3.up * moveSpeed * 1.5f * Time.deltaTime);
                else
                    transform.position = new Vector3(transform.position.x, 3.0f, transform.position.z);
            }
            else
            {
                if (transform.position.y > 2.5f)
                    transform.Translate(Vector3.down * moveSpeed * 1.5f * Time.deltaTime);
                else
                    transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
            }

            yield return null;
        }
    }
}
