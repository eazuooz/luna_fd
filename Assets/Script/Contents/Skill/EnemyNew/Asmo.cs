using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using Spine.Unity;

public class Asmo : MonoBehaviour
{
    public Status status { get; set; }

    public GameObject skillEffect;

    // Start is called before the first frame update
    void Start()
    {
        status = GetComponent<Status>();
        status.unitState.IsTargetingSkill = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (status.unitState.IsSkillActivaion == true)
        {
            status.unitState.IsSkillActivaion = false;
            Skill();
        }
    }

    void Skill()
    {
        float Damage = 300.0f;

        if (Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>() != null)
            Game.Instance.lunaSceneManager.CurrentStage.GetComponent<BasicDungeon>().ShakeCameraOn(4);

        IdentifyFunc.PickingUnits((unit) =>
        {
            Defined.EffectCreate(skillEffect, unit, Defined.eEffectPosition.UNDERFEET);

            unit.GetComponent<Status>().Attack(gameObject, Damage, false, true);
            unit.GetComponent<Status>().TakeStun(1.0f);
            unit.GetComponent<Status>().TakeNuckbackMovement();

        }, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
    }
}
