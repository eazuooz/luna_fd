using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

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
        int resultGold = (Gold - cost);
        if (resultGold < 0)
            return false;

        Gold = resultGold;
        return true;
    }
    public bool LunaLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (LunaLevel < MaxLunaLevel)
        {
            LunaLevel += 1;
            return true;
        }

        return false;
    }
    public bool LunaMoveSpeedLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (LunaMoveSpeedLevel < MaxLunaMoveSpeedLevel)
        {
            LunaMoveSpeedLevel += 1;
            return true;
        }
        return false;
    }
    public bool GoldSpeedLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (GoldSpeedLevel < MaxGoldSpeedLevel)
        {
            GoldSpeedLevel += 1;
            return true;
        }

        return false;
    }
    public bool TowerHpLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (TowerHPLevel < MaxTowerHPLevel)
        {
            TowerHPLevel += 1;
            return true;
        }
        return false;
    }
    public bool AuroraLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (AuroraLevel < MaxAuroraLevel)
        {
            AuroraLevel += 1;
            return true;
        }

        return false;
    }
    public bool UnitMoveSpeedLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (UnitMoveSpeedLevel < MaxUnitMoveSpeedLevel)
        {
            UnitMoveSpeedLevel += 1;
            return true;
        }

        return false;
    }
    public bool LunaReviveTimeLevelUp(int cost)
    {
        if (SpendMoney(cost) == false)
            return false;

        if (LunaReviveTimeLevel <= MaxLunaRevivalTimeLevel)
        {
            LunaReviveTimeLevel += 1;
            return true;
        }

        return false;
    }

    #endregion
    #region ITEM


    #endregion
    #region SKILL
    public List<string> equippedSkill
        = new List<string>(3);
    #endregion
    #region FUNC
    PlayerData()
    {
        if (SaveStageName == null || SaveStageName == "")
            SaveStageName = "Stage01-01";

        if (UnitNamesInDeck.Count == 0)
        {
            UnitNamesInDeck.Add("Lene");
            UnitNamesInDeck.Add("Alice");
            UnitNamesInDeck.Add("Dren");
            UnitNamesInDeck.Add("Tira");
            UnitNamesInDeck.Add("PorongSword");
            UnitNamesInDeck.Add("PorongArcher");
        }

        if (equippedSkill.Count == 0)
        {
            equippedSkill.Add("FireBall");
            equippedSkill.Add("Thunder");
            equippedSkill.Add("Healing");
        }
    }

    public void SetCurPlayer()
    {
        PlayerPrefab = Game.Instance.dataManager.Players[(int)SkinType];
    }
    public GameObject CurPlayer()
    {
        if (PlayerPrefab == null)
            return Game.Instance.dataManager.Players[0];
        else
            return PlayerPrefab;
    }
    #endregion
}