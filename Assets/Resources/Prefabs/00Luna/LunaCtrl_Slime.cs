using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaCtrl_Slime : MonoBehaviour
{
    private Status status;



    void Start()
    {
        status = GetComponent<Status>();
        SlimePassiveSkill(1.1f);
    }

    void Update()
    {
        
    }

    public void SlimePassiveSkill(float coefficient)
    {
        if(status != null)
        {
            status.MultipleArmour(coefficient);
            //status.MultipleSkillResistance(coefficient);
        }
    }
}
