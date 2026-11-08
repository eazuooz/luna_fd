using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;



[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Object Asset/ObjectData")]
public class ObjectData : ScriptableObject
{


    [Header("OriginData")]
    public int ID = -1;

    #region save data
    [Header("SaveData")]
    public string engName = "";
    public string UnitName = "이름";
    public int Level = 1;
    public int moveSpeedLevel = 1;
    public int cardCount = 99;
    public eUltState ultState = eUltState.STAR1;
    public char cardLevel = 'D';
    [Space(15.0f)]
    #endregion

    #region basic info
    [Header("CharacterInfo")]
    public eHitPriority hitPriority = eHitPriority.NORMAL;
    public Sprite profilePicture;
    public Sprite minimapPicture;
    public Sprite illustPicture;
    public Sprite cardPicture;
    public Sprite skillIcon;
    public string illustName = "일러스트 이름";
    [Multiline] public string skillName = "";
    [Multiline] public string skillStr = "";
    [Multiline] public string start3 = "";
    [Multiline] public string start5 = "";
    [Multiline] public string ultra1 = "";
    [Multiline] public string ultra2 = "";
    [Space(15.0f)]
    #endregion

    #region type info
    [Header("CharacterType")]
    public eUnitSide UnitSide;
    public eUnitType UnitType;
    public eUnitAttackType UnitAttackType;

    public bool TypeLunaPlayer = false;
    public bool TypeSuperArmour = false;
    public bool IsAirborne;
    public bool IsOffensiveHero;
    [Space(15.0f)]
    #endregion

    #region stat info

    [Header("CharacterStat")]
    public int Price = 1;
    public float Hp = 10.0f;
    public float Mp = 0.0f;
    public float Stamina = 0.0f;
    public float MoveSpeed = 0.0f;
    public float Armour = 100.0f;
    public float AttackDamage = 0.0f;
    public float AttackOnceBySecondValue = 1.0f;
    public float SkillResistance = 0.0f;
    public float SkillDamage = 100.0f;
    public float AttackRange = 0.0f;
    public float SpawnDelayTime = 2.0f;
    public float CriticalMultiples = 1.0f;  //치명타배수 치명타시 곱연산
    public int CriticalProbability = 0;   //백분위 100일때 최대

    [Space(10.0f)]
    public float LevelUpHpPoint = 0.0f;
    public float LevelUpAttPoint = 0.0f;
    public float LevelUpArmourPoint = 0.0f;
    public float LevelUpSkillDamage = 0.0f;
    public float LevelUpMoveSpeed = 0.0f;

    [Space(10.0f)]
    public float EffectScale = 1.0f;
    #endregion

    #region subject
    /// <summary>
    /// 강화 또는 레벨업 관찰자들에게 Noti를 하기 위한 subject
    /// </summary>
    [field: System.NonSerialized] public event System.Action<ObjectData> Changed;
    public void NotifyChanged() => Changed?.Invoke(this);
    public void RankUp()
    {
        if (ultState < eUltState.ULT2) ultState += 1;

        NotifyChanged();
    }

    public void LevelUp()
    {
        Level += 1;

        NotifyChanged();
    }

    #endregion

    private static float Factor(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Max(0f, value);

    #region GETSTAT

    public int GetCurrentLevelHP_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0, (int)((Hp + LevelUpHpPoint * Mathf.Max(0, Level - 1)) * Factor(coefficient)));
    }
    public int GetCurrentLevelMP_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0, (int)(Mp * Factor(coefficient)));
    }
    public int GetCurrentLevelStamina_Value(float coefficient = 1.0f)
    {
        return Stamina > 0f ? Mathf.Max(1, Mathf.CeilToInt(Stamina * Factor(coefficient))) : 0;
    }
    public float GetAttackOnceBySecond_Value(float coefficient = 1.0f)
    {
        return 1f / Mathf.Max(0.0001f, AttackOnceBySecondValue * Factor(coefficient));
    }
    public float GetMoveSpeed_Value(float coefficient = 1.0f)
    {
        float resultMoveSpeed = MoveSpeed + LevelUpMoveSpeed * Mathf.Max(0, moveSpeedLevel - 1);

        if (TypeLunaPlayer)
        {
            int lunaMoveSpeedLevel 
                = (LunaDataTable.Instance.playerData.LunaMoveSpeedLevel);
            resultMoveSpeed += (Defined.AddMoveSpeedValue * lunaMoveSpeedLevel);
        }
        else if (UnitType == eUnitType.Soldier 
                || UnitType == eUnitType.Hero)
        {
            int unitMoveSpeedLevel 
                = (LunaDataTable.Instance.playerData.UnitMoveSpeedLevel);
            resultMoveSpeed += (Defined.AddMoveSpeedValue * unitMoveSpeedLevel);
        }

        resultMoveSpeed *= Factor(coefficient) * (UnitSide == eUnitSide.Enemy ? -1.0f : 1.0f);
        return resultMoveSpeed;
    }
    public float GetArmour_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0f, (Armour + Mathf.Max(0, Level - 1) * LevelUpArmourPoint) * Factor(coefficient));
    }
    public int GetArmour_ValueInt(float coefficient = 1.0f)
    {
        return (int)GetArmour_Value(coefficient);
    }
    public int GetAttackDamage_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0, (int)((AttackDamage + LevelUpAttPoint * Mathf.Max(0, Level - 1)) * Factor(coefficient)));
    }
    public float GetSkillResistance_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0f, SkillResistance * Factor(coefficient));
    }
    public float GetSkillDamage_Value(float coefficient = 1.0f)
    {
        return GetResultSkillAtt_Value(coefficient);
    }
    public float GetAttackRange_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0f, AttackRange * Random.Range(0.8f, 1.2f) * Factor(coefficient));
    }
    public int GetResultSkillAtt_Value(float coefficient = 1.0f)
    {
        return Mathf.Max(0, (int)((SkillDamage + LevelUpSkillDamage * Mathf.Max(0, Level - 1)) * Factor(coefficient)));
    }

    public float GetResultMoveSpeed_Value(float coefficient = 1.0f)
    {
        return (MoveSpeed + LevelUpMoveSpeed * Mathf.Max(0, moveSpeedLevel - 1)) * Factor(coefficient);
    }

    public float GetAddedMoveSpeed(float coefficient = 1.0f)
    {
        return (LevelUpMoveSpeed * (float)(moveSpeedLevel - 1) * 100.0f);
    }

    public float GetAddedLevelUpMoveSpeed(float coefficient = 1.0f)
    {
        return (LevelUpMoveSpeed * (float)(moveSpeedLevel) * 100.0f);
    }

    #endregion
}