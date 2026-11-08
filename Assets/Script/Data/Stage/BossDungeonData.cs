using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Preserved legacy authoring snapshot for the one-time CSV exporter. Runtime rounds read LunaStageTables.</summary>
public sealed class BossDungeonData : ScriptableObject
{
    [Serializable] public struct MonsterData
    {
        public GameObject monster;
        public int spawnOrder;
        public int monsterLevel;
    }
    [Serializable] public sealed class ItemInformation
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
    public int monstersLevelAtStage = 1;
}
