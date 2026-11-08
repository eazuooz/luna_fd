using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EverydayDevup.Framework
{
    /// <summary>Luna scene presentation invoked by Game Framework procedures.</summary>
    public class LunaSceneManager : MonoBehaviour, IManager
    {
        private SpriteRenderer fadeRenderer;
        private Coroutine fade;
        private bool transitioning;
        private Scene enteredScene;
        public GameObject CurrentStage { get; set; }

        public void ManagerInitialize()
        {
            var camera = Game.Instance.transform.Find("2DCamera");
            if (camera != null)
            {
                var overlay = camera.Find("FadeOutObject");
                if (overlay != null) fadeRenderer = overlay.GetComponent<SpriteRenderer>();
            }
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void EnterCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (enteredScene == scene) return;
            enteredScene = scene;
            string name = scene.name;
            transitioning = false;
            var game = Game.Instance;
            if (name == "Loading") { game.uiManager.Push(UI_TYPE.LOADING); return; }
            FadeOut(0.5f);
            if (name == "Lobby")
            {
                var camera = game.transform.Find("2DCamera");
                if (camera != null) camera.position = new Vector3(0f, 1.865f, camera.position.z);
                game.uiManager.Push(UI_TYPE.LOBBY);
                game.uiManager.Push(UI_TYPE.GOODS);
                game.uiManager.Push(UI_TYPE.GAMEOPTION);
            }
            else if (name == "Game" || name == "Dungeon")
            {
                var prefab = name == "Game" ? game.dataManager.SelectStagePrefab : game.dataManager.selectDungeon;
                if (prefab == null)
                {
                    Debug.LogError("No stage selected for " + name + "; returning to the lobby.");
                    LoadScene("Lobby");
                    return;
                }
                game.dataManager.doNotSummonMode = name == "Dungeon" && prefab.name == "Dungeon04";
                CurrentStage = LunaEntities.Spawn(prefab, Vector3.zero, prefab.transform.rotation);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (transitioning) return;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
                throw new ArgumentException("Scene is missing from build settings: " + sceneName, nameof(sceneName));
            transitioning = true;
            Game.Instance.uiManager.ClearSceneForms();
            LunaEntities.ClearScene();
            Time.timeScale = 1f;
            StartCoroutine(Transition(sceneName));
        }

        private IEnumerator Transition(string name)
        {
            var operation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            // Battle -> battle is the same Procedure state, so explicitly enter the replacement scene.
            if (name == "Game" || name == "Dungeon")
            {
                var manager = GameFramework.GameFrameworkEntry.GetModule<GameFramework.Procedure.IProcedureManager>();
                if (manager.CurrentProcedure is LunaBattleProcedure) EnterCurrentScene();
            }
            transitioning = false;
        }

        public void FadeIn(float seconds, Action next = null) => StartFade(1f, seconds, next);
        public void FadeOut(float seconds, Action next = null) => StartFade(0f, seconds, next);
        private void StartFade(float target, float seconds, Action next)
        {
            if (fade != null) StopCoroutine(fade);
            if (fadeRenderer == null || seconds <= 0f)
            {
                if (fadeRenderer != null) { var color = fadeRenderer.color; color.a = target; fadeRenderer.color = color; }
                next?.Invoke(); return;
            }
            fade = StartCoroutine(Fade(target, seconds, next));
        }
        private IEnumerator Fade(float target, float seconds, Action next)
        {
            float start = fadeRenderer.color.a;
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            {
                var color = fadeRenderer.color;
                color.a = Mathf.Lerp(start, target, elapsed / seconds);
                fadeRenderer.color = color;
                yield return null;
            }
            var final = fadeRenderer.color; final.a = target; fadeRenderer.color = final;
            fade = null;
            next?.Invoke();
        }
        private void OnSceneUnloaded(Scene scene)
        {
            var game = Game.Instance;
            if (game == null) return;
            game.uiManager.ClearSceneForms();
            LunaEntities.ClearScene();
            if (scene.name == "Game" || scene.name == "Dungeon")
            {
                PoolManager.Instance.ResetPoolManager();
                CurrentStage = null;
            }
            game.dataManager.doNotSummonMode = false;
        }
        public bool IsScenes(params string[] names)
        {
            string current = SceneManager.GetActiveScene().name;
            foreach (string name in names) if (current == name) return true;
            return false;
        }
        public void ManagerClear() { SceneManager.sceneUnloaded -= OnSceneUnloaded; }
        public void ManagerUpdate() { }
        public void ManagerLoop() { }
    }
}
