using GameFramework.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace EverydayDevup.Framework
{
    public class SoundManager : MonoBehaviour, IManager
    {
        private SoundComponent sound;
        private int musicSerial;
        private const string MusicGroup = "LunaMusic";
        public void ManagerInitialize()
        {
            sound = GameEntry.GetComponent<SoundComponent>();
            sound.AddSoundGroup(MusicGroup, false, false, 0.5f, 1);
            foreach (var source in sound.GetComponentsInChildren<AudioSource>(true))
                if (!Game.Instance.dataManager.StageSounds.Contains(source)) Game.Instance.dataManager.StageSounds.Add(source);
            var legacySource = GetComponent<AudioSource>();
            if (legacySource != null) { legacySource.Stop(); legacySource.enabled = false; }
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool battle = scene.name == "Game" || scene.name == "Dungeon";
            if (!battle)
            {
                if (musicSerial != 0) sound.StopSound(musicSerial);
                musicSerial = 0;
                return;
            }
            if (musicSerial != 0) return;
            var options = PlaySoundParams.Create();
            options.Loop = true;
            options.SpatialBlend = 0f;
            musicSerial = sound.PlaySound("Luna/Audio/Battle", MusicGroup, options);
        }
        public void ManagerClear() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        public void ManagerUpdate() { }
        public void ManagerLoop() { }
    }
}
