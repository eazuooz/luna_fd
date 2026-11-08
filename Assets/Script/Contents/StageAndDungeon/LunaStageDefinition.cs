using System.Collections.Generic;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Stage selection asset binding only. Round rules and progression live in native DataTables and the Procedure-owned game.</summary>
    public sealed class LunaStageDefinition : MonoBehaviour
    {
        public string TableKey;
        public DungeonTable dungeonTable;
        public LunaStageRow Row => LunaStageTables.Get(string.IsNullOrEmpty(TableKey) ? name : TableKey);
        public LunaStageKind Kind => Row.Kind;
        public string stageName => Row.DisplayName;
        public string DungeonName => Row.DisplayName;
        public bool isBossStage => Row.IsBossStage;
        public GameObject backgroundImage => LunaStageTables.RequireAsset<GameObject>(Row.Background);
        public GameObject bossMonster => LunaStageTables.RequireAsset<GameObject>(Row.Boss);
        public List<GameObject> Monsters
        {
            get
            {
                var result = new List<GameObject>();
                foreach (var row in LunaStageTables.GetWaves(Row.Id, Kind == LunaStageKind.Endless ? "Endless" : "Repeat"))
                    result.Add(LunaStageTables.RequireAsset<GameObject>(row.Prefab));
                return result;
            }
        }
        public List<LunaStageRewardRow> ItemDropTable => LunaStageTables.GetRewards(Row.Id, "Victory");
        public List<LunaStageRewardRow> MonterDropTable => LunaStageTables.GetRewards(Row.Id, "Monster");
    }
}
