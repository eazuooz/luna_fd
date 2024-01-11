using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SKillData", menuName = "Scriptable Object Asset/SKillData")]
public class SkillData : ScriptableObject
{
    public enum SkillType
    {
        Passive,
        Active,
        None,
    }

    public GameObject effectPrefab;
    public Sprite skillImage;

    public SkillType type = SkillType.None;
    public bool isMultiple = false;
    public int level = 1;
    public int mana = 0;
    public float passiveDelay = 0.0f;
    public float activeDelay = 0.0f;
    public float damage = 0;
    public float range = 0.0f;
    public float splashRange = 0.0f;
    public float coolTime = 0.0f;
    public float stunTime = 0.0f;
    public float burfTime = 0.0f;
    public float probablity = 100.0f;
    public float multipleSkillDamageBuff = 1.0f;
    public float multipleSkillResistanceBuff = 1.0f;
    public float coefficient = 1.0f;
    public float levelUpPoint = 1.0f;
    public int maxSummonCount = 1;
    public bool ableSkill = true;
    
    

    [Header("이름")]
    public string skillName;
    [Header("설명")] 
    [Multiline] public string skillDiscription;
    [Multiline] public string skillLevelDiscription;
}




