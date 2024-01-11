using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCoin : LunaItem
{
    private float waitTime = 1.5f;

    void Start()
    {
        ItemInfoInitialized();
        StartCoroutine(WaitForChase());
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer(GetCoin);
    }
    IEnumerator WaitForChase()
    {
        yield return new WaitForSeconds(waitTime);

        isMagnetic = true;
    }

    public void GetCoin()
    {
        LunaDataTable.Instance.playerData.RewardGold += 1;
        LunaDataTable.Instance.playerData.Gold += (1 * 100);

        if (gainEffect != null)
        {
            Defined.EffectCreate(gainEffect, target, Defined.eEffectPosition.UNDERFEET);
        }

        Destroy(gameObject);
    }
}
