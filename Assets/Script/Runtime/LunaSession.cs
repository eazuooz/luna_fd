using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Transient Luna game data. Framework services live in GameEntry, not in this state.</summary>
    public sealed class LunaSession
    {
        public static LunaSession Current { get; private set; }
        public static void Initialize(GameObject[] playerPrefabs)
        {
            if (playerPrefabs == null || playerPrefabs.Length != 7 || Array.Exists(playerPrefabs, p => p == null))
                throw new InvalidOperationException("Luna bootstrap requires its seven ordered player prefabs.");
            Current = new LunaSession
            {
                Players = (GameObject[])playerPrefabs.Clone(),
                AllStagePrefab = new List<GameObject>(Resources.LoadAll<GameObject>("Prefabs/10Stage"))
            };
            Current.SetStagePrefab("Stage01-01");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Current = null;

        public event Action<string> CurrentStageChanged;
        private string currentStageName = "Stage01-01";
        public string CurrentStageName
        {
            get => currentStageName;
            set
            {
                if (currentStageName == value) return;
                currentStageName = value;
                CurrentStageChanged?.Invoke(value);
            }
        }
        public readonly GameObject[] lobbyUnits = new GameObject[7];
        public List<GameObject> AllStagePrefab { get; private set; }
        public GameObject SelectStagePrefab { get; set; }
        public List<GameObject> HeroePrefabs { get; set; } = new List<GameObject>();
        public List<GameObject> SoldierPrefabs { get; set; } = new List<GameObject>();
        public GameObject LunaPlayer { get; set; }
        public GameObject[] Players { get; private set; }
        public int EquipDesignNumber { get; set; }
        public readonly Dictionary<int, int> pickUpData = new Dictionary<int, int>();
        public int pickItemNumber;
        public readonly List<AudioSource> StageSounds = new List<AudioSource>();
        public readonly List<AudioSource> ObjectSounds = new List<AudioSource>();
        public GameObject PickingGameObject { get; set; }
        public GameObject selectDungeon;
        public string SelectDungeonString { get; set; } = string.Empty;
        public bool doNotSummonMode;
        public GameObject chatManager;
        public string chatStr;
        public string OnClickSkillName;

        public void RefreshDeckPrefabs()
        {
            HeroePrefabs.Clear(); SoldierPrefabs.Clear();
            var deck = LunaDataTable.Instance.playerData.UnitNamesInDeck;
            for (int i = 0; i < 6; i++)
            {
                string name = deck[i];
                GameObject prefab = string.IsNullOrEmpty(name) ? null : Resources.Load<GameObject>(
                    (i < 4 ? "Prefabs/01Heroes/" : "Prefabs/02Soldiers/") + name);
                (i < 4 ? HeroePrefabs : SoldierPrefabs).Add(prefab);
            }
        }

        public void SetStagePrefab(string stageName)
        {
            var stage = AllStagePrefab.Find(p => p != null && p.name == stageName);
            if (stage == null) throw new ArgumentException("Unknown stage: " + stageName, nameof(stageName));
            SelectStagePrefab = stage;
        }
        private int StageIndex()
        {
            string value = currentStageName ?? string.Empty;
            if (value.Length != 10 || !value.StartsWith("Stage", StringComparison.Ordinal) || value[7] != '-' ||
                !int.TryParse(value.Substring(5, 2), NumberStyles.None, CultureInfo.InvariantCulture, out int chapter) ||
                !int.TryParse(value.Substring(8, 2), NumberStyles.None, CultureInfo.InvariantCulture, out int floor) ||
                chapter < 1 || chapter > 3 || floor < 1 || floor > 10)
                return 1;
            return (chapter - 1) * 10 + floor;
        }
        public string NextStageString()
        {
            int next = StageIndex() % 30;
            return "Stage" + (next / 10 + 1).ToString("D2", CultureInfo.InvariantCulture) + "-" +
                (next % 10 + 1).ToString("D2", CultureInfo.InvariantCulture);
        }
        public string NextStageLevelString() => (StageIndex() % 30 + 1).ToString(CultureInfo.InvariantCulture) + "층";
        public string CurrentStageLevelString(string prefix = "") => prefix + StageIndex().ToString(CultureInfo.InvariantCulture) + "층";
    }
}
