using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipDesignItemData", menuName = "Scriptable Object Asset/EquipDesignItemData")]
public class EquipDesignItemData : ItemData
{
    [Header("캐릭터 UI PREFAB")]
    public GameObject uiPrefab;
    public PlayerData.eLunaSkin skinType;
    [Header("도면 아이템 정보")]
    public Texture equipIcon;
    public Texture weapon;
    public Texture shield;
    public Texture ring;



    [Space(16)]

    [Multiline]
    public string basicDis = "";

    public string weaponeStr = "";
    [Multiline]
    public string weaponeDis = "";

    public string shieldStr = "";
    [Multiline]
    public string shieldDis = "";

    public string ringStr = "";
    [Multiline]
    public string ringDis = "";

    [Space(16)]
    [Header("도면 스킬 파일")]
    private const int equipItemCount = 3;
    public List<SkillData> equipSkills
        = new List<SkillData>(equipItemCount);
    //public SkillData swordSkill;
    //public SkillData shieldSkill;
    //public SkillData ringSkill;
}
