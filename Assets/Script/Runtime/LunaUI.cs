using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Luna.Gameplay
{
    // Game content identifiers, with the original serialized integer values retained.
    public enum UI_TYPE
    {
        NONE, LOADING, LOBBY, CHARACTERLIST, UPGRADE, SELECTSTAGE, PLAYER, RESULT, PROGRESSBAR, STAGETEXT, GAMEOPTION, POPUP, CHARACTERINFO, GOODS, GAMESHOP, ACCOUNTSHOP, ADVENTURE, DUNGEON, PVP, LOTTERY, SKILL, EQUIPDESIGN, SELECTPOPUP, STAGEINFO, DUNGEONINFO, INVENTORY, QUITPOPUP, ITEMINFO, SKILLINFO
    }

    public sealed class LunaUIFormData
    {
        public UI_TYPE Type { get; internal set; }
        public string AssetName { get; internal set; }
        public string GroupName { get; internal set; }
        public bool PauseCovered { get; internal set; }
    }

    /// <summary>Game UI addresses only. Native UIComponent owns forms, depth, pause and loading.</summary>
    public static class LunaUI
    {
        private static readonly Dictionary<UI_TYPE, LunaUIFormData> definitions = new Dictionary<UI_TYPE, LunaUIFormData>();
        private static readonly Dictionary<UI_TYPE, int> requestedSerials = new Dictionary<UI_TYPE, int>();
        private static UIComponent UI => GameEntry.GetComponent<UIComponent>();
        public static IEnumerable<LunaUIFormData> Definitions { get { EnsureDefinitions(); return definitions.Values; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { definitions.Clear(); requestedSerials.Clear(); }
        internal static void Shutdown() => Reset();
        public static void Register(UI_TYPE type, string assetName, string groupName, bool pauseCovered)
        {
            definitions[type] = new LunaUIFormData { Type = type, AssetName = assetName, GroupName = groupName, PauseCovered = pauseCovered };
        }
        internal static void Initialize()
        {
            ConfigureGroup("Main", 0);
            ConfigureGroup("Hud", 1);
            ConfigureGroup("Popup", 2);
            UI.InstanceCapacity = 0;
            UI.InstanceExpireTime = 0f;
            UI.InstanceAutoReleaseInterval = 0f;
            EnsureDefinitions();
        }
        private static void ConfigureGroup(string name, int depth)
        {
            if (UI.HasUIGroup(name)) UI.GetUIGroup(name).Depth = depth;
            else UI.AddUIGroup(name, depth);
        }
        public static void EnsureDefinitions()
        {
            if (definitions.Count != 0) return;
            Register(UI_TYPE.LOADING, "Luna/UI/LOADING", "Main", true);
            Register(UI_TYPE.LOBBY, "Luna/UI/LOBBY", "Main", true);
            Register(UI_TYPE.CHARACTERLIST, "Luna/UI/CHARACTERLIST", "Main", true);
            Register(UI_TYPE.UPGRADE, "Luna/UI/UPGRADE", "Main", false);
            Register(UI_TYPE.SELECTSTAGE, "Luna/UI/SELECTSTAGE", "Main", true);
            Register(UI_TYPE.PLAYER, "Luna/UI/PLAYER", "Hud", false);
            Register(UI_TYPE.RESULT, "Luna/UI/RESULT", "Popup", false);
            Register(UI_TYPE.PROGRESSBAR, "Luna/UI/PROGRESSBAR", "Hud", false);
            Register(UI_TYPE.STAGETEXT, "Luna/UI/STAGETEXT", "Hud", false);
            Register(UI_TYPE.GAMEOPTION, "Luna/UI/GAMEOPTION", "Hud", false);
            Register(UI_TYPE.POPUP, "Luna/UI/POPUP", "Popup", false);
            Register(UI_TYPE.CHARACTERINFO, "Luna/UI/CHARACTERINFO", "Popup", false);
            Register(UI_TYPE.GOODS, "Luna/UI/GOODS", "Hud", false);
            Register(UI_TYPE.GAMESHOP, "Luna/UI/GAMESHOP", "Popup", false);
            Register(UI_TYPE.ACCOUNTSHOP, "Luna/UI/ACCOUNTSHOP", "Popup", false);
            Register(UI_TYPE.PVP, "Luna/UI/PVP", "Popup", false);
            Register(UI_TYPE.LOTTERY, "Luna/UI/LOTTERY", "Popup", true);
            Register(UI_TYPE.SKILL, "Luna/UI/SKILL", "Hud", false);
            Register(UI_TYPE.EQUIPDESIGN, "Luna/UI/EQUIPDESIGN", "Popup", false);
            Register(UI_TYPE.SELECTPOPUP, "Luna/UI/SELECTPOPUP", "Popup", false);
            Register(UI_TYPE.STAGEINFO, "Luna/UI/STAGEINFO", "Popup", false);
            Register(UI_TYPE.DUNGEONINFO, "Luna/UI/DUNGEONINFO", "Popup", false);
            Register(UI_TYPE.INVENTORY, "Luna/UI/INVENTORY", "Popup", false);
            Register(UI_TYPE.QUITPOPUP, "Luna/UI/QUITPOPUP", "Popup", false);
            Register(UI_TYPE.ITEMINFO, "Luna/UI/ITEMINFO", "Popup", false);
            Register(UI_TYPE.SKILLINFO, "Luna/UI/SKILLINFO", "Popup", false);
        }
        public static int Open(UI_TYPE type)
        {
            if (type == UI_TYPE.NONE) return 0;
            EnsureDefinitions();
            if (!definitions.TryGetValue(type, out var data)) throw new ArgumentException("Unknown UI form: " + type);
            var loaded = UI.GetUIForm(data.AssetName);
            if (loaded != null) { UI.RefocusUIForm(loaded); LunaScenes.CurrentLobbyGame?.RefreshInteraction(); return loaded.SerialId; }
            if (UI.IsLoadingUIForm(data.AssetName)) return requestedSerials.TryGetValue(type, out int pending) ? pending : 0;
            int serial = UI.OpenUIForm(data.AssetName, data.GroupName, data.PauseCovered, data);
            requestedSerials[type] = serial;
            LunaScenes.CurrentLobbyGame?.RefreshInteraction();
            return serial;
        }
        public static void Close(UI_TYPE type)
        {
            if (!definitions.TryGetValue(type, out var data) || UI == null) return;
            if (requestedSerials.TryGetValue(type, out int serial) && UI.IsLoadingUIForm(serial)) UI.CloseUIForm(serial);
            requestedSerials.Remove(type);
            foreach (var form in UI.GetUIForms(data.AssetName)) UI.CloseUIForm(form);
            LunaScenes.CurrentLobbyGame?.RefreshInteraction();
        }
        internal static bool AllowsLobbyInteraction()
        {
            EnsureDefinitions();
            if (UI == null || UI.GetUIForm(definitions[UI_TYPE.LOBBY].AssetName) == null) return false;
            foreach (var data in definitions.Values)
            {
                if (data.Type == UI_TYPE.LOBBY || data.Type == UI_TYPE.GOODS || data.Type == UI_TYPE.GAMEOPTION) continue;
                if (UI.IsLoadingUIForm(data.AssetName) || UI.GetUIForm(data.AssetName) != null) return false;
            }
            return true;
        }
        public static bool IsTop(UI_TYPE type)
        {
            if (!definitions.TryGetValue(type, out var data) || UI == null) return false;
            var form = UI.GetUIForm(data.AssetName);
            return form != null && ReferenceEquals(form.UIGroup.CurrentUIForm, form);
        }
        internal static bool IsOpenOrLoading(UI_TYPE type)
        {
            return definitions.TryGetValue(type, out var data) && UI != null &&
                (UI.IsLoadingUIForm(data.AssetName) || UI.GetUIForm(data.AssetName) != null);
        }
        public static void CloseSceneForms()
        {
            requestedSerials.Clear();
            if (UI == null) return;
            UI.CloseAllLoadingUIForms();
            UI.CloseAllLoadedUIForms();
        }
    }
}
