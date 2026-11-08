using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using Spine.Unity;

public class Asmo : EnemySkillLogic
{

    public GameObject skillEffect => View.GetPrefab("skillEffect");

    // Start is called before the first frame update
    protected override void ConfigureUnitForSpawn()
    {
        status.unitState.IsTargetingSkill = false;
    }

    // Update is called once per frame
    

    protected override void ExecuteSkill()
    {
        float Damage = 300.0f;



        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.UNDERFEET);

            UnitLogic.From(unit).Attack(gameObject, Damage, false, true);
            UnitLogic.From(unit).TakeStun(1.0f);
            UnitLogic.From(unit).TakeNuckbackMovement();

        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }
}
