#if UNITY_EDITOR || DEBUG
using System.Collections;
using System.IO;
using System.Linq;
using GameFramework;
using GameFramework.Setting;
using Luna.Gameplay;
using UnityEngine;
using UnityGameFramework.Runtime;

public sealed partial class LunaNativeValidation
{
    private IEnumerator DeveloperModeContracts()
    {
        BeginCase("development", "hotkeys-real-effects-and-normal-save-restoration");
        yield return EnsureFixtureLobby();
        Require(!LunaDeveloperMode.Active && !LunaDeveloperMode.Invincible && !LunaDeveloperMode.InfiniteMoney &&
            !LunaDeveloperMode.NoTimeLimit, "Developer cheats were enabled by default.");
        var debugger = GameEntry.GetComponent<DebuggerComponent>();
        Require(debugger.GetDebuggerWindow(LunaDeveloperMode.WindowPath) != null, "The native developer window is not registered.");
        bool wasFull = debugger.ShowFullWindow;
        Require(LunaDeveloperMode.Execute(KeyCode.F1) && debugger.ShowFullWindow != wasFull &&
            !LunaDeveloperMode.Active && !LunaUI.AllowsLobbyInteraction(), "Opening the developer menu changed progress, allowed click-through, or did not use the native window.");
        LunaDeveloperMode.Execute(KeyCode.F1);
        var data = LunaDataTable.Instance;
        var settings = GameFrameworkEntry.GetModule<ISettingManager>();
        Require(LunaRuntimeData.SaveNow(), "Normal progress checkpoint failed.");
        string baseline = settings.GetString("Luna.Progress.v1");
        string savePath = Path.Combine(LunaRuntimeData.SaveDirectory, "LunaSave.dat");
        byte[] savedBytes = File.ReadAllBytes(savePath);
        int gold = data.playerData.Gold, cards = data.UnitTable["Alice"].cardCount;
        Require(LunaDeveloperMode.Execute(KeyCode.F3) && data.UnitTable.Values.All(unit => unit.cardCount >= 1), "Not every character was unlocked.");
        Require(LunaDeveloperMode.Execute(KeyCode.F4) && data.playerData.Gold >= 1000000 && data.playerData.StarStone >= 100000,
            "The currency command did not refill its balances.");
        Require(LunaEconomy.TryDraw(true, out _) && settings.GetString("Luna.Progress.v1") == baseline &&
            File.ReadAllBytes(savePath).SequenceEqual(savedBytes), "A developer purchase modified normal saved progress.");
        Require(LunaDeveloperMode.Execute(KeyCode.F6) && data.playerData.UnlockedFloor == 30 &&
            LunaStageTables.Stages.GetAllDataRows().Where(row => !row.Key.StartsWith("TEST"))
                .All(row => LunaStageTables.IsUnlocked(row, data.playerData)), "Stage or dungeon unlock failed.");
        if (!Application.isEditor) yield return CaptureDeveloperPresentation(false);

        yield return LoadStage("Stage01-02");
        var game = (BasicDungeon)LunaScenes.CurrentGame;
        HoldRoundActors(game);
        var player = (PlayerLogic)game.Player;
        var ally = UnitLogic.From(game.gameAllyTower);
        var enemy = UnitLogic.From(game.gameEnemyTower);
        Require(LunaDeveloperMode.Execute(KeyCode.F2), "Invincibility command failed.");
        float playerHp = player.stat.Hp, allyHp = ally.stat.Hp, enemyHp = enemy.stat.Hp;
        player.Attack(enemy.gameObject, 1000000f, damageType: UnitLogic.eDamageType.POISON);
        ally.Attack(enemy.gameObject, 1000000f, damageType: UnitLogic.eDamageType.POISON);
        enemy.Attack(player.gameObject, 1f, damageType: UnitLogic.eDamageType.POISON);
        Require(player.stat.Hp == playerHp && ally.stat.Hp == allyHp && enemy.stat.Hp < enemyHp,
            "God mode failed to protect allies or also protected the enemy.");
        LunaDeveloperMode.Execute(KeyCode.F2);
        player.Attack(enemy.gameObject, 1f, damageType: UnitLogic.eDamageType.POISON);
        Require(player.stat.Hp < playerHp, "Disabling invincibility did not restore damage.");
        Require(LunaDeveloperMode.Execute(KeyCode.F5), "Battle resources command failed.");
        game.Roster.Gold = game.Roster.Mana = 0;
        yield return null;
        Require(game.Roster.Gold == game.Roster.MaxGold && game.Roster.Mana == game.Roster.MaxMana, "Battle resources did not refill on the next native update.");
        Require(LunaDeveloperMode.Execute(KeyCode.F7), "Unlimited time command failed.");
        float limit = game.Definition.Row.TimeLimit;
        try
        {
            game.Definition.Row.TimeLimit = 0.01f;
            yield return new WaitForSeconds(0.2f);
            Require(!game.isStageStop, "The round timed out while unlimited time was enabled.");
        }
        finally { game.Definition.Row.TimeLimit = limit; }
        if (!Application.isEditor) yield return CaptureDeveloperPresentation(true);
        Require(LunaDeveloperMode.Execute(KeyCode.F8), "Current stage clear command failed.");
        yield return Until(() => Active<UIResult>() != null, "Normal result after the developer clear command");
        Require(game.isStageWin && game.ResultReason == LunaGame.EndReason.Victory &&
            settings.GetString("Luna.Progress.v1") == baseline && File.ReadAllBytes(savePath).SequenceEqual(savedBytes),
            "A developer victory was misclassified or persisted its rewards.");
        foreach (string key in new[] { "Dungeon01", "Dungeon05_old" })
        {
            var row = LunaStageTables.Get(key);
            float dungeonLimit = row.TimeLimit;
            try
            {
                row.TimeLimit = 0.01f;
                yield return LoadDungeonFixture(key);
                var dungeon = LunaScenes.CurrentGame;
                HoldRoundActors(dungeon);
                yield return new WaitForSeconds(0.2f);
                Require(!dungeon.isStageStop, "Unlimited time failed in " + key);
                Require(LunaDeveloperMode.Execute(KeyCode.F8), "Developer objective clear failed in " + key);
                if (dungeon is EndlessDungeon endless)
                    yield return Until(() => endless.dungeonLevel >= 1, "Developer Endless floor clear");
                else
                {
                    yield return Until(() => Active<UIResult>() != null, "Developer boss victory");
                    Require(dungeon.isStageWin, "The developer boss clear was not a normal victory.");
                }
            }
            finally { row.TimeLimit = dungeonLimit; }
        }
        Require(LunaDeveloperMode.Execute(KeyCode.F10), "Developer exit command failed.");
        yield return Until(() => LunaScenes.CurrentScene == "Lobby" && !LunaDeveloperMode.Active &&
            !LunaDeveloperMode.IsRestoring && Active<UILobby>() != null, "Restore normal progress after the developer session");
        Require(data.playerData.Gold == gold && data.UnitTable["Alice"].cardCount == cards &&
            settings.GetString("Luna.Progress.v1") == baseline && LunaRuntimeData.Reload() &&
            data.playerData.Gold == gold && !LunaDeveloperMode.Invincible && !LunaDeveloperMode.NoTimeLimit,
            "Developer changes survived exit/reload or normal damage/time rules were not restored.");
        EndCase("Native F1 window/click blocking, F2/F3/F4/F5/F6/F7/F8 effects, paid draw, actual damage, campaign/boss/Endless unlimited timers and clears, byte-identical saves and F10 lobby restoration verified through the commands used by keyboard and buttons.");
    }

    private IEnumerator CaptureDeveloperPresentation(bool battle)
    {
        Require(SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null, "Developer presentation requires graphics.");
        yield return Until(() => battle ? Active<UIPlayer>() != null : Active<UIGoods>() != null, "Developer presentation form ready");
        yield return new WaitForSecondsRealtime(0.6f);
        var oldSize = new Vector2Int(Screen.width, Screen.height);
        try
        {
            foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(1024, 768) })
            {
                Screen.SetResolution(size.x, size.y, FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(0.2f);
                Canvas.ForceUpdateCanvases();
                UILuna form = battle ? (UILuna)Active<UIPlayer>() : Active<UIGoods>();
                string[] paths = battle ? new[] { "TimeText", "GoldText", "ManaText" } : new[] { "Gold/GoldText", "StarStone/StarStoneText" };
                Require(form.transform.Find(paths[0]).GetComponent<UnityEngine.UI.Text>().text == (battle ? "시간 무제한" : "∞"), "Developer status text is stale.");
                renderBoundsFailures.Clear();
                var texture = CaptureOffscreen(size.x, size.y, (width, height) =>
                {
                    foreach (string path in paths) MeasureUiRect(form, path, width, height);
                    var indicator = (RectTransform)LunaBootstrap.RootCanvas.transform.Find("DeveloperModeIndicator");
                    Require(indicator != null && indicator.gameObject.activeInHierarchy, "Active developer mode lacks its visible indicator.");
                    MeasureUiRect(indicator, "DeveloperModeIndicator", width, height);
                });
                try
                {
                    string name = "render-developer-" + (battle ? "battle-" : "wallet-") + size.x + "x" + size.y + ".png";
                    File.WriteAllBytes(Path.Combine(PlayerOutputDirectory, name), texture.EncodeToPNG());
                    Require(renderBoundsFailures.Count == 0, "Developer status exceeds screen bounds: " + string.Join("; ", renderBoundsFailures));
                    current.observations.Add("Developer UGUI status captured at " + size.x + "x" + size.y + ". Native IMGUI menu pixels are not part of this offscreen capture.");
                }
                finally { Destroy(texture); }
            }
        }
        finally { Screen.SetResolution(oldSize.x, oldSize.y, FullScreenMode.Windowed); }
    }
}
#endif
