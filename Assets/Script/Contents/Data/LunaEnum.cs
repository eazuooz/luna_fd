using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eHitPriority
{
    VIP = 1,
    NORMAL = 2,
    TOWER = 3,
}
public enum eUltState
{
    STAR1 = 1,
    STAR2 = 2,
    STAR3 = 3,
    STAR4 = 4,
    STAR5 = 5,
    ULT1 = 6,
    ULT2 = 7,
}
public enum eUnitSide
{
    Allied,
    Enemy,
    None,
}
public enum eUnitType
{
    Hero,
    Soldier,
    Tower,
    Boss,
    TowerAndBoss,
}
public enum eUnitAttackType
{
    Closed,
    Ranged,
    Special,
}

public enum eBuffType
{
    ATTACK = 0,
    ATKSPEED = 1,
    DEFENSE = 2,
    MOVESPEED = 3,
    CRITICAL = 4,
}