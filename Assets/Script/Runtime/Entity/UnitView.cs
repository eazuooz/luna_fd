using System;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Authored asset bindings. No runtime stats, AI, skill execution or update loop lives here.</summary>
    public sealed class UnitView : MonoBehaviour
    {
        public ObjectData Definition;
        public ObjectData Data => LunaRuntimeData.ResolveObjectData(Definition);
        public GameObject Projectile;
        public GameObject AttackEffectPrefab;
        public AudioClip AttackSoundClip;
        public int AttackSoundType = -1;
        public string ParameterKey;
        public PrefabBinding[] Prefabs = Array.Empty<PrefabBinding>();
        public SoundBinding[] Sounds = Array.Empty<SoundBinding>();
        public NumberBinding[] Numbers = Array.Empty<NumberBinding>();
        public TextBinding[] Texts = Array.Empty<TextBinding>();
        public GameObject GetPrefab(string key)
        {
            foreach (var entry in Prefabs) if (entry.Name == key) return entry.Value;
            return null;
        }
        public AudioClip GetSound(string key)
        {
            foreach (var entry in Sounds) if (entry.Name == key) return entry.Value;
            return null;
        }
        public float GetFloat(string key, float fallback = 0f)
        {
            foreach (var entry in Numbers) if (entry.Name == key) return entry.Value;
            return LunaEntityParameters.Get(ParameterKey, key, fallback);
        }
        public bool GetBool(string key, bool fallback = false) => GetFloat(key, fallback ? 1f : 0f) != 0f;
        public string GetText(string key, string fallback = "")
        {
            foreach (var entry in Texts) if (entry.Name == key) return entry.Value;
            return fallback;
        }
    }
    [Serializable] public struct PrefabBinding { public string Name; public GameObject Value; }
    [Serializable] public struct SoundBinding { public string Name; public AudioClip Value; }
    [Serializable] public struct NumberBinding { public string Name; public float Value; }
    [Serializable] public struct TextBinding { public string Name; public string Value; }
}
