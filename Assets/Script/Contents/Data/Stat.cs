using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{

    /// <summary>
    /// Basic stat
    /// </summary>
    public float Hp;
    public float Mp;
    public float Stamina;
    public float MoveSpeed;
    public float AttackSpeed;
    public float Armour;
    public float AttackDamage;
    public float AttackRange;

    /// <summary>
    /// Avoidance stat
    /// </summary>
    public float AttackAvoidance;
    public float SkillAvoidance;
    public float DebuffAvoidance;

    /// <summary>
    /// Criticla stat
    /// </summary>
    public int CriticalActuationProbability;
    public float CriticalMultiples;

    /// <summary>
    /// Time Stat
    /// </summary>
    public float SpawnDelayTime;
    public float AttackDelayTime;

    /// <summary>
    /// Boss Special Stat
    /// </summary>
    public float ReflectPercentForBoss;
    public float ReflectPowerForBoss;
    public float ArmourPercentForBoss;
    public float AttackPercentForBoss;



}
