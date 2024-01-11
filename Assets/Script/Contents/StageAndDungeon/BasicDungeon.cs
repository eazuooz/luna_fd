using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;
using UnityEngine.SceneManagement;

public class BasicDungeon : LunaGame
{
    public GameObject enemyTower;
    public GameObject allyTower;

    private GameObject gameBGimage;
    public GameObject gameEnemyTower { get; set; }
    public GameObject gameAllyTower { get; set; }
    public GameObject gamePlayer { get; set; }

    public float stageWidth = 3.25f;
    private float stageCount = 0.0f;

    public bool isBossStage = false;
    private bool skipStartCameraMove;

    private GameObject firstMonster;

    private GameObject commonObject;

    private BasicDungeonData thisDungeonData;


    public bool PlayerSkillMPCheck()
    {
        if (gamePlayer.GetComponent<Status>().stat.Mp >= 100)
        {
            gamePlayer.GetComponent<Status>().stat.Mp = 0;
            return true;
        }
        else
            return false;
    }


    override public void Init()
    {
        base.Init();

        string dungeonDataPath = "ScriptableObjects/DungeonData/BasicDungeon/" + name.Replace("(Clone)", "");

        thisDungeonData = Resources.Load<BasicDungeonData>(dungeonDataPath);
        commonObject = Resources.Load<GameObject>("LunaAssetBundle");

        Defined.OriginPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Defined.AllyTowerPosition = new Vector3(-8.0f, 0.0f, 0.0f);
        Defined.EnemyTowerPosition = new Vector3(8.0f, 0.0f, 0.0f);
        Defined.LunaStartPosition = new Vector3(-5.0f, 0.0f, 0.0f);
        Defined.StayMonsterPosition = new Vector3(5.0f, 0.0f, 0.0f);

        Defined.SkillTrigger_Goldy = 0.0f;

        isStageStop = true;
        isStageWin = true;
        ShakeOn = false;
        AttachCamera = false;
        FollowCamera = false;
        skipStartCameraMove = false;
    }

    void Start()
    {
        Init();

        if (backgroundImage != null &&
                enemyTower != null &&
                allyTower != null &&
                GameObject.Find("2DCamera") != null)
        {
            StartGameObjectsSetting();
            StartCoroutine(PreCameraMove(/*3.25f*/ stageWidth - 0.01f));
            StartCoroutine(PreCameraMoveSkipCheck());
        }
    }

    void Update()
    {
        if (isStageStop == false)
        {
            stageCount += Time.deltaTime * 1.0f;

            if (gameAllyTower == null)
            {
                StageStopCheck(false);
            }
            if (gameEnemyTower == null)
            {
                StageStopCheck(true);
            }
        }
    }

    void StartGameObjectsSetting()
    {
        PoolManager.WarmPool(LunaDataTable.Instance.playerData.CurPlayer(), 1);

        foreach (var monsterData in thisDungeonData.nonBossMonsters)
        {
            Status status = monsterData.monster.GetComponent<Status>();

            if (status != null)
                PoolManager.WarmPool(monsterData.monster, 50);
            //monsterData.gameObj.GetComponent<UnitAI>().InitProjectTile();
        }

        gameBGimage = GameObject.Instantiate(backgroundImage, Defined.OriginPosition, Quaternion.identity);

        gameEnemyTower = GameObject.Instantiate(enemyTower, Defined.EnemyTowerPosition, Quaternion.identity);
        gameAllyTower = GameObject.Instantiate(allyTower, Defined.AllyTowerPosition, Quaternion.identity);
        gamePlayer = PoolManager.Instance.spawnObject(LunaDataTable.Instance.playerData.CurPlayer(), Defined.LunaStartPosition, Quaternion.identity);

        gameEnemyTower.GetComponent<Status>().unitState.IsLobbyMove = true;
        gameAllyTower.GetComponent<Status>().unitState.IsLobbyMove = true;
        gamePlayer.GetComponent<Status>().unitState.IsLobbyMove = true;

        Game.Instance.dataManager.LunaPlayer = gamePlayer;

        gamePlayer.GetComponent<LunaStateMachine>().myTower = gameAllyTower;

        stageCamera = GameObject.Find("2DCamera").gameObject;
        stageCamera.GetComponent<Camera2D>().target = gamePlayer.transform;

        Vector3 firstCameraPosition = new Vector3(  gamePlayer.transform.position.x - 0.52f,
                                                    gamePlayer.transform.position.y + 1.865f, 
                                                    stageCamera.transform.position.z);

        stageCamera.transform.position = firstCameraPosition;
    }

    void StageStopCheck(bool result)
    {
        StopAllCoroutines();
        isStageWin = result;
        StartCoroutine(StageStop());
    }

    IEnumerator PreCameraMoveSkipCheck()
    {
        while(true)
        {
            //InfiniteLoopDetector.Run();
            if (Input.GetMouseButtonDown(0))
            {
                skipStartCameraMove = true;
            }

            yield return null;
        }
    }
    IEnumerator PreCameraMove(float Coef)
    {
        stageCamera.transform.position = new Vector3(-Coef, stageCamera.transform.position.y, stageCamera.transform.position.z);

        yield return new WaitForSeconds(0.8f);

        thisDungeonData.SettingMonsterBeforeStart(firstMonster);

        float speed = 4.0f;
        float tmpCounter = 0.0f;

        while (stageCamera.transform.position.x < Coef && skipStartCameraMove == false)
        {
            //InfiniteLoopDetector.Run();
            stageCamera.transform.Translate(Vector2.right * speed * Time.deltaTime);
            yield return null;
        }

        while(tmpCounter < 1.0f && skipStartCameraMove == false)
        {
            //InfiniteLoopDetector.Run();
            tmpCounter += Time.deltaTime;

            yield return null;
        }

        while (stageCamera.transform.position.x > -Coef && skipStartCameraMove == false)
        {
            //InfiniteLoopDetector.Run();
            stageCamera.transform.Translate(Vector2.left * speed * Time.deltaTime);
            yield return null;
        }

        stageCamera.transform.position = new Vector3( -Coef, stageCamera.transform.position.y, stageCamera.transform.position.z);

        IdentifyFunc.PickingUnits((unit) => 
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = false;
        }, IdentifyFunc.IsUnit);

        StopCoroutine(PreCameraMoveSkipCheck());

        Game.Instance.uiManager.Push(UI_TYPE.GAMEOPTION);
        Game.Instance.uiManager.Push(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Push(UI_TYPE.SKILL);
        Game.Instance.uiManager.Push(UI_TYPE.PROGRESSBAR);

        StartCoroutine(CameraStatusCheck(Coef));
        StartCoroutine(CameraMoveCheck(Coef));
        StartCoroutine(thisDungeonData.SpawnMonster());
        //StartCoroutine(SummonAllOneAlert30(thisDungeonData.nonBossMonsters));

        isStageStop = false;
    }
    

    IEnumerator CameraStatusCheck(float Coef)
    {
        while (true)
        {
            //InfiniteLoopDetector.Run();
            if (AttachCamera == false)
            {
                if (FollowCamera == true)
                {
                    float disMinus = (gamePlayer.transform.position.x + 0.5f) - stageCamera.transform.position.x;

                    if (disMinus < 0.1f && disMinus > -0.1f)
                    {
                        FollowCamera = false;
                        AttachCamera = true;
                    }

                    stageCamera.transform.Translate(new Vector3((disMinus > 0 ? disMinus * 2.5f + 1.0f : disMinus * 2.5f - 1.0f) * Time.deltaTime, 0, 0));

                }
            }
            else
            {
                stageCamera.transform.position
                       = new Vector3(gamePlayer.transform.position.x + 0.5f,
                                       stageCamera.transform.position.y, stageCamera.transform.position.z);
            }


            if (stageCamera.transform.position.x < -Coef)
            {
                stageCamera.transform.position = new Vector3(-Coef, stageCamera.transform.position.y, stageCamera.transform.position.z);
                FollowCamera = false;
            }
            else if (stageCamera.transform.position.x > Coef)
            {
                stageCamera.transform.position = new Vector3(Coef, stageCamera.transform.position.y, stageCamera.transform.position.z);
                FollowCamera = false;
            }

            yield return null;
        }
    }

    IEnumerator StageStop()
    {
        isStageStop = true;

        Game.Instance.dataManager.LunaPlayer = null;

        if(gamePlayer != null)
            gamePlayer.GetComponent<LunaStateMachine>().moving = 0;

        IdentifyFunc.PickingUnits((unit) => 
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        if (isStageWin == true)
        {
            DropItem();
        }

        yield return new WaitForSeconds(0.5f);

        if (isStageWin == true)
        {
            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.GetComponent<Status>().stat.Hp = 0;
            }, IdentifyFunc.IsEnemy);
        }

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyMove = true;
        }, IdentifyFunc.IsUnit);

        yield return new WaitForSeconds(0.5f);

        GameObject resultText = Game.Instance.transform.Find("2DCamera").Find("WinLoseText").gameObject;

        SpriteRenderer sr = resultText.GetComponent<SpriteRenderer>();
        Color tempColor = sr.color;
        tempColor.a = 1.0f;
        sr.color = tempColor;

        if(isStageWin == true)
        {
            sr.sprite = resultText.GetComponent<WinLoseText>().winText;
        }
        else
        {
            sr.sprite = resultText.GetComponent<WinLoseText>().loseText;
        }

        yield return new WaitForSeconds(0.5f);

        while (tempColor.a > 0f)
        {
            //InfiniteLoopDetector.Run();
            tempColor.a -= Time.deltaTime / 1.0f;
            sr.color = tempColor;

            if (tempColor.a <= 0f) tempColor.a = 0f;

            yield return null;
        }

        sr.color = tempColor;

        Game.Instance.uiManager.Pop(UI_TYPE.PROGRESSBAR);
        Game.Instance.uiManager.Pop(UI_TYPE.SKILL);
        Game.Instance.uiManager.Pop(UI_TYPE.PLAYER);
        Game.Instance.uiManager.Pop(UI_TYPE.GAMEOPTION);

        Game.Instance.uiManager.Push(UI_TYPE.RESULT);
    }

    public void ToNextStage()
    {
        Game.Instance.uiManager.Pop(UI_TYPE.RESULT);

        string nextStageName = Game.Instance.dataManager.NextStageString();

        Game.Instance.dataManager.CurrentStageName = nextStageName;
        Game.Instance.dataManager.SetStagePrefab(nextStageName);

        StartCoroutine(GoDownNextStage(stageWidth - 0.01f));
        StartCoroutine(StageBlinkText(3.0f, 3.0f));
    }

    IEnumerator GoDownNextStage(float coef)
    {
        yield return null;

        IdentifyFunc.PickingUnits((unit) =>
        {
            unit.GetComponent<Status>().unitState.IsLobbyStartMove = true;
        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);

        BasicDungeon nextStage = Game.Instance.dataManager.SelectStagePrefab.GetComponent<BasicDungeon>();
        GameObject gameBGimageNext = GameObject.Instantiate(nextStage.backgroundImage, Defined.OriginPosition + new Vector3(stageCamera.transform.position.x + coef, -15.265f, 0), Quaternion.identity);
        GameObject gameAllyTowerNext = GameObject.Instantiate(nextStage.allyTower, Defined.AllyTowerPosition + new Vector3(stageCamera.transform.position.x + coef, -15.265f, 0), Quaternion.identity);
        GameObject gamePlayerNext = GameObject.Instantiate(LunaDataTable.Instance.playerData.CurPlayer(), Defined.OriginPosition + new Vector3(stageCamera.transform.position.x + coef + -13, -15.265f, 0), Quaternion.identity);

        yield return new WaitForSeconds(1.8f);

        while (gameBGimageNext.transform.position.y < 0)
        {
            //InfiniteLoopDetector.Run();
            gameBGimage.transform.Translate(Vector3.up * Time.deltaTime * 3);
            gameBGimageNext.transform.Translate(Vector3.up * Time.deltaTime * 3);

            IdentifyFunc.PickingUnits((unit) =>
            {
                unit.transform.Translate(Vector3.up * Time.deltaTime * 3);
            }, IdentifyFunc.IsUnit);

            yield return null;
        }


        while (gamePlayerNext.transform.position.x < stageCamera.transform.position.x + coef + Defined.LunaStartPosition.x)
        {
            //InfiniteLoopDetector.Run();
            gamePlayerNext.transform.Translate(Vector3.right * Time.deltaTime * 5);

            yield return null;
        }

        SceneManager.LoadScene("Game");
    }

    IEnumerator StageBlinkText(float beforeSecond, float afterSecond)
    {
        yield return new WaitForSeconds(beforeSecond);

        //Game.Instance.uiManager.Push(UI_TYPE.STAGETEXT);


        yield return new WaitForSeconds(afterSecond);

        //Game.Instance.uiManager.Pop(UI_TYPE.STAGETEXT);
    }

    void DropItem()
    {
        thisDungeonData.DropItem();

        DropGold();
    }

    void DropGold()
    {
        int RichDouble = 1;

        if (LunaDataTable.Instance.playerData.SkinType == PlayerData.eLunaSkin.Rich &&
                    Defined.CanProbability(50))
        {
            RichDouble = 2;
        }

        for (int i = 0; i < thisDungeonData.goldAmount * RichDouble; i++)
        {
            Instantiate(commonObject.GetComponent<LunaAssetBundle>().Coin
                , Defined.EnemyTowerPosition + new Vector3(Random.Range(-2.0f, 2.0f), 0, 0)
                , commonObject.GetComponent<LunaAssetBundle>().Coin.transform.rotation);
        }
    }

    public void DropItemMonster(Vector3 pos)
    {
        thisDungeonData.DropItemMonster(pos);
    }
}
