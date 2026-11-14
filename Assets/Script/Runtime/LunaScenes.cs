using System;
using GameFramework.Event;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Luna.Gameplay
{
    /// <summary>Game navigation requests and queries. Native Procedure and Scene components own transitions.</summary>
    public static class LunaScenes
    {
        internal static string RequestedScene;
        public static string CurrentScene { get; internal set; } = "Loading";
        public static bool IsTransitioning { get; internal set; }
        public static string LastError { get; internal set; }
        public static LunaGame CurrentGame
        {
            get
            {
                var procedures = GameEntry.GetComponent<ProcedureComponent>();
                return procedures != null && procedures.HasProcedure<LunaBattleProcedure>()
                    ? ((LunaBattleProcedure)procedures.GetProcedure<LunaBattleProcedure>()).Game : null;
            }
        }
        public static LunaLobbyGame CurrentLobbyGame =>
            (GameEntry.GetComponent<ProcedureComponent>()?.CurrentProcedure as LunaLobbyProcedure)?.Game;
        internal static void Initialize() { RequestedScene = null; CurrentScene = "Loading"; IsTransitioning = false; LastError = null; }
        public static void Load(string sceneName)
        {
            if (sceneName != "Lobby" && sceneName != "Game" && sceneName != "Dungeon") throw new ArgumentException("Unknown game scene: " + sceneName);
            if (IsTransitioning || RequestedScene != null) return;
            RequestedScene = sceneName;
        }
        public static bool IsScenes(params string[] names) => Array.IndexOf(names, CurrentScene) >= 0;
        public static void FadeIn(float seconds, Action completed = null) => LunaBootstrap.Fade(1f, seconds, completed);
        public static void FadeOut(float seconds, Action completed = null) => LunaBootstrap.Fade(0f, seconds, completed);
    }
    public abstract class LunaProcedure : ProcedureBase
    {
        private EventComponent events;
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            events = GameEntry.GetComponent<EventComponent>();
            events.Subscribe(OpenUIFormFailureEventArgs.EventId, OnUIFailure);
        }
        private void OnUIFailure(object sender, GameEventArgs args)
        {
            var failure = (OpenUIFormFailureEventArgs)args;
            Debug.LogError("Luna UI could not open " + failure.UIFormAssetName + ": " + failure.ErrorMessage);
        }
        protected override void OnLeave(ProcedureOwner owner, bool isShutdown)
        {
            if (events != null) events.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnUIFailure);
            events = null;
            base.OnLeave(owner, isShutdown);
        }
    }
    public sealed class LunaLaunchProcedure : LunaProcedure
    {
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            var bootstrap = GameEntry.GetComponent<LunaBootstrap>();
            if (bootstrap == null) throw new InvalidOperationException("Luna presentation component is missing.");
            bootstrap.InitializeGame();
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            if (LunaBootstrap.IsReady) ChangeState<LunaLoadingProcedure>(owner);
        }
    }
    public sealed class LunaLoadingProcedure : LunaProcedure
    {
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner); LunaUI.Open(UI_TYPE.LOADING); LunaScenes.FadeOut(0.5f);
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            if (LunaScenes.RequestedScene != null) ChangeState<LunaTransitionProcedure>(owner);
        }
        protected override void OnLeave(ProcedureOwner owner, bool isShutdown)
        {
            LunaUI.Close(UI_TYPE.LOADING); base.OnLeave(owner, isShutdown);
        }
    }
    public sealed class LunaTransitionProcedure : LunaProcedure
    {
        private string target, assetName;
        private bool loading, loaded, failed;
        private SceneComponent scenes;
        private EventComponent events;
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            target = LunaScenes.RequestedScene ?? throw new InvalidOperationException("Scene transition has no destination.");
            LunaScenes.RequestedScene = null; LunaScenes.IsTransitioning = true; LunaScenes.LastError = null;
            loading = loaded = failed = false; assetName = "Assets/Scenes/" + target + ".unity";
            scenes = GameEntry.GetComponent<SceneComponent>(); events = GameEntry.GetComponent<EventComponent>();
            events.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoaded);
            events.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadFailed);
            events.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadFailed);
            // The leaving procedure has already shut down its round before any scene is unloaded.
            LunaUI.CloseSceneForms(); LunaSpawn.CancelRound(0); Time.timeScale = 1f;
            foreach (string oldScene in scenes.GetLoadedSceneAssetNames()) scenes.UnloadScene(oldScene, this);
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            if (failed)
            {
                LunaScenes.CurrentScene = "Loading"; LunaScenes.IsTransitioning = false;
                ChangeState<LunaLoadingProcedure>(owner); return;
            }
            if (!loading && scenes.GetUnloadingSceneAssetNames().Length == 0)
            { loading = true; scenes.LoadScene(assetName, this); }
            if (!loaded) return;
            LunaScenes.CurrentScene = target; LunaScenes.IsTransitioning = false;
            if (target == "Lobby") ChangeState<LunaLobbyProcedure>(owner);
            else ChangeState<LunaBattleProcedure>(owner);
        }
        private void OnLoaded(object sender, GameEventArgs args)
        {
            var result = (LoadSceneSuccessEventArgs)args;
            if (result.SceneAssetName == assetName && ReferenceEquals(result.UserData, this)) loaded = true;
        }
        private void OnLoadFailed(object sender, GameEventArgs args)
        {
            var failure = (LoadSceneFailureEventArgs)args;
            if (failure.SceneAssetName != assetName || !ReferenceEquals(failure.UserData, this)) return;
            Fail("Scene load failed: " + failure.ErrorMessage);
        }
        private void OnUnloadFailed(object sender, GameEventArgs args)
        {
            var failure = (UnloadSceneFailureEventArgs)args;
            if (ReferenceEquals(failure.UserData, this)) Fail("Scene unload failed: " + failure.SceneAssetName);
        }
        private void Fail(string message) { failed = true; LunaScenes.LastError = message; Debug.LogError(message); }
        protected override void OnLeave(ProcedureOwner owner, bool isShutdown)
        {
            if (events != null)
            {
                events.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoaded);
                events.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadFailed);
                events.Unsubscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadFailed);
            }
            events = null; scenes = null;
            base.OnLeave(owner, isShutdown);
        }
    }
    public sealed class LunaLobbyGame
    {
        private bool active;
        private int backdropId;
        private int musicSerial;
        private LunaSession session;
        private readonly GameObject[] rosterPrefabs = new GameObject[7];
        private readonly int[] rosterIds = new int[7];
        private readonly int[] rosterVersions = new int[7];
        private readonly string[] rosterStates = new string[7];
        private bool interactionEnabled;
        public LunaStageBackdropLogic Backdrop => LunaSpawn.Find<LunaStageBackdropLogic>(backdropId);
        public void Initialize()
        {
            active = true;
            session = LunaSession.Current;
            var music = GameFramework.Sound.PlaySoundParams.Create();
            music.Loop = true; music.SpatialBlend = 0f;
            musicSerial = GameEntry.GetComponent<SoundComponent>().PlaySound("Luna/Audio/Lobby", "LunaMusic", music);
            RefreshRoster();
            var camera = LunaBootstrap.GameplayCamera.transform;
            camera.position = new Vector3(0f, 1.865f, camera.position.z);
            var prefab = Resources.Load<GameObject>("Prefabs/08StageBackground/Background_Lobby");
            var data = LunaEntityData.Create<LunaStageEntityData>();
            data.Position = new Vector3(-3f, -0.61f, 0f);
            backdropId = LunaSpawn.Show<LunaStageBackdropLogic>(prefab, data, actor =>
            {
                if (!active || LunaScenes.CurrentScene != "Lobby") { actor.SafeHide(); return; }
                LunaUI.Open(UI_TYPE.LOBBY); LunaUI.Open(UI_TYPE.GOODS); LunaUI.Open(UI_TYPE.GAMEOPTION);
                LunaScenes.FadeOut(0.5f);
            }, error => { if (active) Debug.LogError("Lobby scenery could not load: " + error); });
        }
        public void RefreshRoster()
        {
            if (!active || session == null || !ReferenceEquals(session, LunaSession.Current)) return;
            session.RefreshDeckPrefabs();
            LunaDataTable.Instance.playerData.SetCurPlayer();
            for (int i = 0; i < rosterIds.Length; i++)
            {
                GameObject prefab = i == 6 ? LunaDataTable.Instance.playerData.CurPlayer() :
                    i < 4 ? session.HeroePrefabs[i] : session.SoldierPrefabs[i - 4];
                string state = RosterState(prefab, i == 6);
                // Reopening presentation or saving the same selection must not replace native lives.
                if (rosterPrefabs[i] == prefab && rosterStates[i] == state) continue;
                int index = i, version = ++rosterVersions[i];
                LunaSpawn.Cancel(rosterIds[i]);
                rosterIds[i] = 0; rosterPrefabs[i] = prefab; rosterStates[i] = state;
                if (i == 6 && session.LunaPlayer == session.lobbyUnits[i]) session.LunaPlayer = null;
                session.lobbyUnits[i] = null;
                if (prefab == null) continue;
                var position = new Vector3(i == 6 ? 1.5f : i - 5f, 0.1f, 0f);
                var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), position);
                data.InitialMode = UnitSpawnMode.Lobby;
                rosterIds[i] = data.Id;
                LunaSpawn.Show<UnitLogic>(prefab, data, actor =>
                {
                    if (!active || version != rosterVersions[index] || !ReferenceEquals(session, LunaSession.Current) ||
                        LunaScenes.CurrentLobbyGame != this || LunaScenes.CurrentScene != "Lobby")
                    { actor.SafeHide(); return; }
                    session.lobbyUnits[index] = actor.gameObject;
                    if (index == 6) session.LunaPlayer = actor.gameObject;
                    if (actor.TryGetComponent<Collider2D>(out var collider)) collider.enabled = LunaUI.AllowsLobbyInteraction();
                }, error =>
                {
                    if (!active || version != rosterVersions[index]) return;
                    rosterIds[index] = 0; rosterPrefabs[index] = null; rosterStates[index] = null;
                    Debug.LogError("Lobby unit could not load: " + error);
                });
            }
        }
        private static string RosterState(GameObject prefab, bool player)
        {
            if (prefab == null) return null;
            var table = LunaDataTable.Instance;
            var progress = table.playerData;
            var parts = new System.Collections.Generic.List<string>
            {
                JsonUtility.ToJson(prefab.GetComponent<UnitView>().Data),
                progress.UnitMoveSpeedLevel.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            if (player)
            {
                parts.Add(string.Join(",", progress.LunaLevel, progress.LunaMoveSpeedLevel, progress.GoldSpeedLevel,
                    progress.TowerHPLevel, progress.AuroraLevel, progress.LunaReviveTimeLevel));
                foreach (string key in progress.equippedSkill) { parts.Add(key); parts.Add(JsonUtility.ToJson(table.skiiTable[key])); }
                var design = table.GetCurrentEquipDesignItemData();
                if (design != null)
                    foreach (var skill in design.equipSkills)
                        if (skill != null) { parts.Add(skill.name); parts.Add(JsonUtility.ToJson(table.skiiTable[skill.name])); }
            }
            return string.Join("|", parts);
        }
        public void RefreshInteraction()
        {
            if (!active || session == null) return;
            bool value = LunaUI.AllowsLobbyInteraction();
            if (value == interactionEnabled) return;
            interactionEnabled = value;
            // Returning from an upgrade screen snapshots changed stats/loadout even when its prefab stayed the same.
            if (value) RefreshRoster();
            foreach (var unit in session.lobbyUnits)
                if (unit != null && unit.TryGetComponent<Collider2D>(out var collider)) collider.enabled = value;
        }
        public void OpenDoor() => Backdrop?.OpenDoor();
        public void Shutdown()
        {
            active = false;
            if (musicSerial != 0) GameEntry.GetComponent<SoundComponent>()?.StopSound(musicSerial);
            musicSerial = 0;
            LunaSpawn.Cancel(backdropId);
            LunaUI.CloseSceneForms();
            for (int i = 0; i < rosterIds.Length; i++)
            {
                rosterVersions[i]++;
                LunaSpawn.Cancel(rosterIds[i]); rosterIds[i] = 0; rosterPrefabs[i] = null; rosterStates[i] = null;
                if (session != null)
                {
                    if (i == 6 && session.LunaPlayer == session.lobbyUnits[i]) session.LunaPlayer = null;
                    session.lobbyUnits[i] = null;
                }
            }
            LunaSpawn.CancelRound(0);
            session = null;
        }
    }
    public sealed class LunaLobbyProcedure : LunaProcedure
    {
        public LunaLobbyGame Game { get; private set; }
        protected override void OnEnter(ProcedureOwner owner) { base.OnEnter(owner); Game = new LunaLobbyGame(); Game.Initialize(); }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            Game?.RefreshInteraction();
            if (LunaScenes.RequestedScene != null) ChangeState<LunaTransitionProcedure>(owner);
        }
        protected override void OnLeave(ProcedureOwner owner, bool isShutdown)
        {
            Game?.Shutdown(); Game = null; base.OnLeave(owner, isShutdown);
        }
    }
    public sealed class LunaBattleProcedure : LunaProcedure
    {
        public LunaGame Game { get; private set; }
        protected override void OnEnter(ProcedureOwner owner)
        {
            base.OnEnter(owner);
            var session = LunaSession.Current;
            var prefab = LunaScenes.CurrentScene == "Game" ? session.SelectStagePrefab : session.selectDungeon;
            if (prefab == null) throw new InvalidOperationException("No stage selected for " + LunaScenes.CurrentScene);
            var source = prefab.GetComponent<LunaStageDefinition>();
            if (source == null) throw new InvalidOperationException("Stage has no authored definition: " + prefab.name);
            switch (source.Kind)
            {
                case LunaStageKind.Basic: Game = new BasicDungeon(); break;
                case LunaStageKind.Boss: Game = new BossDungeon(); break;
                case LunaStageKind.Endless: Game = new EndlessDungeon(); break;
                default: throw new InvalidOperationException("Unsupported Luna stage kind.");
            }
            session.doNotSummonMode = source.Row.Key == "Dungeon04";
            Game.Initialize(source); LunaScenes.FadeOut(0.5f);
        }
        protected override void OnUpdate(ProcedureOwner owner, float elapsed, float realElapsed)
        {
            base.OnUpdate(owner, elapsed, realElapsed);
            if (LunaScenes.RequestedScene == null) Game?.Update(elapsed, realElapsed);
            if (Game != null && Game.FailureMessage != null && LunaScenes.RequestedScene == null) LunaScenes.Load("Lobby");
            if (LunaScenes.RequestedScene != null) ChangeState<LunaTransitionProcedure>(owner);
        }
        protected override void OnLeave(ProcedureOwner owner, bool isShutdown)
        {
            LunaUI.CloseSceneForms();
            Game?.Shutdown(); Game = null;
            base.OnLeave(owner, isShutdown);
        }
    }
}
