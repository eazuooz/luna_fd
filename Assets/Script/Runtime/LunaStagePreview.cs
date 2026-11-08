using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    public static class LunaStagePreview
    {
        public static void Fill(Transform form, LunaStageDefinition stage)
        {
            if (stage == null) throw new System.ArgumentNullException(nameof(stage));
            var monsterRoot = form.Find("Monster_ScrollView/Monster_Scroll View_Viewport/Monster_Content");
            var itemRoot = form.Find("Item_ScrollView/Item_Scroll View_Viewport/Item_Content");
            var prefab = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
            var names = new HashSet<string>();
            foreach (string phase in new[] { "Start", "Repeat", "Endless" })
                foreach (var wave in LunaStageTables.GetWaves(stage.Row.Id, phase)) names.Add(wave.Prefab);
            if (!string.IsNullOrEmpty(stage.Row.Boss)) names.Add(stage.Row.Boss);
            foreach (string name in names)
            {
                var unit = LunaStageTables.RequireAsset<GameObject>(name).GetComponent<UnitView>();
                if (unit != null && unit.Data != null) Add(prefab, monsterRoot, unit.Data.profilePicture);
            }
            var rewards = new HashSet<string>();
            foreach (string trigger in new[] { "Victory", "Monster", "Boss" })
                foreach (var row in LunaStageTables.GetRewards(stage.Row.Id, trigger)) rewards.Add(row.Item);
            foreach (string name in rewards)
            {
                var item = LunaStageTables.RequireAsset<ItemData>(name);
                Add(prefab, itemRoot, LunaUIViews.CreateSprite(item.IconImage));
            }
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
