using UnityEngine;
using UnityGameFramework.Runtime;
using Luna.Gameplay;

public sealed class LunaMouse : GameFrameworkComponent
{
    private void Update()
    {
        if (LunaScenes.CurrentScene != "Lobby" || !Input.GetMouseButtonDown(0)) return;
        if (LunaUI.IsTop(UI_TYPE.CHARACTERLIST) || LunaUI.IsTop(UI_TYPE.CHARACTERINFO) ||
            LunaUI.IsTop(UI_TYPE.UPGRADE) || LunaUI.IsTop(UI_TYPE.SELECTSTAGE) ||
            LunaUI.IsTop(UI_TYPE.INVENTORY) || LunaUI.IsTop(UI_TYPE.GAMESHOP) ||
            LunaUI.IsTop(UI_TYPE.STAGEINFO) || LunaUI.IsTop(UI_TYPE.DUNGEONINFO)) return;
        var camera = LunaBootstrap.GameplayCamera;
        if (camera == null) return;
        var hit = Physics2D.Raycast(camera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        var unit = hit.collider != null ? UnitLogic.From(hit.collider.gameObject) : null;
        if (unit == null || unit.IsHiding) return;
        if (unit.objectData.TypeLunaPlayer) LunaUI.Open(UI_TYPE.UPGRADE);
        else { LunaSession.Current.PickingGameObject = unit.gameObject; LunaUI.Open(UI_TYPE.CHARACTERLIST); }
    }
    public Vector3 GetClickedWorldPosition() => LunaBootstrap.GameplayCamera != null
        ? LunaBootstrap.GameplayCamera.ScreenToWorldPoint(Input.mousePosition) : Vector3.zero;
    public Vector3 GetClickedScreenPosition() => Input.mousePosition;
}
