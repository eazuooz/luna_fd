using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    public static class LunaStagePreview
    {
        public static void Layout(Transform form, bool besideMap)
        {
            var board = (RectTransform)form.Find("BackGround");
            float scale = besideMap ? 0.88f : 1f;
            if (besideMap)
            {
                form.Find("Image").gameObject.SetActive(false);
                board.sizeDelta = new Vector2(703f, 520f);
                board.anchoredPosition = new Vector2(226f, -58f);
            }
            var title = (RectTransform)board.Find("Title");
            title.sizeDelta = new Vector2(630f, 70f * scale);
            title.anchoredPosition = new Vector2(0f, 227f * scale);
            var name = title.Find("Text").GetComponent<Text>();
            name.rectTransform.sizeDelta = new Vector2(550f, 50f * scale);
            name.rectTransform.anchoredPosition = Vector2.zero;
            name.resizeTextForBestFit = true;
            name.resizeTextMinSize = 18; name.resizeTextMaxSize = 24;
            name.lineSpacing = 1f;
            var skull = title.Find("Skull") as RectTransform;
            if (skull != null) skull.anchoredPosition = new Vector2(-280f, 0f);
            foreach (var text in board.GetComponentsInChildren<Text>(true))
                if (text.transform.parent == board)
                {
                    text.rectTransform.anchoredPosition = new Vector2(-235f, (text.text.Contains("출현") ? 85f : -55f) * scale);
                    text.fontSize = 20;
                    text.lineSpacing = 1f;
                }
            foreach (string button in new[] { "SelectButton", "StartButton" })
            {
                var rect = (RectTransform)board.Find(button);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -235f * scale);
                rect.sizeDelta = new Vector2(319f, 95f * scale);
            }
            foreach (string path in new[] { "Monster_ScrollView", "Item_ScrollView" })
            {
                var rect = (RectTransform)form.Find(path);
                rect.anchoredPosition = board.anchoredPosition + new Vector2(0f, (path.StartsWith("Monster") ? 20f : -115f) * scale);
                rect.sizeDelta = new Vector2(610f, 100f * scale);
                var scroll = rect.GetComponent<ScrollRect>();
                scroll.horizontal = true;
                scroll.vertical = false;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                var content = scroll.content;
                content.anchorMin = content.anchorMax = content.pivot = new Vector2(0f, 1f);
                content.anchoredPosition = new Vector2(2f, -2f);
                content.sizeDelta = new Vector2(0f, 84f);
                var layout = content.GetComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = layout.childControlHeight = false;
                layout.childForceExpandWidth = layout.childForceExpandHeight = false;
                layout.spacing = 12f;
                var fitter = content.GetComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }
        public static void AddBriefing(Transform form, LunaStageRow row, float height)
        {
            var board = form.Find("BackGround");
            var label = new GameObject("StageBriefing", typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)label.transform;
            rect.SetParent(board, false);
            rect.anchoredPosition = new Vector2(0f, height);
            rect.sizeDelta = new Vector2(630f, 65f);
            var text = label.GetComponent<Text>();
            text.font = board.Find("Title/Text").GetComponent<Text>().font;
            text.fontSize = 18;
            text.lineSpacing = 1f;
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
            rect.localScale = Vector3.one; rect.sizeDelta = new Vector2(80f, 80f);
            var image = (RectTransform)row.transform.Find("Button");
            image.anchorMin = Vector2.zero; image.anchorMax = Vector2.one;
            image.offsetMin = image.offsetMax = Vector2.zero;
            var profile = row.GetComponent<ProfileInformation>();
            profile.DisableCountText(); profile.SetProfilePicture(sprite);
        }
    }
}
