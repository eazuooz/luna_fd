using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class DropItem : LunaItem
{
    //private static 
    private SkeletonAnimation ani;

    public int ID { get; set; }
    public bool bossItem;

    private float magnetDistance;

    void Start()
    {
        ItemInfoInitialized();
        magnetDistance = Defined.magneticDistacne;

        SkeletonAnimationLogic();
        StartCoroutine(WaitForChase());
    }

    void Update()
    {
        ChasePlayer(GetItem);
    }
    
    private void SkeletonAnimationLogic()
    {
        ani = transform.Find("Sprite").gameObject.GetComponent<SkeletonAnimation>();
        ani.state.Complete += delegate (Spine.TrackEntry trackEntry)
        {
            if (trackEntry.Animation.Name == "ready")
            {
                ani.state.SetAnimation(0, "idle", true);
                isMagnetic = true;
            }
        };
        ani.AnimationState.SetAnimation(0, "ready", true);
    }

    IEnumerator WaitForChase()
    {
        while (isMagnetic == true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));

        IdentifyFunc.PickingUnits((unit) =>
        {
            if (IdentifyFunc.IsInDistance(gameObject, unit, magnetDistance))
            {
                isMagnetic = true;
            }
        }, IdentifyFunc.IsPlayer);
    }

    public void GetItem()
    {
        ItemData itemData;
        LunaDataTable.Instance.itemTable.TryGetValue(ID, out itemData);
        itemData.itemCount += 1;

        #region ItemDictionaryADd
        if (Game.Instance.dataManager.pickUpData.ContainsKey(ID))
            Game.Instance.dataManager.pickUpData[ID] += 1;
        else
            Game.Instance.dataManager.pickUpData.Add(ID, 1);
        #endregion

        if (gainEffect != null)
        {
            Defined.EffectCreate(gainEffect, target, Defined.eEffectPosition.UNDERFEET);
        }

        Destroy(gameObject);
    }
    public void SetId(ItemData itemdata)
    {
        string itemName = itemdata.name.Substring(0, 4);
        int tmpId = int.Parse(itemName);
        ID = tmpId;

        return;
    }
}
