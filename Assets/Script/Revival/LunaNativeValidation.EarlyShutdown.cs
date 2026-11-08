#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private void StartEarlyPlayerShutdown(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= StartEarlyPlayerShutdown;
        StartCoroutine(Guard(EarlyPlayerShutdown(scene)));
    }

    private IEnumerator EarlyPlayerShutdown(Scene scene)
    {
        BeginCase("shutdown", "before-native-procedure-start");
        Require(scene.name == "Loading", "Early shutdown did not observe the actual boot scene: " + scene.name);
        var procedure = GameEntry.GetComponent<ProcedureComponent>();
        var entities = GameEntry.GetComponent<EntityComponent>();
        var fsms = GameEntry.GetComponent<FsmComponent>();
        var resources = GameEntry.GetComponent<ResourceComponent>()?.ResourceManager as ResourceManager;
        Require(procedure != null && entities != null && fsms != null && resources != null,
            "Early shutdown must run after the native components' Awake methods.");
        Require(procedure.enabled && fsms.Count == 0 && !LunaBootstrap.IsReady && !LunaRuntimeData.IsInitialized,
            "Early fixture missed the pre-Start window: procedureEnabled=" + procedure.enabled + "; fsms=" + fsms.Count +
            "; ready=" + LunaBootstrap.IsReady + "; runtimeData=" + LunaRuntimeData.IsInitialized);
        Require(LunaRuntimeData.RuntimeAssetCount == 0 && LunaUIViews.RuntimeSpriteCount == 0 && LunaSession.Current == null,
            "Gameplay ownership was initialized before the early-shutdown fixture.");
        var saveBefore = SnapshotEarlySave();
        Require(saveBefore.ContainsKey("LunaSave.dat") && saveBefore.ContainsKey("LunaSave.dat.bak"),
            "Early shutdown requires preexisting isolated primary and backup save sentinels.");
        current.observations.Add("sceneLoaded after Awake/before Start: native FSM count=0; Procedure enabled; runtime data/session/assets absent.");

        IEnumerator shutdown = LunaBootstrap.Shutdown();
        try
        {
            // Execute the public entry synchronously inside sceneLoaded. A plain
            // next-frame test would miss the original uninitialized Procedure FSM bug.
            bool yielded = shutdown.MoveNext();
            Require(LunaBootstrap.IsShuttingDown && !procedure.enabled && fsms.Count == 0,
                "Public shutdown did not synchronously disable Procedure before Start could initialize its FSM.");
            current.observations.Add("Public Shutdown reached its first yield without accessing an uninitialized Procedure FSM; Procedure disabled synchronously.");
            if (yielded) yield return shutdown.Current;
            yield return shutdown;
        }
        finally { (shutdown as IDisposable)?.Dispose(); }

        Require(LunaBootstrap.IsShutdownComplete, "Early shutdown failed to reach its completed terminal state.");
        AssertStoppedOwners(entities, fsms, resources);
        for (int frame = 0; frame < 3; frame++)
        {
            yield return null;
            Require((procedure == null || !procedure.enabled) && !LunaBootstrap.IsReady && fsms.Count == 0,
                "Native Procedure initialized late after early shutdown on frame " + frame + ".");
            AssertStoppedOwners(entities, fsms, resources);
        }
        yield return LunaBootstrap.Shutdown();
        AssertStoppedOwners(entities, fsms, resources);
        var saveAfter = SnapshotEarlySave();
        Require(saveBefore.Count == saveAfter.Count && saveBefore.All(pair =>
                saveAfter.TryGetValue(pair.Key, out var bytes) && pair.Value.SequenceEqual(bytes)),
            "Early shutdown changed isolated save bytes or created/deleted a save file before runtime data initialized.");
        current.observations.Add("After shutdown, 3 normal frames and repeated Shutdown: native FSM/entity/resource/pending/reference-pool counters zero; all isolated save files byte-identical.");
        EndCase("Pre-Start shutdown quiesced synchronously, never initialized gameplay late, preserved saves, and was idempotent. The launcher separately requires process exit0 after repeated public Quit requests.");
    }

    private Dictionary<string, byte[]> SnapshotEarlySave() => Directory.GetFiles(report.saveDirectory, "*", SearchOption.AllDirectories)
        .ToDictionary(path => path.Substring(report.saveDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), File.ReadAllBytes, StringComparer.Ordinal);
}
#endif
