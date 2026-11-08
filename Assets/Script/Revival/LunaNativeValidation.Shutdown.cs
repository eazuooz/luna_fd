#if UNITY_EDITOR || DEBUG
using System.Collections;
using System.Linq;
using GameFramework;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private IEnumerator PlayerProbeWithShutdown(string mode)
    {
        yield return PlayerProbe(mode);
        // This is terminal: no test may initialize tables or native modules afterward.
        yield return ShutdownOwnership();
    }

    private IEnumerator ShutdownOwnership()
    {
        BeginCase("shutdown", "owned-assets-and-native-release");
        var assets = LunaRuntimeData.GetRuntimeAssets();
        var sprites = LunaUIViews.GetRuntimeSprites();
        var entities = GameEntry.GetComponent<EntityComponent>();
        var fsms = GameEntry.GetComponent<FsmComponent>();
        var resources = GameEntry.GetComponent<ResourceComponent>()?.ResourceManager as ResourceManager;
        Require(entities != null && fsms != null && resources != null, "Native shutdown fixture did not capture the live managers.");
        Require(assets.Length > 0 && assets.All(asset => asset != null), "Shutdown fixture had no complete live runtime asset ownership snapshot.");
        Require(sprites.Length > 0 && sprites.All(sprite => sprite != null), "Shutdown fixture had no complete live cached Sprite ownership snapshot.");
        int initialEntities = entities.EntityCount;
        int initialFsms = fsms.Count;
        int initialHandles = resources.LoadedHandleCount;
        Require(initialEntities > 0 && initialFsms > 0 && initialHandles > 0, "Shutdown fixture did not start with native consumers and resource handles.");
        var pendingPrefab = Resources.Load<GameObject>("Prefabs/01Heroes/Alice");
        Require(pendingPrefab != null && pendingPrefab.GetComponent<UnitView>() != null, "Shutdown pending-spawn fixture prefab is missing.");
        var pendingData = HeroData.CreateFrom(pendingPrefab.GetComponent<UnitView>(), new Vector3(0f, -50f));
        pendingData.RoundId = NewFixtureRound();
        pendingData.InitialMode = UnitSpawnMode.Preview;
        int pendingId = pendingData.Id, completedCallbacks = 0, failedCallbacks = 0;
        LunaSpawn.Show<HeroUnitLogic>(pendingPrefab, pendingData, shown => completedCallbacks++, error => failedCallbacks++);
        Require(LunaSpawn.PendingCount > 0 && pendingData.Id == pendingId && completedCallbacks == 0 && failedCallbacks == 0,
            "Shutdown fixture did not reach an actual pending native spawn before cancellation.");
        current.observations.Add("Before shutdown: runtimeAssets=" + assets.Length + "; runtimeSprites=" + sprites.Length +
            "; nativeEntities=" + initialEntities + "; nativeFsms=" + initialFsms + "; loadedResourceHandles=" + initialHandles +
            "; pendingLoads=" + resources.PendingLoadCount + "; pendingUnloads=" + resources.PendingUnloadCount + "; pendingSpawns=" + LunaSpawn.PendingCount);

        yield return LunaBootstrap.Shutdown();
        Require(LunaBootstrap.IsShutdownComplete && !LunaBootstrap.IsReady, "Coordinated shutdown did not reach its terminal state.");
        Require(assets.All(asset => asset == null) && sprites.All(sprite => sprite == null), "Previously owned runtime assets or Sprites remain alive after shutdown returned.");
        Require(pendingData.Id == 0 && completedCallbacks == 0 && failedCallbacks == 0,
            "Shutdown did not return the pending EntityData exactly to its cleared state, or dispatched a cancelled callback.");
        AssertStoppedOwners(entities, fsms, resources);
        current.observations.Add("First shutdown: all captured runtime asset/Sprite objects destroyed; all native owner/cache/handle counters zero.");

        yield return LunaBootstrap.Shutdown();
        yield return null;
        Require(assets.All(asset => asset == null) && sprites.All(sprite => sprite == null), "Repeated shutdown resurrected or retained captured runtime assets.");
        Require(pendingData.Id == 0 && completedCallbacks == 0 && failedCallbacks == 0,
            "A cancelled pending native spawn escaped after repeated shutdown.");
        AssertStoppedOwners(entities, fsms, resources);
        current.observations.Add("Pending native request " + pendingId + " was present before shutdown, its typed data now has Id=0, and both completion/failure callback counts stayed0.");
        current.observations.Add("Second shutdown plus one normal frame: still terminal, no late native entity, FSM, resource handle, pending load/unload or spawn.");
        EndCase("The actual coordinated shutdown destroyed the captured owner-created SO/Sprite objects, released native consumers and resource handles, and remained idempotent; no claim is made about unrelated engine UTP shutdown aggregates.");
    }

    private static void AssertStoppedOwners(EntityComponent entities, FsmComponent fsms, ResourceManager resources)
    {
        // Reuse captured managers. GameEntry/GetModule calls here could recreate a
        // cleared registry/module and would invalidate the ownership assertion.
        Require(!LunaRuntimeData.IsInitialized && LunaRuntimeData.RuntimeAssetCount == 0 && LunaUIViews.RuntimeSpriteCount == 0,
            "Runtime data or Sprite caches retained owned objects after shutdown.");
        Require(LunaSession.Current == null && !UnitLogic.ActiveUnits.Any() && LunaSpawn.PendingCount == 0,
            "Gameplay session, live units or pending entity requests survived shutdown.");
        Require(entities.EntityCount == 0 && entities.GetAllLoadingEntityIds().Length == 0 && fsms.Count == 0,
            "Captured native entity or FSM manager retained consumers after shutdown.");
        Require(resources.IsShutdown && resources.LoadedHandleCount == 0 && resources.PendingLoadCount == 0 && resources.PendingUnloadCount == 0,
            "Addressables adapter retained loaded or pending handles after shutdown: loaded=" + resources.LoadedHandleCount +
            ", loading=" + resources.PendingLoadCount + ", unloading=" + resources.PendingUnloadCount);
        var pools = ReferencePool.GetAllReferencePoolInfos();
        Require(pools.All(pool => pool.UsingReferenceCount == 0),
            "Reference pool usage is nonzero or negative after native shutdown: " +
            string.Join(", ", pools.Where(pool => pool.UsingReferenceCount != 0).Select(pool => pool.Type.Name + "=" + pool.UsingReferenceCount)));
    }
}
#endif
