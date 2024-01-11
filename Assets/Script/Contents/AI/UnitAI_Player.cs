using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class UnitAI_Player : BaseAI
{
    [HideInInspector] public Status status;
    public GameObject projectile;

    private void OnEnable()
    {
        if (status != null)
        {
            status.ResetRespwanData();
            ResetAnimation();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        status.IsStatusDataLoadComplete = false;
        status.unitState.IsSummonState = false;

        status.ReleaseData();
    }

    public void ResetAnimation()
    {
        if (SkeletonAnimation != null)
        {
            SkeletonAnimation.ClearState();
            SkeletonAnimation.skeletonDataAsset.Clear();
            SkeletonAnimation.AnimationState.SetAnimation(0, "idle", true);

            
        }
    }

    void Start()
    {
        status = GetComponent<Status>();
        
        SkeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        SkeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
        
        CreateBehaviorTreeAIState();

        InitProjectTile();
    }
    void Update()
    {
        SkeletonAnimation.timeScale = 0.6f;
        AIState.Tick();
        StatusUpdate();
    }

    void CreateBehaviorTreeAIState()
    {
        AIState = new BtRootNode();
        {
            // BT Root 셀렉터
            BtSelector mainSelector = new BtSelector();

            BtSequence deadSequence = new BtSequence();
            {
                DeathCondition deadCondition = new DeathCondition(gameObject);
                DeathAction deathAction = new DeathAction(gameObject);

                deadSequence.AddChild(deadCondition);
                deadSequence.AddChild(deathAction);

                mainSelector.AddChild(deadSequence);
            }

            // 피격 시퀀스
            BtSequence damagedSequence = new BtSequence();
            {
                DamagedCondition damagedCondition = new DamagedCondition(gameObject);
                DamagedAction damagedAction = new DamagedAction(gameObject);

                damagedSequence.AddChild(damagedCondition);
                damagedSequence.AddChild(damagedAction);

                mainSelector.AddChild(damagedSequence);
            }

            // 기본 시퀀스 - 딜레이상태 처리
            BtSequence idleSequence = new BtSequence();
            {
                idleCondition idle = new idleCondition(gameObject);

                idleSequence.AddChild(idle);
                mainSelector.AddChild(idleSequence);
            }

            // 공격 시퀀스
            if (status.stat.AttackRange != 0) // 사거리가 0이면 공격시퀀스 X
            {
                BtSequence attackSqeuence = new BtSequence();
                {
                    AttackCondition rangeCheck = new AttackCondition(gameObject);
                    attackSqeuence.AddChild(rangeCheck);

                    if (status.UnitAttackType == eUnitAttackType.Ranged)
                    {
                        AttackRangeAction attack = new AttackRangeAction(gameObject);
                        attackSqeuence.AddChild(attack);
                        attack.Projectile = projectile;
                    }
                    else
                    {
                        AttackMeleeAction attack = new AttackMeleeAction(gameObject);
                        attackSqeuence.AddChild(attack);
                    }

                    mainSelector.AddChild(attackSqeuence);
                }
            }

            AIState.AddChild(mainSelector);
        }
    }

    private void InitProjectTile()
    {
        //
        //SkeletonAnimation.
        if (SceneManager.GetActiveScene().name == "Game"
                && projectile != null)
        {
            projectile.transform.position = new Vector2(10000.0f, 1000.0f);

            if (status.UnitType == eUnitType.Hero)
            {
                PoolManager.WarmPool(projectile, 5);
            }
            else
            {
                PoolManager.WarmPool(projectile, 50);
            }
        }
    }

    void Finish()
    {
        PoolManager.ReleaseObject(this.gameObject);
    }

    private void StatusUpdate()
    {
        if (status.unitState.IsActionDelay == true)
        {
            status.stat.AttackDelayTime += Time.deltaTime;
        }

        if (status.stat.Mp <= 100.0f)
        {
            status.stat.Mp += (100.0f / status.stat.Stamina) * Time.deltaTime;
        }

        if (status.stat.AttackDelayTime >= 0)
        {
            status.stat.AttackDelayTime = -status.stat.AttackSpeed * Random.Range(0.9f, 1.1f);
            status.unitState.IsActionDelay = false;
        }
    }
}
