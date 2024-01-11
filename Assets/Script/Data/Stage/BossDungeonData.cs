using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;


[CreateAssetMenu(fileName = "BossDungeonData", menuName = "Scriptable Object Asset/BossDungeonData")]
public class BossDungeonData : ScriptableObject
{
    [System.Serializable]
    public struct MonsterData
    {
        public GameObject monster;
        public int spawnOrder;
    }
    [System.Serializable]
    public class ItemInformation
    {
        public ItemData itemData;
        public float probability;
    }

    public float spawnTerm;
    public MonsterData startMonsters;
    public List<MonsterData> nonBossMonsters;

    public int monstersLevelAtStage = 1;

    public List<ItemInformation> ItemDropTable;
    public List<ItemInformation> MonterDropTable;

    public int goldAmount;

    public GameObject GetMonsterByOrder(int count)
    {
        for (int i = nonBossMonsters.Count - 1; i >= 0; i--)
        {
            if (count % nonBossMonsters[i].spawnOrder == 0)
                return nonBossMonsters[i].monster;
        }

        return null;
    }

    public IEnumerator SpawnMonster()
    {
        int count = 1;

        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(spawnTerm);

            count++;

            GameObject nextSpawnMonster = GetMonsterByOrder(count);

            Vector3 createPos = Defined.EnemyTowerPosition;
            createPos.z = 0.00001f * count;

            if (nextSpawnMonster.GetComponent<Status>() != null)
            {
                GameObject spawnMonster
                    = PoolManager.Instance.spawnObject(nextSpawnMonster, createPos, Quaternion.identity);

                Status status = spawnMonster.GetComponent<Status>();

                if (status != null)
                {
                    status.ResetRespwanData();
                    spawnMonster.GetComponent<UnitAI>().ResetAnimation();
                }
            }
            else
            {
                GameObject spawnMonster
                    = Instantiate(nextSpawnMonster, createPos, Quaternion.identity);
            }
        }
    }

    void MonsterLevelSetting()
    {
        foreach (MonsterData monsters in nonBossMonsters)
        {
            if (monsters.monster != null && monsters.monster.GetComponent<Status>() != null)
            {
                monsters.monster.GetComponent<Status>().objectData.Level = monstersLevelAtStage;
            }
        }
    }

    public void SettingMonsterBeforeStart(GameObject firstMonster)
    {
        MonsterLevelSetting();

        if (startMonsters.monster != null)
        {
            for (int i = 0; i < startMonsters.spawnOrder; i++)
            {
                firstMonster = PoolManager.Instance.spawnObject(startMonsters.monster, Defined.StayMonsterPosition - new Vector3(i * -0.3f, 0, i * -0.3f), Quaternion.identity);

                if (firstMonster.GetComponent<Status>() != null)
                    firstMonster.GetComponent<Status>().unitState.IsLobbyMove = true;
            }
        }
        else
        {
            firstMonster = PoolManager.Instance.spawnObject(nonBossMonsters[0].monster, Defined.StayMonsterPosition, Quaternion.identity);

            if (firstMonster.GetComponent<Status>() != null)
                firstMonster.GetComponent<Status>().unitState.IsLobbyMove = true;
        }
    }

    public void DropItem()
    {
        List<GameObject> ItemBox = new List<GameObject>();
        List<ItemData> ItemDataBox = new List<ItemData>();


        int randMax;
        List<int> itemDataID = new List<int>();
        for (int i = 0; i < ItemDropTable.Count; i++)
        {
            GameObject tmp;
            randMax = Random.Range(0, ItemDropTable.Count);

            if (Defined.CanProbability(ItemDropTable[randMax].probability * 100) == true)
            {
                tmp = ItemDropTable[randMax].itemData.cunnectPrefab;
                ItemBox.Add(tmp);
                itemDataID.Add(randMax);
            }
        }
        for (int i = 0; i < ItemBox.Count; ++i)
        {
            GameObject drop
                = Instantiate(ItemBox[i], Defined.EnemyTowerPosition + new Vector3(Random.Range(-2.0f, 2.0f), 0, 0), ItemBox[i].transform.rotation);

            drop.GetComponent<DropItem>().SetId(ItemDropTable[itemDataID[i]].itemData);
        }

    }

    public void DropItemMonster(Vector3 pos)
    {
        if (MonterDropTable.Count != 1)
            return;

        foreach (var info in MonterDropTable)
        {
            if (Defined.CanProbability(info.probability * 100) == true)
            {
                GameObject itemPrefab = info.itemData.cunnectPrefab;
                GameObject item
                    = Instantiate(itemPrefab, pos, itemPrefab.transform.rotation);
                item.GetComponent<DropItem>().SetId(info.itemData);
            }
        }
    }

   
}

