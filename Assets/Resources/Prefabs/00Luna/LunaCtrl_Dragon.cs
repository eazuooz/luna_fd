using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class LunaCtrl_Dragon : EverydayDevup.Framework.LunaEntityBehaviour
{
    public GameObject DragonPet;

    private Status status;

    private SkeletonAnimation MyAni;
    private SkeletonAnimation Petani;

    private GameObject dragonPetPrefab;
    private Vector3 petLocalPosition;
    private Quaternion petLocalRotation;
    private Vector3 petLocalScale;

    protected override void OnEntityAwake()
    {
        if (DragonPet == null)
            throw new System.InvalidOperationException("Dragon Luna is missing its authored pet.");
        petLocalPosition = DragonPet.transform.localPosition;
        petLocalRotation = DragonPet.transform.localRotation;
        petLocalScale = DragonPet.transform.localScale;
        dragonPetPrefab = Resources.Load<GameObject>("Prefabs/00Luna/06_DragonPet");
        if (dragonPetPrefab == null)
            throw new System.InvalidOperationException("The Dragon pet prefab is missing.");
    }

    protected override void OnEntityShow()
    {
        // Keep the initial authored child. Later lives use a fresh GF entity: the
        // pet's independent transient registration is retired with its owner's life.
        if (DragonPet == null)
        {
            DragonPet = LunaEntities.Spawn(dragonPetPrefab, transform.TransformPoint(petLocalPosition),
                transform.rotation * petLocalRotation, transform);
            DragonPet.transform.localPosition = petLocalPosition;
            DragonPet.transform.localRotation = petLocalRotation;
            DragonPet.transform.localScale = petLocalScale;
        }
        Petani = DragonPet.transform.Find("Sprite").GetComponent<SkeletonAnimation>();
    }

    protected override void OnEntityHide()
    {
        var pet = DragonPet;
        DragonPet = null;
        Petani = null;
        if (pet != null) LunaEntities.Hide(pet);
    }



    protected override void OnEntityInitialize()
    {
        status = GetComponent<Status>();

        MyAni = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        Petani = DragonPet.transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
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
