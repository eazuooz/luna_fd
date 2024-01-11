using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public List<GameObject> eEnemySlots = new List<GameObject>();
    public List<bool> eEnemySlotTrigger = new List<bool>();

    void Start()
    {
        for (int i = 0; i < eEnemySlotTrigger.Count; i++)
        {
            eEnemySlotTrigger[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < eEnemySlotTrigger.Count; i++)
        {
            if(eEnemySlotTrigger[i] == true)
            {
                GameObject.Instantiate(eEnemySlots[i], Defined.EnemyTowerPosition, Quaternion.identity);
                eEnemySlotTrigger[i] = false;
            }
        }
    }
}
