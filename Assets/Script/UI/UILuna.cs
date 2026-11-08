using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

/// <summary>Native form layout and authored hierarchy restoration between pooled opens.</summary>
[RequireComponent(typeof(Canvas))]
public class UILuna : UIFormLogic
{
    private struct AuthoredChild
    {
        public Transform Transform, Parent;
        public bool Active;
        public Vector3 Position, Scale;
        public Quaternion Rotation;
        public Vector2 AnchorMin, AnchorMax, Pivot, SizeDelta;
        public Vector3 AnchoredPosition;
    }
    private Canvas formCanvas;
    private bool applicationQuitting;
    private readonly List<AuthoredChild> authored = new List<AuthoredChild>();
    private readonly HashSet<Transform> authoredSet = new HashSet<Transform>();
    public UI_TYPE type { get; private set; }
    protected virtual void OnApplicationQuit() { applicationQuitting = true; }
    protected override void OnInit(object userData)
    {
        base.OnInit(userData); formCanvas = GetComponent<Canvas>();
        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            authoredSet.Add(child);
            if (child != transform)
            {
                var state = new AuthoredChild { Transform = child, Parent = child.parent, Active = child.gameObject.activeSelf,
                    Position = child.localPosition, Scale = child.localScale, Rotation = child.localRotation };
                if (child is RectTransform rect)
                {
                    state.AnchorMin = rect.anchorMin; state.AnchorMax = rect.anchorMax; state.Pivot = rect.pivot;
                    state.SizeDelta = rect.sizeDelta; state.AnchoredPosition = rect.anchoredPosition3D;
                }
                authored.Add(state);
            }
        }
    }
    protected override void OnOpen(object userData)
    {
        foreach (var child in authored)
        {
            if (child.Transform == null) continue;
            child.Transform.localPosition = child.Position;
            child.Transform.localRotation = child.Rotation;
            child.Transform.localScale = child.Scale;
            if (child.Transform is RectTransform authoredRect)
            {
                authoredRect.anchorMin = child.AnchorMin; authoredRect.anchorMax = child.AnchorMax; authoredRect.pivot = child.Pivot;
                authoredRect.sizeDelta = child.SizeDelta; authoredRect.anchoredPosition3D = child.AnchoredPosition;
            }
            child.Transform.gameObject.SetActive(child.Active);
        }
        base.OnOpen(userData);
        type = ((LunaUIFormData)userData).Type;
        var rect = (RectTransform)transform; rect.SetParent(LunaBootstrap.RootCanvas.transform, false);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one; rect.localPosition = Vector3.zero;
        formCanvas.overrideSorting = true;
    }
    protected override void OnClose(bool isShutdown, object userData)
    {
        StopAllCoroutines();
        // Unity may already be destroying this hierarchy when native Procedure
        // shutdown closes its forms. There is no pooled reopen to prepare then;
        // reparenting a child during that destruction is illegal.
        if (isShutdown || applicationQuitting || !Application.isPlaying)
        {
            base.OnClose(isShutdown, userData);
            return;
        }
        LunaSpawn.CancelChildren(transform);
        foreach (var actor in GetComponentsInChildren<EntityLogicBase>(true))
            if (actor != null && !actor.IsHiding) actor.SafeHide();
        // Runtime listeners are reattached by each form's OnOpen. Unity preserves inspector bindings.
        foreach (var button in GetComponentsInChildren<Button>(true)) button.onClick.RemoveAllListeners();
        // Protect authored children if a form temporarily reparented them beneath a runtime row.
        foreach (var child in authored)
            if (child.Transform != null && child.Parent != null && child.Transform.parent != child.Parent)
                child.Transform.SetParent(child.Parent, false);
        var runtimeRoots = new List<GameObject>();
        foreach (var child in GetComponentsInChildren<Transform>(true))
            if (!authoredSet.Contains(child) && child.parent != null && authoredSet.Contains(child.parent)) runtimeRoots.Add(child.gameObject);
        foreach (var root in runtimeRoots)
        {
            // Detach immediately so close/open in one frame cannot discover retired rows.
            root.SetActive(false); root.transform.SetParent(null, false); Destroy(root);
        }
        base.OnClose(isShutdown, userData);
    }
    protected override void OnDepthChanged(int groupDepth, int depth)
    {
        base.OnDepthChanged(groupDepth, depth);
        if (formCanvas != null) formCanvas.sortingOrder = 10000 * groupDepth + 100 * depth;
    }
}
