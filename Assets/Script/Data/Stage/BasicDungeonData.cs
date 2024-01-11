using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;


[CreateAssetMenu(fileName = "BasicDungeonData", menuName = "Scriptable Object Asset/BasicDungeonData")]
public class BasicDungeonData : ScriptableObject
{
    [System.Serializable]
    public struct MonsterData
    {
        public GameObject monster;
        public int spawnOrder;
        public int monsterLevel;
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

    public List<ItemInformation> ItemDropTable;
    public List<ItemInformation> MonterDropTable;

    public int goldAmount;

    public int GetMonstersRepeatNumber()
    {
        int sum = 0;

        for (int i = 0; i < nonBossMonsters.Count; i++)
        {
            sum += nonBossMonsters[i].spawnOrder;
        }

        return sum;
    }

    public GameObject GetMonsterByOrder(int count, int repeatNumber)
    {
        int tmpCount = count % repeatNumber;

        for (int i = 0; i < nonBossMonsters.Count; i++)
        {
            if(tmpCount >= nonBossMonsters[i].spawnOrder)
            {
                tmpCount -= nonBossMonsters[i].spawnOrder;
                continue;
            }
            else
            {
                return nonBossMonsters[i].monster;
            }
        }

        return null;
    }

    public IEnumerator SpawnMonster()
    {
        int count = 0;
        int repeatNumber = GetMonstersRepeatNumber();

        while (true)
        {
            //InfiniteLoopDetector.Run();
            yield return new WaitForSeconds(spawnTerm);

            GameObject nextSpawnMonster = GetMonsterByOrder(count, repeatNumber);

            count++;

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
                if(monsters.monsterLevel == 0)
                    monsters.monster.GetComponent<Status>().objectData.Level = 1;
                else
                    monsters.monster.GetComponent<Status>().objectData.Level = monsters.monsterLevel;

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

    #region remark


    //IEnumerator SummonAllOneAlert30(List<MonsterData> monsters)
    //{
    //    while (gameEnemyTower.GetComponent<Status>().stat.Hp > gameEnemyTower.GetComponent<Status>().objectData.hp * 0.3f)
    //    {
    //        yield return new WaitForSeconds(1.0f);
    //    }

    //    foreach (MonsterData enemy in monsters)
    //    {
    //        Vector3 createPos = Defined.endPos;
    //        createPos.z = UnityEngine.Random.Range(0.10001f, 0.00001f);

    //        GameObject tmpObject
    //            = PoolManager.Instance.spawnObject(enemy.gameObj, createPos, Quaternion.identity);

    //        Status status = tmpObject.GetComponent<Status>();
    //        if (status != null)
    //        {
    //            status.ResetRespwanData();
    //            tmpObject.GetComponent<UnitAI>().ResetAnimation();
    //        }

    //        yield return new WaitForSeconds(0.1f);
    //    }

    //}

    //void tmp()
    //{
    //    GameObject[] tmpStages = Resources.LoadAll<GameObject>("Prefabs/10Stage/");

    //    foreach (GameObject stage in tmpStages)
    //    {
    //        BasicDungeon tmpStage = stage.GetComponent<BasicDungeon>();
    //        string dungeonDataPath = "ScriptableObjects/DungeonData/BasicDungeon/" + stage.name;
    //        BasicDungeonData tmpDungeonData = Resources.Load<BasicDungeonData>(dungeonDataPath);

    //        tmpDungeonData.spawnTerm = 5;

    //        tmpDungeonData.nonBossMonsters.Clear();
    //        foreach (var monsters in tmpStage.nonBossMonsters)
    //        {
    //            BasicDungeonData.MonsterData x;
    //            x.monster = monsters.gameObj;
    //            x.spawnOrder = monsters.spawnPercentage;

    //            tmpDungeonData.nonBossMonsters.Add(x);
    //        }

    //        tmpDungeonData.ItemDropTable.Clear();
    //        foreach (var item in tmpStage.ItemDropTable)
    //        {
    //            BasicDungeonData.ItemInformation x = new BasicDungeonData.ItemInformation();
    //            x.itemData = item.itemData;
    //            x.probability = item.probability;

    //            tmpDungeonData.ItemDropTable.Add(x);
    //        }

    //        tmpDungeonData.MonterDropTable.Clear();
    //        foreach (var item in tmpStage.MonterDropTable)
    //        {
    //            BasicDungeonData.ItemInformation x = new BasicDungeonData.ItemInformation();
    //            x.itemData = item.itemData;
    //            x.probability = item.probability;

    //            tmpDungeonData.MonterDropTable.Add(x);
    //        }

    //        tmpDungeonData.goldAmount = tmpStage.goldAmount;


    //        Debug.Log(stage.name + " ¿Ï·á");
    //    }


    //}

    #endregion
}

