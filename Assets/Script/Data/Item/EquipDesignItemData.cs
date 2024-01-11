using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipDesignItemData", menuName = "Scriptable Object Asset/EquipDesignItemData")]
public class EquipDesignItemData : ItemData
{
    [Header("ĳ���� UI PREFAB")]
    public GameObject uiPrefab;
    public PlayerData.eLunaSkin skinType;
    [Header("���� ������ ����")]
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
    [Header("���� ��ų ����")]
    private const int equipItemCount = 3;
    public List<SkillData> equipSkills
        = new List<SkillData>(equipItemCount);
    //public SkillData swordSkill;
    //public SkillData shieldSkill;
    //public SkillData ringSkill;
}
