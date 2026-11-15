using System;
using GameFramework.Debugger;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    /// <summary>Development-only commands driven by the native Procedure and Debugger modules.</summary>
    public static class LunaDeveloperMode
    {
        public const string WindowPath = "Luna";
        public static bool Available => Application.isEditor || Debug.isDebugBuild;
        public static bool Active => Available && LunaRuntimeData.DeveloperSessionActive;
        public static bool Invincible { get; private set; }
        public static bool InfiniteMoney { get; private set; }
        public static bool InfiniteBattleResources { get; private set; }
        public static bool NoTimeLimit { get; private set; }
        public static bool IsRestoring { get; private set; }
        private static bool charactersUnlocked;
        private static DebuggerComponent debugger;
        private static GameObject indicator;
        private static GameObject inputBlocker;
        public static bool MenuOpen => debugger != null && debugger.ActiveWindow && debugger.ShowFullWindow;
        private static string message = "F2~F8을 누르면 테스트를 시작합니다. 일반 진행은 보호됩니다.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            debugger = null; indicator = null; inputBlocker = null;
            Invincible = InfiniteMoney = InfiniteBattleResources = NoTimeLimit = charactersUnlocked = IsRestoring = false;
            message = "F2~F8을 누르면 테스트를 시작합니다. 일반 진행은 보호됩니다.";
        }

        internal static void Initialize()
        {
            if (!Available) return;
            debugger = GameEntry.GetComponent<DebuggerComponent>();
            if (debugger != null && debugger.GetDebuggerWindow(WindowPath) == null)
                debugger.RegisterDebuggerWindow(WindowPath, new CommandsWindow());
        }

        internal static void Tick()
        {
            if (!Available || !LunaBootstrap.IsReady || LunaBootstrap.IsShuttingDown) return;
            if (IsRestoring && LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning &&
                LunaScenes.RequestedScene == null && LunaScenes.CurrentGame == null && LunaScenes.CurrentLobbyGame != null)
            {
                LunaRuntimeData.EndDeveloperSession();
                IsRestoring = false;
                LunaScenes.CurrentLobbyGame.RefreshRoster();
                LunaUI.Close(UI_TYPE.LOBBY); LunaUI.Open(UI_TYPE.LOBBY);
                message = "테스트 종료 · 원래 재화, 캐릭터, 편성, 진행을 복원했습니다.";
            }
            for (int key = (int)KeyCode.F1; key <= (int)KeyCode.F10; key++)
                if (Input.GetKeyDown((KeyCode)key)) Execute((KeyCode)key);
            RefreshInputBlocker();
            ApplyCheats();
        }

        public static bool Execute(KeyCode key)
        {
            if (!Available || !LunaBootstrap.IsReady || LunaBootstrap.IsShuttingDown) return false;
            if (key == KeyCode.F1)
            {
                if (debugger == null) return false;
                debugger.ActiveWindow = true;
                debugger.ShowFullWindow = !debugger.ShowFullWindow;
                debugger.WindowRect = new Rect(10f, 90f, Mathf.Min(650f, Screen.width - 20f), Mathf.Min(600f, Screen.height - 100f));
                debugger.SelectDebuggerWindow(WindowPath);
                RefreshInputBlocker();
                return true;
            }
            if (IsRestoring || LunaScenes.IsTransitioning || LunaScenes.RequestedScene != null) return false;
            if (key == KeyCode.F10)
            {
                if (!Active) return false;
                Invincible = InfiniteMoney = InfiniteBattleResources = NoTimeLimit = charactersUnlocked = false;
                IsRestoring = true;
                if (indicator != null) UnityEngine.Object.Destroy(indicator);
                indicator = null;
                // The leaving round settles with its frozen normal snapshot before restoration.
                LunaScenes.Load("Lobby");
                message = "로비로 돌아가 원래 진행을 복원하는 중입니다.";
                return true;
            }
            if ((int)key < (int)KeyCode.F2 || (int)key > (int)KeyCode.F8) return false;
            if (!Active && !LunaRuntimeData.BeginDeveloperSession())
            {
                message = "현재 진행을 저장할 수 없어 테스트를 시작하지 못했습니다.";
                return false;
            }
            switch (key)
            {
                case KeyCode.F2: Invincible = !Invincible; break;
                case KeyCode.F3:
                    charactersUnlocked = true;
                    foreach (var unit in LunaDataTable.Instance.UnitTable.Values) unit.cardCount = Mathf.Max(1, unit.cardCount);
                    RefreshForm(UI_TYPE.CHARACTERLIST);
                    break;
                case KeyCode.F4: InfiniteMoney = !InfiniteMoney; break;
                case KeyCode.F5: InfiniteBattleResources = !InfiniteBattleResources; break;
                case KeyCode.F6:
                    LunaDataTable.Instance.playerData.UnlockedFloor = 30;
                    RefreshForm(UI_TYPE.SELECTSTAGE);
                    break;
                case KeyCode.F7: NoTimeLimit = !NoTimeLimit; break;
                case KeyCode.F8:
                    var round = LunaScenes.CurrentGame;
                    if (round == null || round.isStageStop) { message = "전투 중에 사용할 수 있습니다."; return false; }
                    if (round is BasicDungeon basic)
                    {
                        var tower = UnitLogic.From(basic.gameEnemyTower);
                        if (tower == null) return false;
                        tower.stat.Hp = 0f;
                    }
                    else if (round is BossDungeon boss && boss.Boss != null) boss.Boss.stat.Hp = 0f;
                    else if (round is EndlessDungeon)
                        foreach (var unit in UnitLogic.ActiveUnits)
                            if (unit.Data != null && unit.Data.RoundId == round.RoundId && unit.UnitSide == eUnitSide.Enemy) unit.stat.Hp = 0f;
                    break;
            }
            message = "테스트 중 변경은 일반 진행에 저장되지 않습니다. F10으로 원상복구합니다.";
            ShowIndicator();
            ApplyCheats();
            return true;
        }

        public static bool PreventsDamage(UnitLogic unit) => Active && Invincible && unit != null &&
            unit.Data != null && unit.Data.RoundId > 0 && unit.UnitSide == eUnitSide.Allied;

        private static void RefreshForm(UI_TYPE type)
        {
            if (!LunaUI.IsOpenOrLoading(type)) return;
            LunaUI.Close(type); LunaUI.Open(type);
        }

        private static void ApplyCheats()
        {
            if (!Active || IsRestoring) return;
            var table = LunaDataTable.Instance;
            if (InfiniteMoney) { table.playerData.Gold = Mathf.Max(table.playerData.Gold, 1000000); table.playerData.StarStone = Mathf.Max(table.playerData.StarStone, 100000); }
            if (charactersUnlocked)
                foreach (var unit in table.UnitTable.Values) unit.cardCount = Mathf.Max(1, unit.cardCount);
            var round = LunaScenes.CurrentGame;
            if (InfiniteBattleResources && round?.Roster != null)
            { round.Roster.Gold = round.Roster.MaxGold; round.Roster.Mana = round.Roster.MaxMana; }
            if (Invincible && round != null)
                foreach (var unit in UnitLogic.ActiveUnits)
                    if (PreventsDamage(unit) && unit.stat.Hp > 0f) unit.stat.Hp = unit.HealthBarHpPoint;
        }

        private static void ShowIndicator()
        {
            if (indicator != null) return;
            indicator = new GameObject("DeveloperModeIndicator", typeof(RectTransform), typeof(Canvas), typeof(Text), typeof(Shadow));
            var rect = (RectTransform)indicator.transform;
            rect.SetParent(LunaBootstrap.RootCanvas.transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-16f, -145f); rect.sizeDelta = new Vector2(500f, 30f);
            var canvas = indicator.GetComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 31000;
            var label = indicator.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18; label.alignment = TextAnchor.MiddleRight; label.color = Color.yellow; label.raycastTarget = false;
            label.text = "개발자 모드 · F1 메뉴 · F10 원상복구";
        }

        private static void RefreshInputBlocker()
        {
            if (MenuOpen && inputBlocker == null)
            {
                inputBlocker = new GameObject("DeveloperMenuInputBlocker", typeof(RectTransform), typeof(Canvas), typeof(Image), typeof(GraphicRaycaster));
                var rect = (RectTransform)inputBlocker.transform;
                rect.SetParent(LunaBootstrap.RootCanvas.transform, false);
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                var canvas = inputBlocker.GetComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 32000;
                inputBlocker.GetComponent<Image>().color = Color.clear;
            }
            if (inputBlocker != null) inputBlocker.SetActive(MenuOpen);
        }

        internal static void Shutdown()
        {
            if (indicator != null) UnityEngine.Object.Destroy(indicator);
            if (inputBlocker != null) UnityEngine.Object.Destroy(inputBlocker);
            indicator = null; inputBlocker = null; debugger = null;
            Invincible = InfiniteMoney = InfiniteBattleResources = NoTimeLimit = charactersUnlocked = IsRestoring = false;
        }

        private sealed class CommandsWindow : IDebuggerWindow
        {
            public void Initialize(params object[] args) { }
            public void Shutdown() => LunaDeveloperMode.Shutdown();
            public void OnEnter() { }
            public void OnLeave() { }
            public void OnUpdate(float elapsed, float realElapsed) { }
            public void OnDraw()
            {
                GUILayout.Label("<b>루나 개발자 모드 · 일반 저장 보호</b>");
                GUILayout.Label(message);
                GUILayout.BeginVertical("box");
                Draw(KeyCode.F2, "아군 체력 무한 (루나·동료·거점)", Invincible);
                Draw(KeyCode.F3, "모든 영웅·병사 해금", charactersUnlocked);
                Draw(KeyCode.F4, "골드·별석 무한", InfiniteMoney);
                Draw(KeyCode.F5, "전투 골드·마나 무한", InfiniteBattleResources);
                Draw(KeyCode.F6, "모든 스테이지·던전 해금", Active && LunaDataTable.Instance.playerData.UnlockedFloor == 30);
                Draw(KeyCode.F7, "제한시간 무제한", NoTimeLimit);
                if (GUILayout.Button("F8  현재 스테이지 / 무한던전 층 클리어")) Execute(KeyCode.F8);
                GUILayout.EndVertical();
                if (GUILayout.Button("F10  테스트 종료 · 로비로 복귀하고 원상복구")) Execute(KeyCode.F10);
                GUILayout.Label("F1: 메뉴 열기/닫기 · 메뉴를 닫아도 적용한 기능은 유지됩니다.");
            }
            private static void Draw(KeyCode key, string label, bool on)
            { if (GUILayout.Button(key + "  " + label + (on ? "  [ON]" : "  [OFF]"))) Execute(key); }
        }
    }
}
