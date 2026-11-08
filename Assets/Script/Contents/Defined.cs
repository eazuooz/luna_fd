using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using UnityGameFramework.Runtime;

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

    //LevelUp const value
    public const float AddMoveSpeedValue = 0.1f;
    public const float AddGoldSpeedValue = 0.03f;
    public const float AddAuroraRangeValue = 1.0f;
    public const float lunaReviveTimeValue = 0.15f;

    //ItemValue
    public const float magneticDistacne = 100.0f;

    /// <summary>
    /// return prob >= 100f || (prob > 0f && Random.value < prob * 0.01f);
    /// </summary>
    /// <param name="prob"></param>
    /// <returns></returns>
    public static bool CanProbability(float prob)
    {
        return prob >= 100f || (prob > 0f && Random.value < prob * 0.01f);
    }
    public static int EffectCreate(GameObject prefab, Vector3 position, int roundId)
    {
        return EffectCreate(prefab, position, prefab != null ? prefab.transform.rotation : Quaternion.identity, roundId);
    }
    public static int EffectCreate(GameObject prefab, Vector3 position, Quaternion rotation, int roundId)
    {
        if (prefab == null) return 0;
        var data = EffectData.Create(position, roundId: roundId); data.Rotation = rotation;
        return LunaSpawn.Show<EffectLogic>(prefab, data);
    }
    public static int EffectCreate(GameObject prefab, Vector3 position, Vector3 size, int roundId)
    {
        if (prefab == null) return 0;
        var data = EffectData.Create(position, roundId: roundId); data.Rotation = prefab.transform.rotation;
        data.ScaleMultiplier = size.x;
        return LunaSpawn.Show<EffectLogic>(prefab, data);
    }
    public static int EffectCreate(GameObject prefab, GameObject target,
        eEffectPosition effectPosition = eEffectPosition.TARGET, bool chase = false)
    {
        if (prefab == null || target == null) return 0;
        Vector3 point = target.transform.position;
        var unit = UnitLogic.From(target);
        if (effectPosition == eEffectPosition.TARGET)
        {
            if (unit != null && unit.ProjecttileArrivalLocation != null) point = unit.ProjecttileArrivalLocation.transform.position;
            else if (target.transform.Find("Sprite") != null) point = target.transform.Find("Sprite").position;
        }
        else if (effectPosition == eEffectPosition.HEAD) point += Vector3.up * 1.5f;
        var owner = target.GetComponent<Entity>()?.Logic as EntityLogicBase;
        var data = EffectData.Create(point, owner != null ? owner.ID : 0, chase);
        data.Rotation = prefab.transform.rotation;
        return LunaSpawn.Show<EffectLogic>(prefab, data);
    }
}

public static class IdentifyFunc
{
    public static bool IsAlive(GameObject unit) => Luna.Gameplay.LunaCombat.IsAlive(unit);
    private static List<GameObject> SelectUnits(params System.Func<GameObject, bool>[] conditions)
    {
        var matches = new List<GameObject>();
        foreach (var actor in new List<Luna.Gameplay.UnitLogic>(Luna.Gameplay.UnitLogic.ActiveUnits))
        {
            var unit = actor != null ? actor.gameObject : null;
            if (!LunaCombat.IsInCombat(actor)) continue;
            bool match = true;
            foreach (var condition in conditions) if (condition != null && !condition(unit)) { match = false; break; }
            if (match) matches.Add(unit);
        }
        return matches;
    }
    public static void PickingUnit(System.Action<GameObject> action, params System.Func<GameObject, bool>[] conditions)
    {
        var matches = SelectUnits(conditions);
        if (matches.Count > 0) action(matches[Random.Range(0, matches.Count)]);
    }
    public static void PickingUnits(System.Action<GameObject> action, params System.Func<GameObject, bool>[] conditions)
    {
        foreach (var unit in SelectUnits(conditions)) if (IsAlive(unit)) action(unit);
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
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitSide == eUnitSide.Allied)
            return true;
        else
            return false;
    }
    public static bool IsEnemy(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitSide == eUnitSide.Enemy)
            return true;
        else
            return false;
    }
    public static bool IsPlayer(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).hitPriority == eHitPriority.VIP)
            return true;
        else
            return false;
    }
    public static bool IsSoldier(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).hitPriority == eHitPriority.NORMAL)
            return true;
        else
            return false;
    }
    public static bool IsTower(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).hitPriority == eHitPriority.TOWER)
            return true;
        else
            return false;
    }
    public static bool IsNotTower(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).hitPriority != eHitPriority.TOWER)
            return true;
        else
            return false;
    }
    public static bool IsAirborne(GameObject unit) => unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).objectData.IsAirborne;
    public static bool IsMelee(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitAttackType == eUnitAttackType.Closed)
            return true;
        else
            return false;
    }
    public static bool IsRange(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitAttackType == eUnitAttackType.Ranged)
            return true;
        else
            return false;
    }
    public static bool IsHero(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitType == eUnitType.Hero)
            return true;
        else
            return false;
    }
    public static bool IsNotHero(GameObject unit)
    {
        if (unit != null && Luna.Gameplay.UnitLogic.From(unit) != null && Luna.Gameplay.UnitLogic.From(unit).UnitType != eUnitType.Hero)
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
        if (standard == null) return false;
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
        if (standard == null) return false;
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
