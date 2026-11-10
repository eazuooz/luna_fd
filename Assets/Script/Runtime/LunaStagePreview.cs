using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    public static class LunaStagePreview
    {
        public static void AddBriefing(Transform form, LunaStageRow row, float height)
        {
            var board = form.Find("BackGround");
            var label = new GameObject("StageBriefing", typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)label.transform;
            rect.SetParent(board, false);
            rect.anchoredPosition = new Vector2(0f, height);
            rect.sizeDelta = new Vector2(630f, 58f);
            var text = label.GetComponent<Text>();
            text.font = board.Find("Title/Text").GetComponent<Text>().font;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            string reward = row.Kind == LunaStageKind.Endless
                ? "층당 " + row.Gold + " G · 최고 " + LunaDataTable.Instance.playerData.BestEndlessFloor(row.Key) + "층"
                : HasCleared(row) ? "재도전 보상 " + row.Gold + " G · 첫 승리 보상 획득 완료"
                    : "첫 승리 " + (row.Gold + row.FirstClearGold) + " G · 재도전 " + row.Gold + " G";
            text.text = reward + "\n" + row.Tip;
        }

        public static void Fill(Transform form, LunaStageDefinition stage)
        {
            if (stage == null) throw new System.ArgumentNullException(nameof(stage));
            var monsterRoot = form.Find("Monster_ScrollView/Monster_Scroll View_Viewport/Monster_Content");
            var itemRoot = form.Find("Item_ScrollView/Item_Scroll View_Viewport/Item_Content");
            var prefab = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
            var names = new HashSet<string>();
            foreach (string phase in new[] { "Start", "Timed", "Endless", "BossPhase" })
                foreach (var wave in LunaStageTables.GetWaves(stage.Row.Id, phase)) names.Add(wave.Prefab);
            if (!string.IsNullOrEmpty(stage.Row.Boss)) names.Add(stage.Row.Boss);
            foreach (string name in names)
            {
                var unit = LunaStageTables.RequireAsset<GameObject>(name).GetComponent<UnitView>();
                if (unit != null && unit.Data != null) Add(prefab, monsterRoot, unit.Data.profilePicture);
            }
            var rewards = new HashSet<string>();
            foreach (string trigger in new[] { "Victory", "FirstClear", "Monster", "EndlessBoss" })
            {
                if (trigger == "FirstClear" && HasCleared(stage.Row)) continue;
                foreach (var row in LunaStageTables.GetRewards(stage.Row.Id, trigger)) rewards.Add(row.Item);
            }
            foreach (string name in rewards)
            {
                var item = LunaStageTables.RequireAsset<ItemData>(name);
                Add(prefab, itemRoot, LunaUIViews.CreateSprite(item.IconImage));
            }
        }
        private static bool HasCleared(LunaStageRow row)
        {
            var progress = LunaDataTable.Instance;
            return progress.StageTable.TryGetValue(row.Key, out var stage)
                ? stage.isClear : progress.playerData.ClearedDungeons.Contains(row.Key);
        }
        private static void Add(GameObject prefab, Transform parent, Sprite sprite)
        {
            if (sprite == null || parent == null) return;
            var row = Object.Instantiate(prefab, parent, false);
            var rect = row.GetComponent<RectTransform>();
            rect.localScale = Vector3.one; rect.sizeDelta = new Vector2(100f, 100f);
            var profile = row.GetComponent<ProfileInformation>();
            profile.DisableCountText(); profile.SetProfilePicture(sprite);
        }
    }
}
