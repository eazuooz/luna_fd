using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitState
{
    public bool IsSuperArmour;
    public bool IsDamaged;
    public bool IsAttackingTower;

    public bool IsSkillActivaion;
    public bool IsTargetingSkill;

    public bool IsActionDelay;
    public bool IsDeath;
    public bool IsPossibleSummonState;
    public bool IsSummonState;

    public bool IsLobbyMove;
    public bool IsLobbyStartMove;
    public bool IsLobbyBackMove;

    public void InitializedUnitState(ObjectData objectData)
    {
        InitializeAITypes();
        InitializeLobbyMove();
        SetTypeSuperArmour(objectData);
    }
    private void InitializeAITypes()
    {
        IsDamaged = false;
        IsAttackingTower = false;
        IsSkillActivaion = false;
        IsTargetingSkill = false;
        IsActionDelay = false;
        IsDeath = false;          
        IsPossibleSummonState = true;
        IsSummonState = false;
    }
    public void InitializeLobbyMove()
    {
        if(!IsLobbyMove)
            IsLobbyMove = false;

        IsLobbyStartMove = false;
        IsLobbyBackMove = false;
    }
    public void SetTypeSuperArmour(ObjectData objectData)
    {
        IsSuperArmour = objectData.TypeSuperArmour;
    }
}
