using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    /// <summary>Preserves Luna UI callbacks while Game Framework owns forms and their resources.</summary>
    [RequireComponent(typeof(Canvas))]
    public class UIBase : UIFormLogic, IBase
    {
        private UIAddressableData data;
        private Canvas canvas;
        private float loopElapsed;
        private bool active;
        public bool IsFullScreen => data != null && data.isFullScreen;
        public bool IsCaching => data != null && data.isCaching;
        public AssetReference AssetReference => data?.reference;
        public UI_TYPE type => data != null ? data.type : UI_TYPE.NONE;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            canvas = GetComponent<Canvas>();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            data = (UIAddressableData)userData;
            var manager = Game.Instance.uiManager;
            var rect = GetComponent<RectTransform>();
            rect.SetParent(manager.rootCanvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
            canvas.overrideSorting = true;
            manager.RegisterOpened(this);
            OnInit();
            manager.RefreshVisibility();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            StopAllCoroutines();
            // Actor previews are Entity-owned even when displayed below a UI canvas.
            // Hide them before the UI instance pool can destroy their hierarchy.
            foreach (var entity in GetComponentsInChildren<LunaEntityLogic>(true))
                if (entity != null) LunaEntities.Hide(entity.gameObject);
            OnClear();
            base.OnClose(isShutdown, userData);
        }

        protected override void OnUpdate(float elapsed, float realElapsed)
        {
            base.OnUpdate(elapsed, realElapsed);
            UIUpdate();
            loopElapsed += realElapsed;
            if (loopElapsed >= 1f) { loopElapsed = 0f; UILoop(); }
        }

        protected override void OnDepthChanged(int groupDepth, int depth)
        {
            base.OnDepthChanged(groupDepth, depth);
            if (canvas != null) canvas.sortingOrder = groupDepth + depth;
        }

        internal void SetDepth(int depth) { if (canvas != null) canvas.sortingOrder = depth; }
        public void UIInit(UIAddressableData info, int depth)
        {
            data = info; canvas = GetComponent<Canvas>(); canvas.overrideSorting = true;
            SetDepth(depth); OnInit();
        }
        public void UIActive() { if (active) return; active = true; canvas.enabled = true; OnActive(); }
        public void UIInActive() { if (!active) return; active = false; canvas.enabled = false; OnInActive(); }
        public void UIUpdate() { if (canvas != null && canvas.enabled) OnUpdate(); }
        public void UILoop() { if (canvas != null && canvas.enabled) OnLoop(); }
        public void UIClear() { Game.Instance.uiManager.Pop(type); }
        public virtual void OnInit() { transform.localScale = Vector3.one; }
        public virtual void OnActive() { }
        public virtual void OnInActive() { }
        public virtual void OnUpdate() { }
        public virtual void OnLoop() { }
        public virtual void OnClear() { }
    }
}
