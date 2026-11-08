using System;
using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;

/// <summary>One native scenery actor owns its explicit plate, demon and ambient emitter presentation.</summary>
public sealed class LunaStageBackdropLogic : EntityLogicBase
{
    private LunaStageBackdropView view;
    private Vector3[] plateOrigins;
    private Vector3[] demonOrigins;
    private float[] emitterTimers;
    private float age;
    private Vector3 doorOrigin;
    private bool openingDoor;
    private Vector3 sparkleOrigin;
    private bool sparklesInitiallyActive;
    private float sparkleAge;
    public bool IsDoorOpen => view != null && view.Door != null && view.Door.localPosition.y >= 2.5f;
    private readonly HashSet<int> pendingEffects = new HashSet<int>();

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        view = GetComponent<LunaStageBackdropView>();
        if (view == null) throw new InvalidOperationException("Backdrop has no authored scenery bindings: " + name);
        plateOrigins = new Vector3[view.ScrollingPlates.Length];
        demonOrigins = new Vector3[view.WanderingDemons.Length];
        emitterTimers = new float[view.Emitters.Length];
        doorOrigin = view.Door != null ? view.Door.localPosition : Vector3.zero;
        if (view.LobbySparkles != null)
        {
            sparkleOrigin = view.LobbySparkles.localPosition;
            sparklesInitiallyActive = view.LobbySparkles.gameObject.activeSelf;
        }
        for (int i = 0; i < plateOrigins.Length; i++) plateOrigins[i] = view.ScrollingPlates[i].localPosition;
        for (int i = 0; i < demonOrigins.Length; i++) demonOrigins[i] = view.WanderingDemons[i].Root.localPosition;
    }
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        age = 0f; pendingEffects.Clear(); openingDoor = false;
        sparkleAge = 0f;
        if (view.LobbySparkles != null)
        {
            view.LobbySparkles.localPosition = sparkleOrigin;
            view.LobbySparkles.gameObject.SetActive(sparklesInitiallyActive);
        }
        if (view.Door != null) view.Door.localPosition = doorOrigin;
        if (view.DoorSound != null) view.DoorSound.Stop();
        for (int i = 0; i < plateOrigins.Length; i++) view.ScrollingPlates[i].localPosition = plateOrigins[i];
        for (int i = 0; i < demonOrigins.Length; i++)
        {
            var binding = view.WanderingDemons[i];
            binding.Root.localPosition = demonOrigins[i];
            binding.Animation.Skeleton.ScaleX = 1f;
            if (binding.Animation.Skeleton.Data.FindAnimation("idle") != null)
                binding.Animation.AnimationState.SetAnimation(0, "idle", true);
        }
        for (int i = 0; i < emitterTimers.Length; i++) emitterTimers[i] = 0f;
    }
    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        if (view.LobbySparkles != null && view.LobbySparkles.gameObject.activeInHierarchy)
        {
            sparkleAge += elapsed;
            var position = sparkleOrigin;
            // The authored motion was 0.2 units/frame at 60 FPS, between -1.5 and 6.
            position.x = -1.5f + Mathf.PingPong(sparkleOrigin.x + 1.5f - sparkleAge * 12f, 7.5f);
            view.LobbySparkles.localPosition = position;
        }
        if (openingDoor && view.Door != null)
        {
            var position = view.Door.localPosition;
            // Original movement was +0.02 per frame. Preserve its 60 FPS speed without frame-rate dependence.
            position.y = Mathf.MoveTowards(position.y, 2.5f, 1.2f * elapsed);
            view.Door.localPosition = position;
            if (position.y >= 2.5f) openingDoor = false;
        }
        age += elapsed;
        float scroll = Mathf.Repeat(age * 0.1f, 13.6f);
        for (int i = 0; i < plateOrigins.Length; i++)
            view.ScrollingPlates[i].localPosition = plateOrigins[i] + Vector3.left * scroll;
        float demonAge = Mathf.Repeat(age, 12f);
        float demonOffset = demonAge < 4f ? demonAge * 0.5f : demonAge < 6f ? 2f : demonAge < 10f ? (10f - demonAge) * 0.5f : 0f;
        for (int i = 0; i < demonOrigins.Length; i++)
        {
            var binding = view.WanderingDemons[i];
            binding.Root.localPosition = demonOrigins[i] + Vector3.right * demonOffset;
            binding.Animation.Skeleton.ScaleX = demonAge < 6f ? 1f : -1f;
        }
        for (int i = 0; i < view.Emitters.Length; i++)
        {
            var emitter = view.Emitters[i];
            if (!emitter.gameObject.activeInHierarchy) continue;
            var prefab = emitter.GetPrefab("effect");
            if (prefab == null) continue;
            emitterTimers[i] -= elapsed;
            if (emitterTimers[i] > 0f) continue;
            emitterTimers[i] = Mathf.Max(0.1f, emitter.GetFloat("delayTime", 1f));
            var data = EffectData.Create(emitter.transform.position, ID, false, SpawnData.RoundId);
            data.Parent = emitter.transform; data.Rotation = emitter.transform.rotation;
            int id = data.Id; pendingEffects.Add(id);
            LunaSpawn.Show<EffectLogic>(prefab, data, _ => pendingEffects.Remove(id), _ => pendingEffects.Remove(id));
        }
    }
    public void OpenDoor()
    {
        if (IsHiding || !Available || view.Door == null || openingDoor || IsDoorOpen) return;
        openingDoor = true;
        if (view.DoorSound != null) view.DoorSound.Play();
    }
    protected override void OnHide(bool isShutdown, object userData)
    {
        try
        {
            openingDoor = false;
            if (view != null && view.DoorSound != null) view.DoorSound.Stop();
            foreach (int id in pendingEffects) LunaSpawn.Cancel(id);
            pendingEffects.Clear();
            var entities = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.EntityComponent>();
            if (entities != null)
                foreach (var entity in entities.GetAllLoadedEntities())
                    if (entity.Logic is EntityLogicBase child && child.SpawnData != null && child.SpawnData.OwnerId == ID)
                        child.SafeHide();
        }
        finally { base.OnHide(isShutdown, userData); }
    }
}
public sealed class LunaStageEntityData : LunaEntityData { }
