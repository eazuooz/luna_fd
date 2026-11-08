using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using EverydayDevup.Framework;

using Spine.Unity;

using UnityEngine.SceneManagement;





public class UnitAI : BaseAI

{

    [HideInInspector] public Status status;

    public GameObject projectile;



    private bool isNearby;

    IEnumerator tirggerProcess;



    private GameObject MiniEffectToPlayer;



    protected override void OnEntityInitialize()

    {

        InitializeStatus();

        InitializeSkeletonAnimation();

        InitializeBehaviorTreeAIState();

        InitializeProjectTile();

        InitializeBuffImp(BuffIconUpdate);

    }



    private void InitializeStatus()

    {

        status = GetComponent<Status>();

        status.Ready();



        if (false == status.IsStatusDataLoadComplete)

            LunaEntities.Hide(gameObject);

    }



    private void InitializeSkeletonAnimation()

    {

        InitializeBaseSkeletonAnimation();

        SetAnimation("idle", true);

    }



    private void InitializeProjectTile()

    {

        bool isScenes

            = Game.Instance.lunaSceneManager.IsScenes("Game", "Dungeon");



        if (isScenes == false)

            return;



        if (projectile != null)

        {

            // Warming registers the prefab; never reposition the source asset.



            if (status.UnitType == eUnitType.Hero)

                PoolManager.WarmPool(projectile, 5);

            else

                PoolManager.WarmPool(projectile, 50);

        }

    }



    protected override void OnEntityShow()

    {

        if (status != null)

        {

            status.ResetRespwanData();

            ResetAnimation();

        }

    }



    protected override void OnEntityHide()

    {

        StopAllCoroutines();

        if (status == null) return;

        status.IsStatusDataLoadComplete = false;

        status.unitState.IsSummonState = false;



        status.ReleaseData();



        if (status.UnitType == eUnitType.Hero)

        {

            GameObject uiProgressBar

            = LunaUtility.FindWithTagGameObject("UIProgressBar");

            if (uiProgressBar != null)

            {

                UIProgressBar comp = uiProgressBar.GetComponent<UIProgressBar>();

                comp.ReleaseMinimapObject(gameObject);

            }

        }

    }



    protected override void OnEntityUpdate(float elapsed, float realElapsed)

    {

        if (status == null || !status.IsStatusDataLoadComplete || AIState == null) return;
        AIState.Tick();



        ConfirmStatus();



        if (tag == "Unit" &&

            IdentifyFunc.IsAlly(gameObject) &&

            IdentifyFunc.IsSoldier(gameObject))

        {

            PlayerEffectCheck();

        }



        if (status.UnitSide == eUnitSide.Enemy) //일단 적군만

        {

            CollisionCheck();

        }

    }



    void InitializeBehaviorTreeAIState()

    {

        AIState = new BtRootNode();



        // BT Root Main Sellector

        BtSelector mainSelector = new BtSelector();



        // 사망 시퀀스

        BtSequence deadSequence = new BtSequence();

        DeathCondition deadCondition = new DeathCondition(gameObject);

        DeathAction deathAction = new DeathAction(gameObject);

        deadSequence.AddChild(deadCondition);

        deadSequence.AddChild(deathAction);

        mainSelector.AddChild(deadSequence);



        // 피격 시퀀스

        BtSequence damagedSequence = new BtSequence();

        DamagedCondition damagedCondition = new DamagedCondition(gameObject);

        DamagedAction damagedAction = new DamagedAction(gameObject);

        damagedSequence.AddChild(damagedCondition);

        damagedSequence.AddChild(damagedAction);

        mainSelector.AddChild(damagedSequence);



        // 로비 및 ui 행동처리

        BtSequence lobbySequence = new BtSequence();

        LobbyCondition robby = new LobbyCondition(gameObject);

        lobbySequence.AddChild(robby);

        mainSelector.AddChild(lobbySequence);



        // 기본 시퀀스 - 딜레이상태 처리

        BtSequence idleSequence = new BtSequence();

        idleCondition idle = new idleCondition(gameObject);

        idleSequence.AddChild(idle);

        mainSelector.AddChild(idleSequence);



        // 스킬 시퀀스

        if (status.stat.Stamina != 0) // 스태미너가 0이면 스킬시퀀스 X

        {

            BtSequence skillSequence = new BtSequence();

            SkillCondition mpCheck = new SkillCondition(gameObject);

            SkillAction skill = new SkillAction(gameObject);

            skillSequence.AddChild(mpCheck);

            skillSequence.AddChild(skill);

            mainSelector.AddChild(skillSequence);

        }



        // 공격 시퀀스

        if (status.stat.AttackRange != 0) // 사거리가 0이면 공격시퀀스 X

        {

            BtSequence attackSqeuence = new BtSequence();

            AttackCondition rangeCheck = new AttackCondition(gameObject);

            attackSqeuence.AddChild(rangeCheck);

            if (status.UnitAttackType == eUnitAttackType.Ranged == true)

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



        // 이동 시퀀스

        if (status.stat.MoveSpeed != 0) // 이동속도가 0이면 이동시퀀스 X

        {

            BtSequence moveSequence = new BtSequence();

            MoveAction move = new MoveAction(gameObject);

            moveSequence.AddChild(move);

            mainSelector.AddChild(moveSequence);

        }



        AIState.AddChild(mainSelector);

    }



    private void ConfirmDeath()

    {

        if (status.unitState.IsDeath == true)

        {

            // hero soldier enemy 일경우 객체반납

            // 그렇지 않은 경우 메모리 해제

            if (status.UnitType == eUnitType.Hero

                || status.UnitType == eUnitType.Soldier)

            {

                PoolManager.ReleaseObject(this.gameObject);

            }

            else

            {

                LunaEntities.Hide(gameObject);

            }

        }

    }



    private void ConfirmSkill()

    {

        // skillOn이 켜지면 스킬을 시전하고 mp를 0으로 합니다.

        if (status.unitState.IsSkillActivaion == false && status.stat.Stamina > 0f)

        {

            status.stat.Mp += (100.0f / status.stat.Stamina) * Time.deltaTime;

        }

    }



    private void ConfirmActionDelay()

    {

        // action변수(행동이 끝나고 대기 상태)가 켜지면 쿨타임동안 다른 행동이 일어나지 않습니다.

        if (status.unitState.IsActionDelay == true)

        {

            status.stat.AttackDelayTime += Time.deltaTime;

        }



        if (status.stat.AttackDelayTime >= 0.0f)

        {

            status.stat.AttackDelayTime = -status.stat.AttackSpeed * Random.Range(0.9f, 1.1f);

            status.unitState.IsActionDelay = false;

        }

    }

    private void ConfirmStatus()

    {

        ConfirmDeath();

        ConfirmSkill();

        ConfirmActionDelay();

    }



    private void CollisionCheck()

    {

        if (tirggerProcess == null)

        {

            tirggerProcess = Trigger_Proceed();

            StartCoroutine(tirggerProcess);

        }



        if (status.AttackTarget == null)

        {

            isNearby = false;

            return;

        }



        GameObject target = status.AttackTarget;

        Transform targetTransform = target.GetComponent<Transform>();

        float distance = targetTransform.position.x - gameObject.transform.position.x;



        if (status.UnitSide == eUnitSide.Enemy)

            distance *= -1.0f;



        if (distance < 0.5f && distance > 0)

        {

            isNearby = true;

        }

        else

        {

            isNearby = false;

        }

    }



    IEnumerator Trigger_Proceed()

    {

        while (true)

        {

            //            InfiniteLoopDetector.Run();

            if (isNearby == true)

            {

                if (status.UnitSide == eUnitSide.Enemy)

                    transform.Translate(Vector2.right * 1.0f * Time.deltaTime);

                else

                    transform.Translate(Vector2.left * 1.0f * Time.deltaTime);

            }



            yield return null;

        }

    }





    void PlayerEffectCheck()

    {

        if (status.unitState.IsLobbyMove == true || status.stat.Hp <= 0.0f)

        {

            if (MiniEffectToPlayer != null)

            {

                LunaEntities.Hide(MiniEffectToPlayer);
                MiniEffectToPlayer = null;

            }



            return;

        }



        float auroraRange = 2.0f + (Defined.AddAuroraRangeValue * (LunaDataTable.Instance.playerData.AuroraLevel - 1));

        if (MiniEffectToPlayer == null)

        {

            if (IdentifyFunc.IsInDistance(gameObject, Game.Instance.dataManager.LunaPlayer, auroraRange) == true)

            {

                MiniEffectToPlayer = LunaEntities.Spawn(status.GetLunaAssetBundle().MiniPlayerEffectForUnit, gameObject.transform.position, status.GetLunaAssetBundle().MiniPlayerEffectForUnit.transform.rotation);

            }

        }

        else

        {

            MiniEffectToPlayer.transform.position = gameObject.transform.position;



            if (IdentifyFunc.IsInDistance(gameObject, Game.Instance.dataManager.LunaPlayer, auroraRange) == false)

            {

                LunaEntities.Hide(MiniEffectToPlayer);
                MiniEffectToPlayer = null;

            }

        }

    }



    // buff icon



    private void InitializeBuffImp(System.Action action)

    {

        status.BuffImp.OnEventBuffIcons = action;

    }



    private void BuffIconUpdate()

    {

        BuffPushCheck();

        BuffPositionCheck();

        BuffBlinkCheck();

    }



    void BuffPushCheck()

    {

        foreach (Buff buffData in status.BuffImp.Buffs)

        {

            if (buffData.IconObject == null)

            {

                buffData.IconObject = LunaEntities.Spawn(status.GetLunaAssetBundle().BuffIcons[(int)buffData.BuffType]);

                buffData.IconObject.GetComponent<BuffIconCtrl>().CharacterObject = gameObject;

            }

        }

    }

    private void BuffPositionCheck()

    {

        float sizeCoefficent = (float)((status.BuffImp.Buffs.Count - 1) / 2);



        for (int i = 0; i < status.BuffImp.Buffs.Count; i++)

        {

            if (status.BuffImp.Buffs[i].IconObject != null)

            {

                status.BuffImp.Buffs[i].IconObject.GetComponent<BuffIconCtrl>().SetFinePosition(i, sizeCoefficent, status.BuffImp.IconHeight);

            }

        }

    }



    void BuffBlinkCheck()

    {

        foreach (Buff buffData in status.BuffImp.Buffs)

        {

            if (buffData.IconObject != null)

            {

                if (buffData.IsBlinking == false && buffData.BuffLeftTime <= 5.0f)

                {

                    buffData.IsBlinking = true;



                    buffData.IconObject.GetComponent<BuffIconCtrl>().Blink();

                }

                else if (buffData.IsBlinking == true && buffData.BuffLeftTime >= 5.0f)

                {

                    buffData.IsBlinking = false;



                    buffData.IconObject.GetComponent<BuffIconCtrl>().UnBlink();

                }

            }

        }

    }

}
