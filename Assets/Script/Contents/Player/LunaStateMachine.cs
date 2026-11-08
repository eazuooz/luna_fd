using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using Spine.Unity;

public class LunaStateMachine : LunaEntityBehaviour
{
    private List<State> curState = new List<State>();
    private List<State> stateMachine = new List<State>();

    private PlayerData playerData;
    public GameObject myTower;
    public GameObject buffPrefab;
    private GameObject buffObject;
    private SkeletonAnimation skeletonAnimation;
    public string curStateString = "none";
    private bool initialized;
    private GameObject upgradedTower;

    public SkeletonAnimation SkeletonAnimation
    {
        get { return skeletonAnimation; }
        set { skeletonAnimation = value; }
    }


    [HideInInspector] public Status status;
    public int moving;

    public List<State> GetStateMachine()
    {
        return stateMachine;
    }

    protected override void OnEntityShow()
    {
        if (status != null)
        {
            Game.Instance.dataManager.LunaPlayer = gameObject;
            if (initialized) InitBuff();
            // UnitAI_Player owns reused combat-status reset on all authored Luna prefabs.
            if (GetComponent<UnitAI_Player>() == null)
            {
                status.ResetRespwanData();
                ResetAnimation();
            }
        }
    }

    protected override void OnEntityHide()
    {
        StopAllCoroutines();
        if (buffObject != null) LunaEntities.Hide(buffObject);
        buffObject = null;
        if (status == null) return;
        if (status.unitState.IsDeath)
        {
            UIPlayer playerUI = FindAnyObjectByType<UIPlayer>();
            if (playerUI != null && playerUI.isActiveAndEnabled)
                playerUI.QueuePlayerRespawn(gameObject);
        }
        status.IsStatusDataLoadComplete = false;
        status.unitState.IsSummonState = false;

        //status.ReleaseData();
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


    // Start is called before the first frame update
    protected override void OnEntityInitialize()
    {
        InitializePlayer(true);
    }

    /// <summary>Completes first-use or reused pool activation before the caller continues.</summary>
    public void PrepareForSpawn(GameObject tower)
    {
        myTower = tower;
        if (!initialized)
        {
            InitializePlayer(false);
            return;
        }

        // Entity show already reset reused status; do not duplicate buff coroutines.
        Game.Instance.dataManager.LunaPlayer = gameObject;
        moving = 0;
        ChangeState(Idle.Instance);
    }

    public void PrepareForStage(GameObject tower)
    {
        PrepareForSpawn(tower);
        ApplyTowerUpgrade(tower);
    }

    private void ApplyTowerUpgrade(GameObject tower)
    {
        if (tower == null || tower == upgradedTower) return;
        foreach (LunaEntityBehaviour module in tower.GetComponents<LunaEntityBehaviour>())
            module.EnsureEntityInitialized();
        tower.GetComponent<Status>().stat.Hp *= Mathf.Max(1, LunaDataTable.Instance.playerData.TowerHPLevel);
        upgradedTower = tower;
    }

    private void InitializePlayer(bool applyTowerUpgrade)
    {
        if (initialized) return;
        initialized = true;
        Game.Instance.dataManager.LunaPlayer = this.gameObject;
        playerData = LunaDataTable.Instance.playerData;
        string playerDataPath = Application.dataPath;
        playerDataPath += "/ScriptableObjects/PlayerData.asset";

        PlayerData ttt = Resources.Load(playerDataPath) as PlayerData;

        moving = 0;

        status = GetComponent<Status>();

        if (!status.IsStatusDataLoadComplete) status.Ready();

        if (false == status.IsStatusDataLoadComplete)
        {
            LunaEntities.Hide(gameObject);
            return;
        }

        //status.stat.MoveSpeed *= (100.0f + playerData.Coef_Move) / 100.0f;

        if (applyTowerUpgrade) ApplyTowerUpgrade(myTower);

        stateMachine.Add(Idle.Instance);
        stateMachine.Add(Death.Instance);
        stateMachine.Add(Hit.Instance);
        stateMachine.Add(Skill.Instance);
        stateMachine.Add(Attack.Instance);
        stateMachine.Add(Move.Instance);

        ChangeState(stateMachine[0]);
        skeletonAnimation
            = gameObject.GetComponent<Transform>().Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        skeletonAnimation.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "death")
            {
                gameObject.GetComponent<Status>().unitState.IsDeath = true;
            }
            else if (trackEntry.Animation.Name == "damage")
            {
                gameObject.GetComponent<Status>().unitState.IsDamaged = false;
            }
        };

        InitBuff();
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        // 현재 상태들을 모두 실행
        for (int i = curState.Count - 1; i >= 0; i--)
        {
            curState[i].OnExcute(gameObject);
        }

        UpdateBuffEffect();
    }

    protected override void OnEntityRelease()
    {
        if(buffObject != null)
        {
            LunaEntities.Hide(buffObject);
        }
    }

    public void ChangeState(State changeState)
    {
        // 변경하려는 State가 null일 경우 return
        if (changeState == null) return;

        // 변경하려는 State의 level이 0이 아닌 경우 즉, 2단계 이상의 하위 State일 경우.
        if (changeState.mLevel != 0)
        {
            // 현재 바꾸려는 상태가 상위 상태에 연결되어 있지 않는 경우
            if (curState[changeState.mLevel - 1].mNext.Contains(changeState) == false)
                return;
        }

        // 현재 상태의 갯수가 바꾸려는 상태의 레벨보다 더 클경우, 같은 상태가 추가되려 한다면
        if (curState.Count > changeState.mLevel && curState[changeState.mLevel] == changeState)
            return;

        // 상태가 변경될 경우 기존의 하위 상태들을 하나씩 Exit 해준다.
        // ex) 상태가 변경 될 경우 changeState.level이 낮아지기 때문
        for (int i = curState.Count - 1; i >= changeState.mLevel; i--)
        {
            curState[i].OnExit(gameObject);
            curState.RemoveAt(i);
        }

        curState.Add(changeState);
        changeState.OnEnter(gameObject);
    }

    private void InitBuff()
    {
        if (buffObject != null || buffPrefab == null) return;
        Vector3 buffPos = new Vector3
                                 (gameObject.transform.position.x
                                 ,gameObject.transform.position.y - 0.1f
                                 ,gameObject.transform.position.z);

        buffObject = LunaEntities.Spawn
                                    (buffPrefab
                                    ,buffPos
                                    ,Quaternion.Euler(-90.0f, 0.0f, 0.0f));
    }

    private void UpdateBuffEffect()
    {
        if (buffObject != null && buffObject.activeSelf)
        {
            Vector3 buffPos = new Vector3
                             (gameObject.transform.position.x
                             , gameObject.transform.position.y - 0.1f
                             , gameObject.transform.position.z);

            buffObject.transform.position = buffPos;
        }
    }

    public void LMove()
    {
        moving = -1;
    }

    public void RMove()
    {
        moving = 1;
    }

    public void NMove()
    {
        moving = 0;
    }
}


