using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using Spine.Unity;

public class LunaStateMachine : MonoBehaviour
{
    private List<State> curState = new List<State>();
    private List<State> stateMachine = new List<State>();

    private PlayerData playerData;
    public GameObject myTower;
    public GameObject buffPrefab;
    private GameObject buffObject;
    private SkeletonAnimation skeletonAnimation;
    public string curStateString = "none";

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
    void Start()
    {
        Game.Instance.dataManager.LunaPlayer = this.gameObject;
        playerData = LunaDataTable.Instance.playerData;
        string playerDataPath = Application.dataPath;
        playerDataPath += "/ScriptableObjects/PlayerData.asset";

        PlayerData ttt = Resources.Load(playerDataPath) as PlayerData;

        moving = 0;

        status = GetComponent<Status>();

        status.Ready();

        if (false == status.IsStatusDataLoadComplete)
            Destroy(gameObject);

        //status.stat.MoveSpeed *= (100.0f + playerData.Coef_Move) / 100.0f;

        if (myTower != null)
            myTower.GetComponent<Status>().stat.Hp *= playerData.TowerHPLevel;

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
    void Update()
    {
        // 현재 상태들을 모두 실행
        for (int i = curState.Count - 1; i >= 0; i--)
        {
            curState[i].OnExcute(gameObject);
        }

        UpdateBuffEffect();
    }

    private void OnDestroy()
    {
        if(buffObject != null)
        {
            GameObject.Destroy(buffObject);
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
        Vector3 buffPos = new Vector3
                                 (gameObject.transform.position.x
                                 ,gameObject.transform.position.y - 0.1f
                                 ,gameObject.transform.position.z);

        buffObject = GameObject.Instantiate
                                    (buffPrefab
                                    ,buffPos
                                    ,Quaternion.Euler(-90.0f, 0.0f, 0.0f));
    }

    private void UpdateBuffEffect()
    {
        if (buffObject != null || buffObject.activeSelf != false)
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


