using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class DwarfAI : BaseAI
{
    [HideInInspector] public Status status;
    public GameObject projectile;

    public bool SkillTrigger { get; set; }

    void Start()
    {
        status = GetComponent<Status>();

        status.Ready();

        if (false == status.IsStatusDataLoadComplete)
            Destroy(gameObject);

        SkeletonAnimation = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();

        CreateBehaviorTreeAIState();
    }

    void Update()
    {
        AIState.Tick();

        StatusUpdate();

        if (transform.position.x > 1700)
        {
            Destroy(gameObject);
        }

        if(SkillTrigger == true)
        {
            Provoke();
        }
    }

    void Provoke()
    {
        float provokeDistance = 2.0f;

        IdentifyFunc.PickingUnits((unit) =>
        {
            if(IdentifyFunc.IsInDistanceRight(gameObject, unit, provokeDistance))
            {
                if (unit.GetComponent<Status>().AttackTarget != null && unit.GetComponent<Status>().AttackTarget != gameObject)
                {
                    unit.GetComponent<Status>().AttackTarget = gameObject;
                }
            }
        }, IdentifyFunc.IsEnemy, IdentifyFunc.IsNotTower);
    }

    void CreateBehaviorTreeAIState()
    {
        AIState = new BtRootNode();
        {
            // BT Root 셀렉터
            BtSelector mainSelector = new BtSelector();

            // 사망 시퀀스
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

            // 로비 및 ui 행동처리
            BtSequence lobbySequence = new BtSequence();
            {
                LobbyCondition robby = new LobbyCondition(gameObject);

                lobbySequence.AddChild(robby);

                mainSelector.AddChild(lobbySequence);
            }

            // 기본 시퀀스 - 딜레이상태 처리
            BtSequence idleSequence = new BtSequence();
            {
                idleCondition idle = new idleCondition(gameObject);

                idleSequence.AddChild(idle);

                mainSelector.AddChild(idleSequence);
            }

            // 스킬 시퀀스
            if (status.stat.Stamina != 0) // 스태미너가 0이면 스킬시퀀스 X
            {
                BtSequence skillSequence = new BtSequence();
                {
                    SkillCondition mpCheck = new SkillCondition(gameObject);
                    DwarfSkillAction skill = new DwarfSkillAction(gameObject);

                    skillSequence.AddChild(mpCheck);
                    skillSequence.AddChild(skill);

                    mainSelector.AddChild(skillSequence);
                }
            }

            // 이동 시퀀스
            if (status.stat.MoveSpeed != 0) // 이동속도가 0이면 이동시퀀스 X
            {
                BtSequence moveSequence = new BtSequence();
                {
                    MoveAction move = new MoveAction(gameObject);

                    moveSequence.AddChild(move);

                    mainSelector.AddChild(moveSequence);
                }
            }

            AIState.AddChild(mainSelector);
        }
    }

    void Finish()
    {
        PoolManager.ReleaseObject(this.gameObject);
    }

    private void StatusUpdate()
    {
        if (status.unitState.IsDeath == true)
        {
            Destroy(gameObject);
        }

        if (status.unitState.IsSkillActivaion == false)
        {
            status.stat.Mp += (100.0f / status.stat.Stamina) * Time.deltaTime;
        }
    }


}
