#if UNITY_EDITOR || DEBUG
using System;
using System.Collections;
using System.Linq;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private static LunaUIFormData UiDefinition(UI_TYPE type) => LunaUI.Definitions.Single(value => value.Type == type);
    private static UILuna NativeUi(UI_TYPE type) => GameEntry.GetComponent<UIComponent>().GetUIForm(UiDefinition(type).AssetName)?.Logic as UILuna;

    private IEnumerator OpenNativeUi(UI_TYPE type)
    {
        int serial = LunaUI.Open(type);
        Require(serial > 0, "Native UI request did not receive a serial: " + type);
        yield return Until(() => NativeUi(type) != null, "Native form open: " + type);
        var native = GameEntry.GetComponent<UIComponent>().GetUIForm(serial);
        Require(native != null && native.Logic == NativeUi(type) && NativeUi(type).type == type,
            "Native form serial, logic, or typed opening data differs: " + type);
        yield return null;
    }

    private IEnumerator CloseNativeUi(UI_TYPE type)
    {
        LunaUI.Close(type);
        var ui = GameEntry.GetComponent<UIComponent>();
        string address = UiDefinition(type).AssetName;
        yield return Until(() => !ui.IsLoadingUIForm(address) && ui.GetUIForms(address).Length == 0, "Native form close: " + type);
        yield return null;
    }

    private static bool BattleUi(UI_TYPE type) => type == UI_TYPE.PLAYER || type == UI_TYPE.SKILL ||
        type == UI_TYPE.PROGRESSBAR || type == UI_TYPE.STAGETEXT || type == UI_TYPE.SELECTPOPUP || type == UI_TYPE.RESULT;
    private static bool LobbyColliders(bool enabled) => LunaSession.Current.lobbyUnits.Where(unit => unit != null)
        .All(unit => !unit.TryGetComponent<Collider2D>(out var collider) || collider.enabled == enabled);

    private IEnumerator UiLobby()
    {
        Time.timeScale = 1f;
        if (LunaScenes.CurrentScene != "Lobby" || LunaScenes.CurrentGame != null)
        {
            LunaScenes.Load("Lobby");
            yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaScenes.IsTransitioning && NativeUi(UI_TYPE.LOBBY) != null, "UI fixture lobby");
        }
        foreach (var definition in LunaUI.Definitions)
            if (definition.Type != UI_TYPE.LOBBY && definition.Type != UI_TYPE.GOODS) LunaUI.Close(definition.Type);
        yield return OpenNativeUi(UI_TYPE.LOBBY);
        yield return Until(() => NativeUi(UI_TYPE.LOBBY) is UILobby lobby && lobby.ProfileReady && LunaSpawn.PendingCount == 0, "Lobby presentation ready");
    }

    private IEnumerator UiContracts()
    {
        var session = LunaSession.Current;
        var data = LunaDataTable.Instance;
        float originalTimeScale = Time.timeScale;
        int originalItem = session.pickItemNumber, originalDesign = session.EquipDesignNumber;
        string originalSkill = session.OnClickSkillName, originalDungeon = session.SelectDungeonString;
        GameObject originalDungeonPrefab = session.selectDungeon;
        var originalLottery = data.LotteryResult.ToArray();
        try
        {
            var definitions = LunaUI.Definitions.ToArray();
            Require(definitions.Length == 26 && definitions.Select(value => value.AssetName).Distinct().Count() == 26,
                "The native UI content contract requires all 26 distinct addresses.");
            session.pickItemNumber = data.itemTable.Keys.First();
            session.EquipDesignNumber = data.itemTable.First(pair => pair.Value is EquipDesignItemData design && design.uiPrefab != null).Key;
            session.selectDungeon = Resources.Load<GameObject>("Prefabs/11Dungeons/Dungeon01");
            Require(session.selectDungeon != null, "Dungeon info fixture has no authored Dungeon01.");
            session.SelectDungeonString = "Dungeon01";
            data.LotteryResult.Clear(); data.LotteryResult.Add("Alice");

            if (Includes("ui", "lobby-session-ownership"))
            {
                BeginCase("ui", "lobby-session-ownership");
                yield return UiLobby();
                var lobbyGame = LunaScenes.CurrentLobbyGame;
                Require(lobbyGame != null && lobbyGame.Backdrop != null, "Lobby session/backdrop is not initialized.");
                int[] ids = session.lobbyUnits.Select(unit => unit != null ? UnitLogic.From(unit).ID : 0).ToArray();
                Require(ids[6] > 0, "Lobby player has no native identity.");
                Time.timeScale = 0f;
                for (int cycle = 0; cycle < 2; cycle++)
                {
                    yield return CloseNativeUi(UI_TYPE.LOBBY);
                    yield return OpenNativeUi(UI_TYPE.LOBBY);
                    lobbyGame.RefreshRoster();
                    Require(ReferenceEquals(lobbyGame, LunaScenes.CurrentLobbyGame), "Form reopen replaced its lobby session.");
                    Require(ids.SequenceEqual(session.lobbyUnits.Select(unit => unit != null ? UnitLogic.From(unit).ID : 0)),
                        "Lobby form reopen or unchanged refresh replaced native roster lives.");
                }
                string originalHero = data.playerData.UnitNamesInDeck[0];
                try
                {
                    var alternate = Resources.LoadAll<GameObject>("Prefabs/01Heroes").First(prefab => prefab.name != originalHero && prefab.GetComponent<UnitView>() != null);
                    data.playerData.UnitNamesInDeck[0] = alternate.name;
                    lobbyGame.RefreshRoster();
                    LunaUI.Open(UI_TYPE.POPUP);
                    yield return Until(() => session.lobbyUnits[0] != null && UnitLogic.From(session.lobbyUnits[0]).ID != ids[0] && LunaSpawn.PendingCount == 0,
                        "Changed lobby slot completed asynchronously");
                    Require(Enumerable.Range(1, 6).All(index => (session.lobbyUnits[index] != null ? UnitLogic.From(session.lobbyUnits[index]).ID : 0) == ids[index]),
                        "Changing one deck slot replaced unrelated lobby actors.");
                    if (session.lobbyUnits[0].TryGetComponent<Collider2D>(out var collider))
                        Require(!collider.enabled, "Late roster completion ignored the current interaction state.");
                }
                finally
                {
                    data.playerData.UnitNamesInDeck[0] = originalHero;
                    lobbyGame.RefreshRoster(); LunaUI.Close(UI_TYPE.POPUP);
                }
                yield return Until(() => LunaSpawn.PendingCount == 0, "Restored lobby deck");
                Require(LobbyColliders(true), "Closing the popup did not restore lobby interaction.");
                yield return OpenNativeUi(UI_TYPE.UPGRADE);
                yield return OpenNativeUi(UI_TYPE.POPUP);
                Require(LobbyColliders(false), "Nested lobby forms did not block actor interaction.");
                yield return CloseNativeUi(UI_TYPE.POPUP);
                Require(LobbyColliders(false), "Closing the top popup unlocked actors beneath an open upgrade form.");
                yield return CloseNativeUi(UI_TYPE.UPGRADE);
                Require(LobbyColliders(true), "Closing the final blocking form did not restore actor interaction.");

                var hero = data.UnitTable[originalHero];
                int originalLevel = hero.Level;
                try
                {
                    yield return OpenNativeUi(UI_TYPE.UPGRADE);
                    int previousId = UnitLogic.From(session.lobbyUnits[0]).ID;
                    hero.Level = originalLevel + 1;
                    yield return CloseNativeUi(UI_TYPE.UPGRADE);
                    yield return Until(() => session.lobbyUnits[0] != null && UnitLogic.From(session.lobbyUnits[0]).ID != previousId && LunaSpawn.PendingCount == 0,
                        "Same-prefab upgrade refreshed lobby unit data");
                    Require(UnitLogic.From(session.lobbyUnits[0]).Data.Level == originalLevel + 1, "Lobby unit retained its pre-upgrade level snapshot.");
                }
                finally { hero.Level = originalLevel; lobbyGame.RefreshRoster(); }
                yield return Until(() => LunaSpawn.PendingCount == 0, "Restored lobby hero level");

                string originalEquippedSkill = data.playerData.equippedSkill[0];
                try
                {
                    yield return OpenNativeUi(UI_TYPE.UPGRADE);
                    int previousId = UnitLogic.From(session.lobbyUnits[6]).ID;
                    string replacement = originalEquippedSkill == "Thunder" ? "Healing" : "Thunder";
                    data.playerData.equippedSkill[0] = replacement;
                    yield return CloseNativeUi(UI_TYPE.UPGRADE);
                    yield return Until(() => session.lobbyUnits[6] != null && UnitLogic.From(session.lobbyUnits[6]).ID != previousId && LunaSpawn.PendingCount == 0,
                        "Same-skin loadout refreshed lobby player data");
                    Require(UnitLogic.From(session.lobbyUnits[6]).Data.PlayerLoadout.Skills[0].name == replacement,
                        "Lobby player retained its previous equipped skill snapshot.");
                }
                finally { data.playerData.equippedSkill[0] = originalEquippedSkill; lobbyGame.RefreshRoster(); }
                yield return Until(() => LunaSpawn.PendingCount == 0, "Restored lobby skill loadout");
                CheckOwnership();
                EndCase("Unchanged form reopens retained native lives; deck, same-prefab level and same-skin skill changes refreshed data; pending/nested UI correctly blocked and restored actor interaction.");
            }

            if (Includes("ui", "pending-close-and-duplicate"))
            {
                BeginCase("ui", "pending-close-and-duplicate");
                yield return UiLobby();
                yield return CloseNativeUi(UI_TYPE.ITEMINFO);
                var native = GameEntry.GetComponent<UIComponent>();
                string address = UiDefinition(UI_TYPE.ITEMINFO).AssetName;
                int first = LunaUI.Open(UI_TYPE.ITEMINFO), second = LunaUI.Open(UI_TYPE.ITEMINFO);
                Require(first > 0 && first == second, "Duplicate native UI requests received different serials.");
                Require(native.IsLoadingUIForm(first), "Pending-close fixture did not exercise an asynchronous open.");
                yield return CloseNativeUi(UI_TYPE.ITEMINFO);
                yield return Until(() => !native.IsLoadingUIForm(address), "Canceled form load settled");
                yield return OpenNativeUi(UI_TYPE.ITEMINFO);
                Require(native.GetUIForms(address).Length == 1, "Canceled/duplicate UI request left more than one native form.");
                yield return CloseNativeUi(UI_TYPE.ITEMINFO);
                CheckOwnership();
                EndCase("Duplicate opens shared one native serial; closing the pending request prevented a late form, and a new open remained unique.");
            }

            // Forms which depend on a battle run after all lobby forms; RESULT uses the normal loss transition last.
            foreach (var definition in definitions.OrderBy(value => BattleUi(value.Type) ? value.Type == UI_TYPE.RESULT ? 2 : 1 : 0).ThenBy(value => (int)value.Type))
            {
                UI_TYPE type = definition.Type;
                if (!Includes("ui", type.ToString())) continue;
                BeginCase("ui", type.ToString());
                if (!BattleUi(type)) yield return UiLobby();
                else if (!(LunaScenes.CurrentGame is BasicDungeon round) || round.isStageStop)
                    yield return LoadStage("Stage01-01");
                if (type == UI_TYPE.CHARACTERINFO) yield return OpenNativeUi(UI_TYPE.CHARACTERLIST);
                if (type == UI_TYPE.EQUIPDESIGN || type == UI_TYPE.SKILLINFO) yield return OpenNativeUi(UI_TYPE.UPGRADE);
                if (type == UI_TYPE.RESULT)
                {
                    Time.timeScale = 1f;
                    UnitLogic.From(((BasicDungeon)LunaScenes.CurrentGame).gameAllyTower).stat.Hp = 0f;
                    yield return Until(() => LunaScenes.CurrentGame.isStageStop && NativeUi(type) != null, "Normal defeat result form");
                }
                Time.timeScale = 0f;
                yield return CloseNativeUi(type);
                int firstChildren = -1;
                for (int cycle = 0; cycle < 2; cycle++)
                {
                    if (type == UI_TYPE.CHARACTERINFO) ((UICharacterList)NativeUi(UI_TYPE.CHARACTERLIST)).PickedUnitName = "Alice_Card";
                    if (type == UI_TYPE.SKILLINFO) session.OnClickSkillName = "Thunder";
                    yield return OpenNativeUi(type);
                    yield return Until(() => LunaSpawn.PendingCount == 0, "UI native previews ready: " + type);
                    var logic = NativeUi(type);
                    Require(logic != null && logic.isActiveAndEnabled, "Form did not remain active after its normal OnOpen: " + type);
                    Require(GameEntry.GetComponent<UIComponent>().GetUIForms(definition.AssetName).Length == 1,
                        "UI form has duplicate native instances: " + type);
                    if (type == UI_TYPE.ACCOUNTSHOP || type == UI_TYPE.PVP || type == UI_TYPE.POPUP)
                        Require(logic is UIPopup, "Popup alias resolved to a different implementation: " + type);
                    int children = logic.GetComponentsInChildren<Transform>(true).Length;
                    if (cycle == 0) firstChildren = children;
                    else Require(children == firstChildren, "Repeated OnOpen changed child count: " + type + " " + firstChildren + " -> " + children);
                    CheckOwnership();
                    current.observations.Add("open " + (cycle + 1) + ": native " + logic.UIForm.SerialId + ", children " + children);
                    yield return CloseNativeUi(type);
                }
                if (type == UI_TYPE.CHARACTERINFO) yield return CloseNativeUi(UI_TYPE.CHARACTERLIST);
                if (type == UI_TYPE.EQUIPDESIGN || type == UI_TYPE.SKILLINFO) yield return CloseNativeUi(UI_TYPE.UPGRADE);
                EndCase("Opened and closed the real native form twice with its required selection/scene state; native identity and hierarchy size remained consistent.");
            }
        }
        finally
        {
            Time.timeScale = originalTimeScale;
            session.pickItemNumber = originalItem; session.EquipDesignNumber = originalDesign;
            session.OnClickSkillName = originalSkill; session.SelectDungeonString = originalDungeon; session.selectDungeon = originalDungeonPrefab;
            data.LotteryResult.Clear(); data.LotteryResult.AddRange(originalLottery);
        }
    }
}
#endif
