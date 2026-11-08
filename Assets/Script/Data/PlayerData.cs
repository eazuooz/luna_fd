using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;

[CreateAssetMenu(fileName = "PlayerVar", menuName = "Scriptable Object Asset/PlayerVar")]
public class PlayerData : ScriptableObject//SingletonSO<PlayerData>
{
    #region PLAYER_INFO
    /// <summary>
    /// ITEM NUMBER와 맞춰주세요!
    /// </summary>
    public enum eLunaSkin
    {
        Basic,
        Wood,
        SkullAx,
        Slime,
        Rich,
        Dragon,
        Lene,
    }

    public List<string> UnitNamesInDeck
        = new List<string>();

    public bool IsBegining = true;
    public string SaveStageName;


    [HideInInspector]
    private const int MaxLunaLevel = 20;
    private const int MaxLunaMoveSpeedLevel = 5;
    private const int MaxGoldSpeedLevel = 20;
    private const int MaxTowerHPLevel = 10;
    private const int MaxAuroraLevel = 3;
    private const int MaxUnitMoveSpeedLevel = 5;
    private const int MaxLunaRevivalTimeLevel = 3;

    public int RewardGold = 0;
    public int Gold = 1234567;
    public int StarStone = 1234567;

    public int LunaLevel = 1;
    public int LunaMoveSpeedLevel = 1;
    public int GoldSpeedLevel = 1;
    public int TowerHPLevel = 1;
    public int AuroraLevel = 1;
    public int UnitMoveSpeedLevel = 1;
    public int LunaReviveTimeLevel = 1;

    //EQUIP
    public eLunaSkin SkinType = eLunaSkin.Basic;

    public void ReleaseUnitNameDeck()
    {
        UnitNamesInDeck.Clear();

        for (int i = 0; i < 6; i++)
        {
            UnitNamesInDeck.Add(null);
        }
    }

    public string GetStringSkinType()
    {
        string returnStr = "";

        switch (SkinType)
        {
            case eLunaSkin.Basic:
                {
                    returnStr = "Basic";
                }
                break;
            case eLunaSkin.Wood:
                {
                    returnStr = "Wood";
                }
                break;
            case eLunaSkin.SkullAx:
                {
                    returnStr = "Skull";
                }
                break;
            case eLunaSkin.Slime:
                {
                    returnStr = "Slime";
                }
                break;
            case eLunaSkin.Rich:
                {
                    returnStr = "Rich";
                }
                break;
            case eLunaSkin.Dragon:
                {
                    returnStr = "Dragon";
                }
                break;
            case eLunaSkin.Lene:
                {
                    returnStr = "Lene";
                }
                break;
        }

        return returnStr;
    }

    [HideInInspector]
    public GameObject PlayerPrefab;

    #endregion

    #region LEVEL UP
    private bool SpendMoney(int cost)
    {
        if (cost < 0 || Gold < cost) return false;
        int resultGold = Gold - cost;
        if (resultGold < 0)
            return false;

        Gold = resultGold;
        return true;
    }
    public bool LunaLevelUp(int cost)
    {
        if (LunaLevel >= MaxLunaLevel || !SpendMoney(cost)) return false;
        LunaLevel++;
        return true;
    }
    public bool LunaMoveSpeedLevelUp(int cost)
    {
        if (LunaMoveSpeedLevel >= MaxLunaMoveSpeedLevel || !SpendMoney(cost)) return false;
        LunaMoveSpeedLevel++;
        return true;
    }
    public bool GoldSpeedLevelUp(int cost)
    {
        if (GoldSpeedLevel >= MaxGoldSpeedLevel || !SpendMoney(cost)) return false;
        GoldSpeedLevel++;
        return true;
    }
    public bool TowerHpLevelUp(int cost)
    {
        if (TowerHPLevel >= MaxTowerHPLevel || !SpendMoney(cost)) return false;
        TowerHPLevel++;
        return true;
    }
    public bool AuroraLevelUp(int cost)
    {
        if (AuroraLevel >= MaxAuroraLevel || !SpendMoney(cost)) return false;
        AuroraLevel++;
        return true;
    }
    public bool UnitMoveSpeedLevelUp(int cost)
    {
        if (UnitMoveSpeedLevel >= MaxUnitMoveSpeedLevel || !SpendMoney(cost)) return false;
        UnitMoveSpeedLevel++;
        return true;
    }
    public bool LunaReviveTimeLevelUp(int cost)
    {
        if (LunaReviveTimeLevel >= MaxLunaRevivalTimeLevel || !SpendMoney(cost)) return false;
        LunaReviveTimeLevel++;
        return true;
    }

    #endregion
    #region ITEM


    #endregion
    #region SKILL
    public List<string> equippedSkill
        = new List<string>(3);
    #endregion
    #region FUNC
    public void ValidateDefaults()
    {
        if (string.IsNullOrEmpty(SaveStageName) || Resources.Load<GameObject>("Prefabs/10Stage/" + SaveStageName) == null)
            SaveStageName = "Stage01-01";
        string[] defaultDeck = { "Lene", "Alice", "Dren", "Tira", "PorongSword", "PorongArcher" };
        if (UnitNamesInDeck == null) UnitNamesInDeck = new List<string>();
        while (UnitNamesInDeck.Count < 6) UnitNamesInDeck.Add(defaultDeck[UnitNamesInDeck.Count]);
        if (UnitNamesInDeck.Count > 6) UnitNamesInDeck.RemoveRange(6, UnitNamesInDeck.Count - 6);
        for (int i = 0; i < 6; i++)
        {
            if (string.IsNullOrEmpty(UnitNamesInDeck[i])) continue;
            var path = (i < 4 ? "Prefabs/01Heroes/" : "Prefabs/02Soldiers/") + UnitNamesInDeck[i];
            if (Resources.Load<GameObject>(path) == null) UnitNamesInDeck[i] = defaultDeck[i];
        }
        string[] defaultSkills = { "FireBall", "Thunder", "Healing" };
        if (equippedSkill == null) equippedSkill = new List<string>();
        while (equippedSkill.Count < 3) equippedSkill.Add(defaultSkills[equippedSkill.Count]);
        if (equippedSkill.Count > 3) equippedSkill.RemoveRange(3, equippedSkill.Count - 3);
        for (int i = 0; i < 3; i++)
            if (string.IsNullOrEmpty(equippedSkill[i]) || Resources.Load<SkillData>("ScriptableObjects/LunaSkills/" + equippedSkill[i]) == null)
                equippedSkill[i] = defaultSkills[i];
        LunaLevel = Mathf.Clamp(LunaLevel, 1, MaxLunaLevel);
        LunaMoveSpeedLevel = Mathf.Clamp(LunaMoveSpeedLevel, 1, MaxLunaMoveSpeedLevel);
        GoldSpeedLevel = Mathf.Clamp(GoldSpeedLevel, 1, MaxGoldSpeedLevel);
        TowerHPLevel = Mathf.Clamp(TowerHPLevel, 1, MaxTowerHPLevel);
        AuroraLevel = Mathf.Clamp(AuroraLevel, 1, MaxAuroraLevel);
        UnitMoveSpeedLevel = Mathf.Clamp(UnitMoveSpeedLevel, 1, MaxUnitMoveSpeedLevel);
        LunaReviveTimeLevel = Mathf.Clamp(LunaReviveTimeLevel, 1, MaxLunaRevivalTimeLevel);
        SkinType = (eLunaSkin)Mathf.Clamp((int)SkinType, 0, 6);
        PlayerPrefab = null;
    }

    public void SetCurPlayer()
    {
        PlayerPrefab = LunaSession.Current.Players[(int)SkinType];
    }
    public GameObject CurPlayer()
    {
        if (PlayerPrefab == null)
            return LunaSession.Current.Players[0];
        else
            return PlayerPrefab;
    }
    #endregion
}