using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;

[CreateAssetMenu(fileName = "LunaDataTable", menuName = "Scriptable Object Asset/LunaDataTable")]
public class LunaDataTable : ScriptableObject
{
    private static LunaDataTable runtimeTable;

    public static LunaDataTable Instance
    {
        get
        {
            if (!Application.isPlaying) return Resources.Load<LunaDataTable>("ScriptableObjects/LunaDataTable");
            if (runtimeTable == null)
            {
                runtimeTable = CreateInstance<LunaDataTable>();
                runtimeTable.name = "Luna Runtime Data";
                runtimeTable.hideFlags = HideFlags.DontSave;
                LunaRuntimeData.TrackRuntimeAsset(runtimeTable);
            }
            return runtimeTable;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeTable() { runtimeTable = null; }
    internal static void ReleaseRuntimeTable() { runtimeTable = null; }

    LunaDataTable()
    {
        InitializeUnitNames();
        //InitializeItemDatas();
    }

    public string[] heroTable = new string[128];
    public string[] soliderTable = new string[128];
    [System.NonSerialized] public Dictionary<string, ObjectData> UnitTable
        = new Dictionary<string, ObjectData>();

    public PlayerData playerData;

    public void InitializeUnitNames()
    {
        heroTable[0] = "Alice";
        heroTable[1] = "Clare";
        heroTable[2] = "Dren";
        heroTable[3] = "Elena";
        heroTable[4] = "Goldy";
        heroTable[5] = "Joe";
        heroTable[6] = "Lena";
        heroTable[7] = "Lene";
        heroTable[8] = "Lennon";
        heroTable[9] = "Matilda";
        heroTable[10] = "Mei";
        heroTable[11] = "Miu";
        heroTable[12] = "Pobi";
        heroTable[13] = "Pochi";
        heroTable[14] = "Pring";
        heroTable[15] = "Raccon";
        heroTable[16] = "Ria";
        heroTable[17] = "Roen";
        heroTable[18] = "Roni";
        heroTable[19] = "Rose";
        heroTable[20] = "Scarlet";
        heroTable[21] = "Tira";

        soliderTable[0] = "PorongSword";
        soliderTable[1] = "PorongLance";
        soliderTable[2] = "PorongArcher";
        soliderTable[3] = "PorongMace";
        soliderTable[4] = "PorongMaceHigh";
    }

    public void InitializeObjectDatas()
    {
        ScriptableObject[] heroArr 
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/Heroes");
        foreach (var so in heroArr)
        {
            AddUnitData(so as ObjectData);
        }

        ScriptableObject[] soldierArr 
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/Soldiers");
        foreach (var so in soldierArr)
        {
            AddUnitData(so as ObjectData);                                
        }
    }
    private bool AddUnitData(ObjectData objectData)
    {
        if (objectData == null || UnitTable.ContainsKey(objectData.name) || string.IsNullOrEmpty(objectData.name))
            return false;

        UnitTable.Add(objectData.name, LunaRuntimeData.ResolveObjectData(objectData));
        return true;
    }

    #region LUNA DATA TABLE
    [System.NonSerialized] public Dictionary<string, ObjectData> lunaTable
        = new Dictionary<string, ObjectData>();

    public void InitializeLunaDatas()
    {
        ScriptableObject[] lunaDatas
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/Luna");

        foreach (var data in lunaDatas)
        {
            if (lunaTable.ContainsKey(data.name))
                continue;

            string name = data.name;

            lunaTable.Add(data.name, LunaRuntimeData.ResolveObjectData(data as ObjectData));
        }
    }

    public void SetLunaDatasLevel(int level)
    {
        foreach (var data in lunaTable)
        {
            data.Value.Level = level;
        }
    }

    #endregion

    #region ITEM TABLE

    [System.NonSerialized] public Dictionary<int, ItemData> itemTable
        = new Dictionary<int, ItemData>();

    public ItemData currentItemData;

    public void InitializeItemDatas()
    {
        ScriptableObject[] items 
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/Items");

        foreach (var item in items)
        {
            if (item.name.Length < 4 || !int.TryParse(item.name.Substring(0, 4), out int itemNumber)) continue;
            if (itemTable.ContainsKey(itemNumber) || item.name == "")
                continue;

            itemTable.Add(itemNumber, LunaRuntimeData.CloneAsset(item as ItemData));
        }
    }
    public EquipDesignItemData GetCurrentEquipDesignItemData()
    {
        int itemNumber = (int)playerData.SkinType;
        EquipDesignItemData itemData 
            = itemTable[itemNumber] as EquipDesignItemData;

        return itemData;
    }

    #endregion

    #region SKILL

    [System.NonSerialized] public Dictionary<string, SkillData> skiiTable
        = new Dictionary<string, SkillData>();
    
    public void InitializeSKillDatas()
    {
        ScriptableObject[] skills
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/LunaSkills");

        foreach (var skill in skills)
        {
            if (skiiTable.ContainsKey(skill.name) || skill.name == "")
                continue;

            skiiTable.Add(skill.name, LunaRuntimeData.CloneAsset(skill as SkillData));
        }
    }

    public SkillData GetSkillValue(string key)
    {
        if (skiiTable.ContainsKey(key))
        {
            return skiiTable[key];
        }

        return null;
    }

    public void SKillLevelUp(string name)
    {
        if (skiiTable.ContainsKey(name))
        {
            skiiTable[name].level += 1;
        }
    }

    #endregion

    #region StageClearState

    [System.NonSerialized] public Dictionary<string, StageData> StageTable
    = new Dictionary<string, StageData>();

    public void InitializeStageDatas()
    {
        ScriptableObject[] items
            = Resources.LoadAll<ScriptableObject>("ScriptableObjects/Stages");

        foreach (var item in items)
        {
            if (StageTable.ContainsKey(item.name))
                continue;

            var progress = LunaRuntimeData.CloneAsset(item as StageData);
            progress.isClear = false;
            StageTable.Add(item.name, progress);
        }
    }
    #endregion

    #region ETC
    [HideInInspector]
    public List<string> LotteryResult
        = new List<string>();
    #endregion
}
