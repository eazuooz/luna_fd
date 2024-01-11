using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public static class Manager
{
    public static ScreenManager Screen()
    {
        return Game.Instance.screenManager;
    }
    public static ResourcesManager Resources()
    {
        return Game.Instance.resourcesManager;
    }
    public static CryptoManager Crypto()
    {
        return Game.Instance.cryptoManager;
    }
    public static UIManager UI()
    {
        return Game.Instance.uiManager;
    }
    public static DataManager Data
    {
        get { return Game.Instance.dataManager; }
        private set { }
    }
    public static LunaSceneManager LunaScene()
    {
        return Game.Instance.lunaSceneManager;
    }
    public static LightManager Lightr()
    {
        return Game.Instance.lightManager;
    }
    public static TutorialManager Tutorial()
    {
        return Game.Instance.tutorialManager;
    }
    public static LanguageManager Language()
    {
        return Game.Instance.languageManager;
    }
    public static InputManager Input()
    {
        return Game.Instance.inputManager;
    }
    public static SoundManager Sound()
    {
        return Game.Instance.soundManager;
    }
}

public static class Defined
{
    public enum eEffectPosition
    {
        TARGET = 0,
        UNDERFEET = 1,
        HEAD = 2,
    }
    //Game Value
    public static Vector3 AllyTowerPosition = new Vector3(-8.0f, 0.0f, 0.0f);
    public static Vector3 EnemyTowerPosition = new Vector3(8.0f, 0.0f, 0.0f);

    public static Vector3 LunaStartPosition = new Vector3(-5.0f, 0.0f, 0.0f);
    public static Vector3 StayMonsterPosition = new Vector3(5.0f, 0.0f, 0.0f);
    public static Vector3 OriginPosition = Vector3.zero;

    public static float BackgroundVolumeSize = 1.0f;
    public static float EffectVolumeSize = 0.25f;
    
    public static int MaxHeroCnt = 4;
    public static int MaxSoldierCnt = 2;
    public static int MaxUnitCnt = 6;

    //skill trigger
    public static bool SkillTrigger_Roni = false;
    public static bool SkillTrigger_Rose = false;
    public static bool SkillTrigger_Roen1 = false;
    public static bool SkillTrigger_Roen2 = false;
    public static bool SkillTrigger_Roen3 = false;
    public static bool SkillTrigger_Elena = false;
    public static bool SkillTrigger_Pochi = false;
    public static float SkillTrigger_Goldy = 0.0f;

    //LevelUp const value
    public const float AddMoveSpeedValue = 0.1f;
    public const float AddGoldSpeedValue = 0.03f;
    public const float AddAuroraRangeValue = 1.0f;
    public const float lunaReviveTimeValue = 0.15f;

    //ItemValue
    public const float magneticDistacne = 100.0f;

    /// <summary>
    /// return prob >= Random.Range(1.0f, 100.0f);
    /// </summary>
    /// <param name="prob"></param>
    /// <returns></returns>
    public static bool CanProbability(float prob)
    {
        return prob >= Random.Range(1.0f, 100.0f);
    }
    public static GameObject EffectCreate(GameObject originEffect
                                        , Vector3 targetPos)
    {
        if (originEffect == null)
            return null;

        GameObject tmpEffect 
            = GameObject.Instantiate(originEffect, targetPos, originEffect.transform.rotation);

        return tmpEffect;
    }
    public static GameObject EffectCreate(GameObject originEffect
                                                    , Vector3 targetPos
                                                    , Quaternion rot )
    {
        if (originEffect == null)
            return null;

        GameObject tmpEffect
            = GameObject.Instantiate(originEffect, targetPos, rot);

        return tmpEffect;
    }
    public static GameObject EffectCreate(GameObject originEffect
                                        , Vector3 targetPos
                                        , Vector3 size)
    {
        if (originEffect == null)
            return null;

        GameObject tmpEffect 
            = GameObject.Instantiate(originEffect, targetPos, originEffect.transform.rotation);
        tmpEffect.transform.localScale = size;

        return tmpEffect;
    }

    public static GameObject EffectCreate(GameObject originEffect
                                        , GameObject target
                                        , eEffectPosition effectPosition = eEffectPosition.TARGET
                                        , bool chase = false)
    {
        Vector3 pos = new Vector3();

        if (target == null)
            return null;

        switch (effectPosition)
        {
            case Defined.eEffectPosition.UNDERFEET:
                { 
                    pos = target.transform.position;
                }
                break;
            case Defined.eEffectPosition.TARGET:
                {
                    pos = GetEffectPositionType(target);
                }
                break;
            case Defined.eEffectPosition.HEAD:
                break;
            default:
                break;
        }

        GameObject createdEffect = EffectCreate(originEffect, pos);

        if (chase == true)
        {   
            FollowTarget(createdEffect, target);
        }

        return createdEffect;
    }
    private static Vector3 GetEffectPositionType(GameObject target)
    {
        Status status = target.GetComponent<Status>();
        Transform targetTr = target.transform;

        if (status)
        {
            GameObject projectTileArrivalLocation
                = status.ProjecttileArrivalLocation;

            if (projectTileArrivalLocation != null)
                return projectTileArrivalLocation.transform.position;
        }

        if (targetTr.Find("Sprite"))
        {
            return targetTr.Find("Sprite").position;
        }

        return targetTr.position; 
    }
    private static void FollowTarget(GameObject effect, GameObject target)
    {
        Effect effectCom 
            = effect.GetComponent<Effect>();
        if(effectCom)
        {
            Vector3 effectPos = effect.transform.position;
            Vector3 targetPos = target.transform.position;

            effectCom.IsChase = true;
            effectCom.ChaseDirectionVector = effectPos - targetPos;
            effectCom.ChaseTargetObject = target;
        }
        else
        {
            effect.transform.SetParent(target.transform);
        }
    }
}

public static class IdentifyFunc
{
    public static void PickingUnit(System.Action<GameObject> action
                                 , params System.Func<GameObject, bool>[] conditions)
    {
        GameObject[] units 
            = GameObject.FindGameObjectsWithTag("Unit");

        int trueUnitsCount
            = CheckUnitsConditions(units, conditions);

        if (trueUnitsCount > 0)
        {
            while (!DoPickedAction(units, action))
            {
                //          InfiniteLoopDetector.Run();
            }
        }
    }

    public static void PickingUnits(System.Action<GameObject> action
                                 , params System.Func<GameObject, bool>[] conditions)
    {
        GameObject[] units 
            = GameObject.FindGameObjectsWithTag("Unit");

        int trueUnitsCount 
            = CheckUnitsConditions(units, conditions);

        DoPicksAction(units, action);
    }
    private static int CheckUnitsConditions(GameObject[] units, params System.Func<GameObject, bool>[] conditions)
    {
        int trueUnitsCount = 0;
        for (int i = 0; i < units.Length; i++)
        {
            bool condition
                = CheckConditions(units[i], conditions);

            if (condition)
                trueUnitsCount++;
            else
                EraseUnit(units, units[i]);
        }

        return trueUnitsCount;
    }
    private static bool CheckConditions(GameObject unit, params System.Func<GameObject, bool>[] conditions)
    {
        bool check = true;
        foreach (var condition in conditions)
        {
            bool unitFlag = condition(unit);
            check = check && unitFlag;

            if (check == false)
            {
                return false;
            }
        }

        return true;
    }
    private static void EraseUnit(GameObject[] units, GameObject eraseUnit)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == eraseUnit)
            {
                units[i] = null;
            }
        }
    }
    private static bool DoPickedAction(GameObject[] units, System.Action<GameObject> action)
    {
        int index 
            = Random.Range(0, units.Length);

        if (units[index] == null)
            return false;

        if (units[index].GetComponent<Status>() == null)
            return false;

        if (units[index].GetComponent<Status>().stat.Hp <= 0.0f)
            return false;

        action(units[index]);

        return true;
    }
    private static bool DoPicksAction(GameObject[] units, System.Action<GameObject> action)
    {
        foreach (GameObject unit in units)
        {
            if (unit == null)
                continue;

            if (unit.GetComponent<Status>() == null)
                continue;

            if (unit.GetComponent<Status>().stat.Hp <= 0.0f)
                continue;

            action(unit);
        }

        return true;
    }
    public static bool IsUnit(GameObject unit)
    {
        if (unit)
            return true;
        else
            return false;
    }
    public static bool IsAlly(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitSide == eUnitSide.Allied)
            return true;
        else
            return false;
    }
    public static bool IsEnemy(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitSide == eUnitSide.Enemy)
            return true;
        else
            return false;
    }
    public static bool IsPlayer(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().hitPriority == eHitPriority.VIP)
            return true;
        else
            return false;
    }
    public static bool IsSoldier(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().hitPriority == eHitPriority.NORMAL)
            return true;
        else
            return false;
    }
    public static bool IsTower(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().hitPriority == eHitPriority.TOWER)
            return true;
        else
            return false;
    }
    public static bool IsNotTower(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().hitPriority != eHitPriority.TOWER)
            return true;
        else
            return false;
    }
    public static bool IsMelee(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitAttackType == eUnitAttackType.Closed)
            return true;
        else
            return false;
    }
    public static bool IsRange(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitAttackType == eUnitAttackType.Ranged)
            return true;
        else
            return false;
    }
    public static bool IsHero(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitType == eUnitType.Hero)
            return true;
        else
            return false;
    }
    public static bool IsNotHero(GameObject unit)
    {
        if (unit != null && unit.GetComponent<Status>().UnitType != eUnitType.Hero)
            return true;
        else
            return false;
    }
    public static bool IsInDistance(Vector3 standardPos, GameObject unit, float dis)
    {
        if (standardPos != null && unit != null)
            return IsInDistance(standardPos, unit.transform.position, dis);
        else
            return false;
    }
    public static bool IsInDistance(GameObject standard, GameObject unit, float dis)
    {
        if (standard != null && unit != null)
            return IsInDistance(standard, unit.transform.position, dis);
        else
            return false;
    }
    public static bool IsInDistance(Vector3 standardPos, Vector3 unitPos, float dis)
    {
        return IsInDistanceRight(standardPos, unitPos, dis) || IsInDistanceLeft(standardPos, unitPos, dis);
    }
    public static bool IsInDistance(GameObject standard, Vector3 unitPos, float dis)
    {
        return IsInDistanceRight(standard, unitPos, dis) || IsInDistanceLeft(standard, unitPos, dis);
    }
    public static bool IsInDistanceRight(GameObject standard, GameObject unit, float attackRange)
    {
        if (standard != null && unit != null)
            return IsInDistanceRight(standard, unit.transform.position, attackRange);
        else
            return false;
    }
    public static bool IsInDistanceRight(GameObject standard, Vector3 unitPos, float attackRange)
    {
        float standardToUnitDistance = standard.GetComponent<Transform>().position.x - unitPos.x;

        if (standard != null && standardToUnitDistance <= 0 && -standardToUnitDistance <= attackRange )
            return true;
        else
            return false;
    }
    public static bool IsInDistanceRight(Vector3 standardPos, GameObject unit, float attackRange)
    {
        if (standardPos != null && unit != null)
            return IsInDistanceRight(standardPos, unit.transform.position, attackRange);
        else
            return false;
    }
    public static bool IsInDistanceRight(Vector3 standardPos, Vector3 unitPos, float attackRange)
    {
        float standardToUnitDistance = standardPos.x - unitPos.x;

        if (standardPos != null && standardToUnitDistance <= 0 && -standardToUnitDistance <= attackRange)
            return true;
        else
            return false;
    }
    public static bool IsInDistanceLeft(GameObject standard, GameObject unit, float dis)
    {
        if (standard != null && unit != null)
            return IsInDistanceLeft(standard, unit.transform.position, dis);
        else
            return false;
    }
    public static bool IsInDistanceLeft(GameObject standard, Vector3 unitPos, float dis)
    {
        float distance = standard.GetComponent<Transform>().position.x - unitPos.x;

        if (standard != null && distance >= 0 && dis >= distance)
            return true;
        else
            return false;
    }
    public static bool IsInDistanceLeft(Vector3 standardPos, GameObject unit, float dis)
    {
        if (standardPos != null && unit != null)
            return IsInDistanceLeft(standardPos, unit.transform.position, dis);
        else
            return false;
    }
    public static bool IsInDistanceLeft(Vector3 standardPos, Vector3 unitPos, float dis)
    {
        float distance = standardPos.x - unitPos.x;

        if (standardPos != null && distance >= 0 && dis >= distance)
            return true;
        else
            return false;
    }
}