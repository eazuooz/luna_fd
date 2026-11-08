using Luna.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class RButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private int pressedPlayerId;
    public void OnPointerDown(PointerEventData eventData)
    {
        var player = LunaSession.Current != null ? UnitLogic.From(LunaSession.Current.LunaPlayer) as PlayerLogic : null;
        if (player == null || player.IsHiding || player.stat.Hp <= 0f) return;
        pressedPlayerId = player.ID;
        player.SetMovement(1);
    }
    public void OnPointerUp(PointerEventData eventData) => Release();
    private void OnDisable() => Release();
    private void Release()
    {
        var player = LunaSpawn.Find<PlayerLogic>(pressedPlayerId);
        pressedPlayerId = 0;
        if (player != null && player.Moving == 1) player.SetMovement(0);
    }
}
