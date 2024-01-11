using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class BossDungeon : LunaGame
{
    public GameObject bossMonster;
    private GameObject gameBGimage;
    private GameObject gameBossMonster;
    public  GameObject GamePlayer { get; set; }
    public string DungeonName;
    [System.Serializable]
    public class ItemInformation
    {
        public ItemData itemData;
        public float probability;
    }

    //   public int itemAmount;
    public List<ItemInformation> ItemDropTable;
    public List<ItemInformation> MonterDropTable;


    public enum eWeekDay
    {
        MONDAY,
        TUESDAY,
        WEDNESDAY,
    }

    public eWeekDay todayDungeon;

    public float dungeonCount = 90.0f;

    override public void Init()
    {
        base.Init();

        Defined.OriginPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Defined.LunaStartPosition = new Vector3(-5.0f, 0.0f, 0.0f);
        Defined.EnemyTowerPosition = new Vector3(5.0f, 0.0f, 0.0f);
        Defined.AllyTowerPosition = new Vector3(-7.0f, 0.0f, 0.0f);

        isStageStop = true;
        isStageWin = true;
        StageCount = 0.0f;
    }
    void Start()
    {
        Init();

        StageCount = 0.0f;
        if (backgroundImage != null &&
                bossMonster != null &&
                GameObject.Find("2DCamera") != null)
        {
            gameBGimage = GameObject.Instantiate(backgroundImage, Defined.OriginPosition, Quaternion.identity);
            gameBossMonster = GameObject.Instantiate(bossMonster, Defined.EnemyTowerPosition, Quaternion.identity);
            GamePlayer = GameObject.Instantiate(LunaDataTable.Instance.playerData.CurPlayer(), Defined.LunaStartPosition, Quaternion.identity);

            if (Game.Instance.dataManager.LunaPlayer == null)
                Game.Instance.dataManager.LunaPlayer = GamePlayer; // 플레이어 접근을 위함
            else
                return;

            stageCamera = GameObject.Find("2DCamera").gameObject;
            stageCamera.GetComponent<Camera2D>().target = GamePlayer.transform;
            {
                stageCamera.transform.position
                    = new Vector3(0.0f,
                                    GamePlayer.transform.position.y + 1.865f, stageCamera.transform.position.z);
            }
        }

        AdditionalDungeonSetting(todayDungeon);

        StartCoroutine(PreCameraMove(0.0f));
    }

    void Update()
    {
        if (isStageStop == false)
        {
            dungeonCount -= Time.deltaTime;

            DungeonEndCheck();
        }
    }

    void AdditionalDungeonSetting(eWeekDay today)
    {
        switch (today) // 예시로 드는것
        {
            case eWeekDay.MONDAY:
                {
                    // 타임어택
                }
                break;
            case eWeekDay.TUESDAY:
                {
                    // 근거리 or 원거리 제한
                }
                break;
            case eWeekDay.WEDNESDAY:
                {
                    // 전체 지속데미지 등등
                }
                break;
            default:
                break;
        }
    }

    void DungeonEndCheck()
    {
        if (gameBossMonster == null)
        {
            isStageWin = true;
            isStageStop = true;
        }
        else if (GamePlayer == null)
        {
            isStageWin = false;
            isStageStop = true;
        }
        else if (dungeonCount < 0.0f)
        { 
            dungeonCount = 0.0f;
            isStageWin = false;
            isStageStop = true;
        }

        if (isStageStop == true)
        {
            StopAllCoroutines();
            StartCoroutine(BackLobby());
        }
    }

    IEnumerator PreCameraMove(float Coef)
    {
        gameBossMonster.GetComponent<Status>().unitState.IsLobbyMove = true;
        GamePlayer.GetComponent<Status>().unitState.IsLobbyMove = true;

        yield return new WaitForSeconds(1.0f);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = false;
        }, IdentifyFunc.IsUnit);

        Game.Instance.uiManager.Push(UI_TYPE.GAMEOPTION);
        Game.Instance.uiManager.Push(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Push(UI_TYPE.SKILL);
        ///
        Defined.SkillTrigger_Goldy = 0.0f;

        isStageStop = false;
        ///
    }



    IEnumerator BackLobby()
    {
        Game.Instance.dataManager.LunaPlayer = null;

        if (GamePlayer != null)
            GamePlayer.GetComponent<LunaStateMachine>().moving = 0;

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        yield return new WaitForSeconds(0.5f);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        if (isStageWin == true)
            DropItem();

        yield return new WaitForSeconds(0.5f);

        GameObject resultText = Game.Instance.transform.Find("2DCamera").Find("WinLoseText").gameObject;

        SpriteRenderer sr = resultText.GetComponent<SpriteRenderer>();
        Color tempColor = sr.color;
        tempColor.a = 1.0f;
        sr.color = tempColor;

        if (isStageWin == true)
        {
            sr.sprite = resultText.GetComponent<WinLoseText>().winText;
        }
        else
        {
            sr.sprite = resultText.GetComponent<WinLoseText>().loseText;
        }

        yield return new WaitForSeconds(1.0f);

        while (tempColor.a > 0f)
        {
            tempColor.a -= Time.deltaTime / 1.0f;
            sr.color = tempColor;

            if (tempColor.a <= 0f) tempColor.a = 0f;

            yield return null;
        }

        sr.color = tempColor;

        Game.Instance.uiManager.Pop(UI_TYPE.SKILL);
        Game.Instance.uiManager.Pop(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Pop(UI_TYPE.GAMEOPTION);

        Game.Instance.uiManager.Push(UI_TYPE.RESULT);
    }

    void DropItem()
    {
        List<GameObject> ItemBox = new List<GameObject>();
        List<ItemData> ItemDataBox = new List<ItemData>();
        //if (itemAmount < 0)
        //    return;

        int randMax;
        List<int> itemDataID = new List<int>();
        for (int i = 0; i < ItemDropTable.Count; i++)
        {
            GameObject tmp;
            randMax = Random.Range(0, ItemDropTable.Count);

            if (Defined.CanProbability(ItemDropTable[randMax].probability * 100) == true)
            {
                tmp = ItemDropTable[randMax].itemData.cunnectPrefab;
                ItemBox.Add(tmp);
                itemDataID.Add(randMax);
            }
        }
        for (int i = 0; i < ItemBox.Count; ++i)
        {
            GameObject drop
                = Instantiate(ItemBox[i], Defined.EnemyTowerPosition + new Vector3(Random.Range(-2.0f, 2.0f), 0, 0), ItemBox[i].transform.rotation);

            drop.GetComponent<DropItem>().SetId(ItemDropTable[itemDataID[i]].itemData);
        }


    }
}
