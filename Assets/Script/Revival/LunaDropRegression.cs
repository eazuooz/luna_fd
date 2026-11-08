#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EverydayDevup.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>Opt-in content validation; never invoked by normal gameplay.</summary>
public static class LunaDropRegression
{
    public static IEnumerator Validate(Action<string> record)
    {
        Require(Application.isEditor || Debug.isDebugBuild, "Regression checks require Editor or Development Build.");
        Require(LunaEntities.IsReady && LunaRuntimeData.IsInitialized, "Framework and runtime data must be initialized.");
        ItemData boss = Resources.Load<ItemData>("ScriptableObjects/Items/1000_equip_stone");
        ItemData normal = Resources.Load<ItemData>("ScriptableObjects/Items/1001_skill_stone");
        GameObject dungeonPrefab = Resources.Load<GameObject>("Prefabs/11Dungeons/Dungeon04");
        Require(boss != null && normal != null && dungeonPrefab != null, "Known dungeon/loot fixtures are missing.");
        Require(boss.cunnectPrefab != null && normal.cunnectPrefab != null && boss.cunnectPrefab != normal.cunnectPrefab,
            "Known loot must have distinct real prefabs.");
        DropItem bossSource = boss.cunnectPrefab.GetComponent<DropItem>();
        DropItem normalSource = normal.cunnectPrefab.GetComponent<DropItem>();
        Require(bossSource != null && normalSource != null, "Known loot prefabs require DropItem.");

        ItemData bossRuntime = LunaDataTable.Instance.itemTable[1000];
        ItemData normalRuntime = LunaDataTable.Instance.itemTable[1001];
        Require(!ReferenceEquals(boss, bossRuntime) && !ReferenceEquals(normal, normalRuntime), "Item runtime views must be isolated.");
        string bossJson = JsonUtility.ToJson(boss);
        string normalJson = JsonUtility.ToJson(normal);
        string bossPrefabJson = JsonUtility.ToJson(bossSource);
        string normalPrefabJson = JsonUtility.ToJson(normalSource);
        int bossSourceId = bossSource.ID;
        int normalSourceId = normalSource.ID;
        int bossCount = bossRuntime.itemCount;
        int normalCount = normalRuntime.itemCount;
        var pickups = Game.Instance.dataManager.pickUpData;
        bool hadBossPickup = pickups.TryGetValue(1000, out int bossPickup);
        bool hadNormalPickup = pickups.TryGetValue(1001, out int normalPickup);
        Random.State randomState = Random.state;
        var existing = new HashSet<DropItem>(Object.FindObjectsByType<DropItem>(FindObjectsInactive.Include));
        GameObject fixtureRoot = null;

        try
        {
            // The inactive parent prevents stage Start/adoption, camera movement and monster spawning.
            fixtureRoot = new GameObject("Luna Drop Regression Fixture");
            fixtureRoot.SetActive(false);
            GameObject fixture = Object.Instantiate(dungeonPrefab, fixtureRoot.transform);
            EndlessDungeon dungeon = fixture.GetComponent<EndlessDungeon>();
            Require(dungeon != null, "Dungeon04 must contain EndlessDungeon.");
            dungeon.dungeonLevel = 2;
            dungeon.BossDropTable = new List<EndlessDungeon.ItemInformation>
            {
                new EndlessDungeon.ItemInformation { itemData = boss, probability = 1f }
            };
            dungeon.ItemDropTable = new List<EndlessDungeon.ItemInformation>();
            Random.InitState(7319);

            RunCase(dungeon, boss, normal, 2, 0);
            Require(bossRuntime.itemCount == bossCount + 2 && normalRuntime.itemCount == normalCount,
                "Boss-only pickup credited the wrong ItemData.");
            record?.Invoke("Endless boss-only rewards: 2 real drops, correct prefab/ID and inventory credit; empty regular table is safe.");

            dungeon.ItemDropTable.Add(new EndlessDungeon.ItemInformation { itemData = normal, probability = 1f });
            RunCase(dungeon, boss, normal, 2, 2);
            Require(bossRuntime.itemCount == bossCount + 4 && normalRuntime.itemCount == normalCount + 2,
                "Mixed pickups credited the wrong ItemData.");
            Require(pickups[1000] == bossPickup + 4 && pickups[1001] == normalPickup + 2,
                "Mixed reward ledger does not match actual loot IDs.");
            Require(bossSource.ID == bossSourceId && normalSource.ID == normalSourceId,
                "Drop generation changed an original prefab ID.");
            Require(JsonUtility.ToJson(boss) == bossJson && JsonUtility.ToJson(normal) == normalJson &&
                JsonUtility.ToJson(bossSource) == bossPrefabJson && JsonUtility.ToJson(normalSource) == normalPrefabJson,
                "Drop generation changed original loot data/prefabs.");
            record?.Invoke("Endless mixed rewards: 2 boss + 2 regular drops, matching prefab/ID, exact inventory/ledger deltas and unchanged source assets.");
        }
        finally
        {
            // There is no yield during fixture execution, so only this test can create new drops here.
            foreach (DropItem drop in Object.FindObjectsByType<DropItem>(FindObjectsInactive.Include))
                if (drop != null && !existing.Contains(drop)) LunaEntities.Hide(drop.gameObject);
            if (fixtureRoot != null) Object.Destroy(fixtureRoot);
            bossRuntime.itemCount = bossCount;
            normalRuntime.itemCount = normalCount;
            if (hadBossPickup) pickups[1000] = bossPickup; else pickups.Remove(1000);
            if (hadNormalPickup) pickups[1001] = normalPickup; else pickups.Remove(1001);
            bossSource.ID = bossSourceId;
            normalSource.ID = normalSourceId;
            Random.state = randomState;
        }
        yield return null; // Allow deferred native Entity recycling and fixture destruction.
    }

    private static void RunCase(EndlessDungeon dungeon, ItemData boss, ItemData normal, int bossExpected, int normalExpected)
    {
        var before = new HashSet<DropItem>(Object.FindObjectsByType<DropItem>(FindObjectsInactive.Include));
        MethodInfo dropMethod = typeof(EndlessDungeon).GetMethod("DropItem", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(dropMethod != null, "EndlessDungeon.DropItem entry point is missing.");
        try { dropMethod.Invoke(dungeon, null); }
        catch (TargetInvocationException exception)
        {
            throw new InvalidOperationException("Endless reward regression: real DropItem flow threw.", exception.InnerException ?? exception);
        }

        int bossActual = 0;
        int normalActual = 0;
        foreach (DropItem drop in Object.FindObjectsByType<DropItem>(FindObjectsInactive.Include))
        {
            if (before.Contains(drop)) continue;
            Require(LunaEntities.IsSpawned(drop.gameObject), "Reward was created outside native Entity ownership.");
            ItemData expected;
            if (drop.ID == 1000) { expected = boss; bossActual++; }
            else if (drop.ID == 1001) { expected = normal; normalActual++; }
            else throw new InvalidOperationException("Endless reward regression: unexpected loot ID " + drop.ID + ".");
            Require(drop.name == expected.cunnectPrefab.name || drop.name == expected.cunnectPrefab.name + "(Clone)",
                "Spawned prefab does not match its ItemData ID " + drop.ID + ".");
            // SetId occurs before first Entity update in production too. Collect synchronously to
            // exercise the real inventory/ledger path without a magnetic animation or new effects.
            drop.GetItem();
        }
        Require(bossActual == bossExpected && normalActual == normalExpected,
            "Reward counts differ: boss=" + bossActual + ", regular=" + normalActual + ".");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Endless reward regression: " + message);
    }
}
#endif
