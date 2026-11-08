#if UNITY_EDITOR || DEBUG
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Luna.Gameplay;

public sealed partial class LunaNativeValidation
{
    private sealed class CanvasCaptureState
    {
        public Canvas canvas;
        public RenderMode mode;
        public Camera camera;
        public float plane;
    }

    // Explicit diagnostic fallback. This renders the live scene but temporarily converts
    // overlay canvases to camera-space; it is not evidence of an OS-window backbuffer.
    private Texture2D CaptureOffscreen(int width, int height)
    {
        Camera camera = LunaBootstrap.GameplayCamera;
        if (!camera) camera = Camera.main;
        Require(camera, "No live gameplay camera for offscreen capture.");
        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        target.name = "Luna diagnostic offscreen target";
        target.Create();
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        int previousMask = camera.cullingMask;
        var canvases = new List<CanvasCaptureState>();
        Texture2D pixels = null;
        try
        {
            camera.targetTexture = target;
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude).Where(c => c.isRootCanvas))
            {
                canvases.Add(new CanvasCaptureState { canvas = canvas, mode = canvas.renderMode, camera = canvas.worldCamera, plane = canvas.planeDistance });
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = camera.nearClipPlane + 1f;
                    camera.cullingMask |= 1 << canvas.gameObject.layer;
                }
            }
            Canvas.ForceUpdateCanvases();
            MeasureCriticalUiBounds(width, height);
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                var request = new RenderPipeline.StandardRequest { destination = target };
                Require(RenderPipeline.SupportsRenderRequest(camera, request), "Active pipeline does not support a standard explicit render request.");
                RenderPipeline.SubmitRenderRequest(camera, request);
            }
            else camera.Render();
            RenderTexture.active = target;
            pixels = new Texture2D(width, height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            pixels.Apply(false, false);
            current.observations.Add("Explicit pipeline render to " + width + "x" + height + "; " + canvases.Count + " root canvases inspected, overlay canvases temporarily camera-space. Physical display/backbuffer not validated.");
            return pixels;
        }
        catch
        {
            if (pixels != null) Destroy(pixels);
            throw;
        }
        finally
        {
            foreach (CanvasCaptureState state in canvases)
            {
                if (!state.canvas) continue;
                state.canvas.renderMode = state.mode; state.canvas.worldCamera = state.camera; state.canvas.planeDistance = state.plane;
            }
            camera.targetTexture = previousTarget; camera.cullingMask = previousMask;
            RenderTexture.active = previousActive;
            target.Release(); Destroy(target);
            Canvas.ForceUpdateCanvases();
        }
    }

    private readonly List<string> renderBoundsFailures = new List<string>();
    private void MeasureCriticalUiBounds(int width, int height)
    {
        renderBoundsFailures.Clear();
        UIPlayer player = Active<UIPlayer>();
        foreach (string name in new[] { "ButtonLeft", "ButtonRight" }) MeasureUiRect(player, name, width, height);
        UISkill skill = Active<UISkill>();
        for (int i = 0; i < 3; i++) MeasureUiRect(skill, "Skill_Button_" + i, width, height);
        UIGameOption options = Active<UIGameOption>();
        foreach (string name in new[] { "ButtonOption", "ButtonSound" }) MeasureUiRect(options, name, width, height);
        UIProgressBar progress = Active<UIProgressBar>();
        foreach (string name in new[] { "MinimapImage", "MinimapText", "TitleText", "Text", "Text (1)", "Text (2)" }) MeasureUiRect(progress, name, width, height);
        Text[] distanceLabels = progress ? progress.GetComponentsInChildren<Text>().Where(t => t.name == "DistanceText").ToArray() : new Text[0];
        if (distanceLabels.Length == 0) renderBoundsFailures.Add("No active minimap distance label.");
        foreach (Text label in distanceLabels) MeasureUiRect(label.rectTransform, "UIProgressBar/" + label.transform.parent.name + "/DistanceText", width, height);
    }
    private void MeasureUiRect(Component owner, string path, int width, int height)
    {
        Transform child = owner ? owner.transform.Find(path) : null;
        MeasureUiRect(child as RectTransform, (owner ? owner.GetType().Name : "missing form") + "/" + path, width, height);
    }
    private void MeasureUiRect(RectTransform rect, string name, int width, int height)
    {
        if (!rect || !rect.gameObject.activeInHierarchy) { renderBoundsFailures.Add(name + " is missing or inactive."); return; }
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (!canvas) { renderBoundsFailures.Add(name + " has no Canvas."); return; }
        canvas = canvas.rootCanvas;
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && !camera) { renderBoundsFailures.Add(name + " has no render camera."); return; }
        Vector3[] corners = new Vector3[4]; rect.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity), max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector3 point = camera ? camera.WorldToViewportPoint(corner) : corner;
            if (camera) { if (point.z <= 0) renderBoundsFailures.Add(name + " is behind its camera."); point.x *= width; point.y *= height; }
            min = Vector2.Min(min, point); max = Vector2.Max(max, point);
        }
        current.observations.Add("Critical UI bounds " + name + ": [" + min.x.ToString("F2") + "," + min.y.ToString("F2") + "]..[" + max.x.ToString("F2") + "," + max.y.ToString("F2") + "] in " + width + "x" + height);
        const float tolerance = 1f;
        if (min.x < -tolerance || min.y < -tolerance || max.x > width + tolerance || max.y > height + tolerance || max.x <= min.x || max.y <= min.y)
            renderBoundsFailures.Add(name + " is clipped or has no area: " + min + ".." + max);
    }
}
#endif
