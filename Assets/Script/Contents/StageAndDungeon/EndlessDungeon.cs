using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class EndlessDungeon : LunaGame
{
    public List<GameObject> Monsters;

    

    private GameObject gameBGimage;
    private GameObject gameBGimageNext;
    public  GameObject gamePlayer { get; set; }

    private int allyCount;
    private int enemyCount;
    private bool changeDungeon;

    public int dungeonLevel;

    public bool randomYDistance;

    [System.Serializable]
    public class ItemInformation
    {
        public ItemData itemData;
        public float probability;
    }

    //   public int itemAmount;
    public List<ItemInformation> ItemDropTable;
    public List<ItemInformation> BossDropTable;
    public List<ItemInformation> MonterDropTable;

    override public void Init()
    {
        base.Init();

        Defined.OriginPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Defined.LunaStartPosition = new Vector3(-5.0f, 0.0f, 0.0f);
        Defined.EnemyTowerPosition = new Vector3(5.0f, 0.0f, 0.0f);
        Defined.AllyTowerPosition = new Vector3(-7.5f, 0.0f, 0.0f);

        isStageStop = false;
        isStageWin = true;
    }
    void Start()
    {
        Init();

        changeDungeon = true;
        dungeonLevel = 0;


        if (backgroundImage != null &&
                Monsters.Count != 0 &&
                GameObject.Find("2DCamera") != null)
        {

            gameBGimage = GameObject.Instantiate(backgroundImage, Defined.OriginPosition, Quaternion.identity);
            gamePlayer = GameObject.Instantiate(LunaDataTable.Instance.playerData.CurPlayer(), Defined.LunaStartPosition, Quaternion.identity);


            foreach (GameObject unit in Monsters)
            {
                PoolManager.WarmPool(unit, 5);
            }

            if (Game.Instance.dataManager.LunaPlayer == null)
                Game.Instance.dataManager.LunaPlayer = gamePlayer; // 플레이어 접근을 위함
            else
                return;

            stageCamera = GameObject.Find("2DCamera").gameObject;

            stageCamera.GetComponent<Camera2D>().target = gamePlayer.transform;

            {
                stageCamera.transform.position
                    = new Vector3(0.0f,
                                    gamePlayer.transform.position.y + 1.865f, stageCamera.transform.position.z);
            }
        }



        StartCoroutine(PreCameraMove(0.0f));
        StartCoroutine(CountUnit());
    }

    void Update()
    {
        Debug.Log(changeDungeon);

        if (changeDungeon == false && isStageStop == false)
        {
            DungeonEndCheck();
        }
    }

    void SummonMonster(bool next = false)
    {
        foreach(GameObject unit in Monsters)
        {
            if(randomYDistance == true)
            {
                float tmpY = Random.Range(-0.3f, 1.0f);

                Vector3 tmpYPos = new Vector3(0, tmpY, tmpY);

                PoolManager.SpawnObject(unit, Defined.EnemyTowerPosition + (next ? new Vector3(17.07f, 0, 0) : new Vector3(0, 0, 0)) + tmpYPos, unit.transform.rotation);
            }
            else
                PoolManager.SpawnObject(unit, Defined.EnemyTowerPosition + (next ? new Vector3(17.07f, 0, 0) : new Vector3(0, 0, 0)) , unit.transform.rotation);
        }
    }

    void DungeonEndCheck()
    {
        if (enemyCount == 0)
        {
            changeDungeon = true;
            isStageWin = true;
        }
        else if (gamePlayer == null)
        {
            isStageStop = true;
            isStageWin = false;
        }

        if (changeDungeon == true)
        {
            StartCoroutine(ProceedDungeon());
        }
        else if (isStageStop == true)
        {
            StopAllCoroutines();
            StartCoroutine(BackLobby());
        }
    }


    IEnumerator ProceedDungeon()
    {
        // 추가 스테이지 생성
        // 적 생성

        dungeonLevel++;

        gameBGimageNext = GameObject.Instantiate(backgroundImage, Defined.OriginPosition + new Vector3(17.07f, 0, 0), Quaternion.identity);

        SummonMonster(true);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        // 카메라와 유닛 이동

        yield return new WaitForSeconds(0.5f);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().LobbyDistance = Defined.LunaStartPosition.x - unit.transform.position.x + Random.Range(-0.5f, 0.5f);

            if (unit.GetComponent<Status>().LobbyDistance > 0)
                unit.GetComponent<Status>().LobbyDistance = 0;
        }, IdentifyFunc.IsAlly);

        while (gameBGimageNext.transform.position.x >= 0)
        {
            gameBGimage.transform.Translate(Vector3.right * Time.deltaTime * -5 + Vector3.forward * Time.deltaTime);
            gameBGimageNext.transform.Translate(Vector3.right * Time.deltaTime * -5);

            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.transform.Translate(Vector3.right * Time.deltaTime * -5);
            }, IdentifyFunc.IsEnemy);

            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.transform.Translate(Vector3.right * Time.deltaTime * unit.GetComponent<Status>().LobbyDistance * 0.25f);
            }, IdentifyFunc.IsAlly);

            yield return null;
        }

        gameBGimageNext.transform.position = Defined.OriginPosition;

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.transform.position = new Vector3( Defined.EnemyTowerPosition.x , unit.transform.position.y, unit.transform.position.z);
        }, IdentifyFunc.IsEnemy);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = false;
        }, IdentifyFunc.IsUnit);

        Destroy(gameBGimage);

        gameBGimage = gameBGimageNext;

        changeDungeon = false;
    }


    IEnumerator CountUnit()
    {
        int tmpAllyCount;
        int tmpEnemyCount;

        while (true)
        {
            tmpAllyCount = 0;
            tmpEnemyCount = 0;

            yield return new WaitForSeconds(0.5f);

            IdentifyFunc.PickingUnits((unit) => 
            {
                tmpAllyCount++;
            }, IdentifyFunc.IsAlly);

            IdentifyFunc.PickingUnits((unit) =>
            {
                tmpEnemyCount++;
            }, IdentifyFunc.IsEnemy);

            allyCount = tmpAllyCount;
            enemyCount = tmpEnemyCount;
        }
    }

    IEnumerator PreCameraMove(float Coef)
    {
        SummonMonster();

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

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

        changeDungeon = false;
    }



    IEnumerator BackLobby()
    {
        Game.Instance.dataManager.LunaPlayer = null;

        if (gamePlayer != null)
            gamePlayer.GetComponent<LunaStateMachine>().moving = 0;

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        yield return new WaitForSeconds(0.5f);

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        //if (StageState == true)
        //    DropItem();

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

        if (BossDropTable.Count > 0)
        {
            GameObject tmp;

            if (true /* 아이템 이미 가지고 있는지 확인 작업 */)
            {
                tmp = BossDropTable[0].itemData.cunnectPrefab;
                tmp.GetComponent<DropItem>().SetId(BossDropTable[0].itemData);
            }
            else
            {
                tmp = BossDropTable[1].itemData.cunnectPrefab;
                tmp.GetComponent<DropItem>().SetId(BossDropTable[1].itemData);
            }

            ItemBox.Add(tmp);
        }

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
        for (int j = 0; j < dungeonLevel; j++)
        {
            for (int i = 0; i < ItemBox.Count; ++i)
            {
                GameObject drop
                    = Instantiate(ItemBox[i], Defined.EnemyTowerPosition + new Vector3(Random.Range(-2.0f, 2.0f), 0, 0), ItemBox[i].transform.rotation);

                drop.GetComponent<DropItem>().SetId(ItemDropTable[itemDataID[i]].itemData);
            }
        }
    }
}
