using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.Gameplay
{
    /// <summary>Shared presentation primitives. Native forms retain ownership of every created child.</summary>
    public static class LunaUIPresentation
    {
        public static readonly Color Ink = new Color32(19, 33, 37, 246);
        public static readonly Color Paper = new Color32(58, 64, 55, 245);
        public static readonly Color Gold = new Color32(239, 198, 108, 255);
        public static readonly Color Muted = new Color32(180, 189, 185, 255);
        public static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }
        public static Image Panel(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool border = true)
        {
            var rect = Rect(parent, name, position, size);
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false;
            if (border) { var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color32(143, 132, 95, 255); outline.effectDistance = new Vector2(1f, -1f); }
            return image;
        }
        public static Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize = 20, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = Rect(parent, name, position, size).gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = fontSize;
            label.text = value; label.color = Color.white; label.lineSpacing = 1f; label.alignment = alignment; label.raycastTarget = false;
            return label;
        }
        public static Button Button(Transform parent, string name, string caption, Vector2 position, Vector2 size, Action click, bool primary = false)
        {
            var image = Panel(parent, name, position, size, primary ? new Color32(22, 91, 92, 255) : new Color32(48, 58, 59, 255));
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.onClick.AddListener(() => click());
            Label(image.transform, "Caption", caption, Vector2.zero, size - new Vector2(12f, 4f), 20, TextAnchor.MiddleCenter);
            return button;
        }
        public static void Caption(Button button, string caption)
        {
            var text = button.transform.Find("Text")?.GetComponent<Text>();
            if (text != null) { text.text = caption; text.fontSize = 24; text.lineSpacing = 1f; }
        }
        public static void CardQuantity(Text label, int count)
        {
            label.text = "카드 " + count + "장"; label.fontSize = 12; label.alignment = TextAnchor.MiddleCenter;
            var plate = (RectTransform)label.transform.parent;
            plate.anchoredPosition = new Vector2(40f, 49f); plate.sizeDelta = new Vector2(74f, 22f);
            label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(74f, 22f);
        }
        public static void Prepare(UILuna form, string group)
        {
            foreach (var text in form.GetComponentsInChildren<Text>(true)) text.lineSpacing = 1f;
            if ((group != "Popup" && form.type != UI_TYPE.UPGRADE) || form.type == UI_TYPE.STAGEINFO || form.type == UI_TYPE.ITEMINFO || form.type == UI_TYPE.LOTTERY) return;
            var shade = Panel(form.transform, "ModalShade", Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.58f), false);
            var rect = shade.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling(); shade.raycastTarget = true;
        }
        private static readonly string[] Starter = { "Alice", "Lene", "Dren", "Tira", "PorongSword", "PorongArcher" };
        public static bool Owned(ObjectData unit) => unit != null && (unit.cardCount > 0 || unit.Level > 1 || unit.ultState > eUltState.STAR1 || Starter.Contains(unit.name));
        public static string Role(ObjectData unit) => (unit.UnitType == eUnitType.Hero ? "영웅" : "병사") + " · " + (unit.AttackRange >= 3f ? "원거리" : "근접");
        public static void ConfigurePartyButton(Transform form)
        {
            var button = form.Find("BackGround/SelectButton").GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                var map = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>().GetUIForm("Luna/UI/SELECTSTAGE")?.Logic as UISelectDungeon;
                if (map != null) map.OpenParty();
                else { LunaUI.Close(UI_TYPE.STAGEINFO); LunaUI.Close(UI_TYPE.DUNGEONINFO); LunaUI.Open(UI_TYPE.CHARACTERLIST); }
            });
            Caption(button, "편성 변경");
            var hint = button.transform.Find("LevelText"); if (hint != null) hint.gameObject.SetActive(false);
            button.transform.Find("Text").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}
