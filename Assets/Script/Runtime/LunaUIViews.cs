using System.Collections.Generic;
using UnityEngine;

namespace Luna.Gameplay
{
    public static class LunaUIViews
    {
        private static readonly Dictionary<Texture2D, Sprite> sprites = new Dictionary<Texture2D, Sprite>();
        public static int RuntimeSpriteCount => sprites.Count;
        public static Sprite[] GetRuntimeSprites()
        {
            var result = new Sprite[sprites.Count];
            sprites.Values.CopyTo(result, 0);
            return result;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => ReleaseRuntimeSprites(!Application.isPlaying);
        internal static void ReleaseRuntimeSprites(bool immediate = false)
        {
            foreach (var sprite in sprites.Values)
            {
                if (sprite == null) continue;
                if (immediate) Object.DestroyImmediate(sprite);
                else Object.Destroy(sprite);
            }
            sprites.Clear();
        }
        public static GameObject FindChildObject(GameObject root, string name)
        {
            if (root == null) return null;
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child.gameObject;
            return null;
        }
        public static List<GameObject> FindChildObjects(GameObject root, params string[] names)
        {
            var matches = new List<GameObject>();
            foreach (string name in names)
                if (root != null) foreach (var child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == name) matches.Add(child.gameObject);
            return matches;
        }
        public static T FindComponentByChildObject<T>(GameObject root, string name) where T : Component
        {
            var child = FindChildObject(root, name);
            return child != null ? child.GetComponent<T>() : null;
        }
        public static GameObject FindWithTagGameObject(string tag) => GameObject.FindWithTag(tag);
        public static T FindWithTagGameObjectComponent<T>(string tag) where T : Component
        {
            var target = GameObject.FindWithTag(tag);
            return target != null ? target.GetComponent<T>() : null;
        }
        public static Sprite CreateSprite(Texture source)
        {
            var texture = source as Texture2D;
            if (texture == null) return null;
            if (sprites.TryGetValue(texture, out var cached) && cached != null) return cached;
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f);
            sprite.hideFlags = HideFlags.DontSave;
            sprites[texture] = sprite;
            return sprite;
        }
    }
}
