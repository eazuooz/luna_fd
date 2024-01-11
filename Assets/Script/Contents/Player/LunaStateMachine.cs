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
        // ���� ���µ��� ��� ����
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
        // �����Ϸ��� State�� null�� ��� return
        if (changeState == null) return;

        // �����Ϸ��� State�� level�� 0�� �ƴ� ��� ��, 2�ܰ� �̻��� ���� State�� ���.
        if (changeState.mLevel != 0)
        {
            // ���� �ٲٷ��� ���°� ���� ���¿� ����Ǿ� ���� �ʴ� ���
            if (curState[changeState.mLevel - 1].mNext.Contains(changeState) == false) 
                return;
        }

        // ���� ������ ������ �ٲٷ��� ������ �������� �� Ŭ���, ���� ���°� �߰��Ƿ� �Ѵٸ�
        if (curState.Count > changeState.mLevel && curState[changeState.mLevel] == changeState) 
            return;

        // ���°� ����� ��� ������ ���� ���µ��� �ϳ��� Exit ���ش�.
        // ex) ���°� ���� �� ��� changeState.level�� �������� ����
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


