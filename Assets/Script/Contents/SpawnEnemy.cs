using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : EverydayDevup.Framework.LunaEntityBehaviour
{
    public List<GameObject> eEnemySlots = new List<GameObject>();
    public List<bool> eEnemySlotTrigger = new List<bool>();

    protected override void OnEntityInitialize()
    {
        for (int i = 0; i < eEnemySlotTrigger.Count; i++)
        {
            eEnemySlotTrigger[i] = false;
        }
    }

    // Update is called once per frame
    protected override void OnEntityUpdate(float elapsed, float realElapsed)
    {
        for (int i = 0; i < eEnemySlotTrigger.Count; i++)
        {
            if(eEnemySlotTrigger[i] == true)
            {
                EverydayDevup.Framework.LunaEntities.Spawn(eEnemySlots[i], Defined.EnemyTowerPosition, Quaternion.identity);
                eEnemySlotTrigger[i] = false;
            }
        }
    }
}
