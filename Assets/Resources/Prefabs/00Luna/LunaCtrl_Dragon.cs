using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LunaCtrl_Dragon : MonoBehaviour
{
    public GameObject DragonPet;

    private Status status;

    private SkeletonAnimation MyAni;
    private SkeletonAnimation Petani;



    void Start()
    {
        status = GetComponent<Status>();

        MyAni = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        Petani = DragonPet.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    void Update()
    {
        
    }

    public void DragonSheildSkill(float coefficient)
    {
        Status stat = DragonPet.GetComponent<Status>();

        if(stat != null)
        {
            stat.MultipleAttackDamage(coefficient);
        }
    }
}
