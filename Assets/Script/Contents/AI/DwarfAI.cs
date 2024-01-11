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
            // BT Root ������
            BtSelector mainSelector = new BtSelector();

            // ��� ������
            BtSequence deadSequence = new BtSequence();
            {
                DeathCondition deadCondition = new DeathCondition(gameObject);
                DeathAction deathAction = new DeathAction(gameObject);

                deadSequence.AddChild(deadCondition);
                deadSequence.AddChild(deathAction);

                mainSelector.AddChild(deadSequence);
            }

            // �ǰ� ������
            BtSequence damagedSequence = new BtSequence();
            {
                DamagedCondition damagedCondition = new DamagedCondition(gameObject);
                DamagedAction damagedAction = new DamagedAction(gameObject);

                damagedSequence.AddChild(damagedCondition);
                damagedSequence.AddChild(damagedAction);

                mainSelector.AddChild(damagedSequence);
            }

            // �κ� �� ui �ൿó��
            BtSequence lobbySequence = new BtSequence();
            {
                LobbyCondition robby = new LobbyCondition(gameObject);

                lobbySequence.AddChild(robby);

                mainSelector.AddChild(lobbySequence);
            }

            // �⺻ ������ - �����̻��� ó��
            BtSequence idleSequence = new BtSequence();
            {
                idleCondition idle = new idleCondition(gameObject);

                idleSequence.AddChild(idle);

                mainSelector.AddChild(idleSequence);
            }

            // ��ų ������
            if (status.stat.Stamina != 0) // ���¹̳ʰ� 0�̸� ��ų������ X
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

            // �̵� ������
            if (status.stat.MoveSpeed != 0) // �̵��ӵ��� 0�̸� �̵������� X
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
