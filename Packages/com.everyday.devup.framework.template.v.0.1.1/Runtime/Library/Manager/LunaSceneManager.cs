//#define LUNA_SCENE_MANAGER Game.Instance.lunaSceneManager;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



namespace EverydayDevup.Framework
{
    public class LunaSceneManager : MonoBehaviour, IManager
    {
        private GameObject fadeOutObject;
        private GameObject currentStage;

        public GameObject CurrentStage 
        {
            get { return currentStage; }
            set { currentStage = value; }
        }
        public void ManagerInitialize()
        {

        }
        public void ManagerClear()
        {
            //throw new System.NotImplementedException();
        }

        public void ManagerLoop()
        {
            //throw new System.NotImplementedException();
        }

        public void ManagerUpdate()
        {
            //throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {
            Transform cameraTr = transform.parent.parent.transform.Find("2DCamera");
            fadeOutObject = cameraTr.Find("FadeOutObject").gameObject;

            SceneManager.sceneUnloaded += OnSceneUnLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void FadeIn(float duringTime, System.Action nextEvent = null)
        {
            StartCoroutine(CoroutineFadeIn(duringTime, nextEvent));
        }


        public void FadeOut(float duringTime, System.Action nextEvent = null)
        {
            StartCoroutine(CoroutineFadeOut(duringTime, nextEvent));
        }


        IEnumerator CoroutineFadeIn(float duringTime, System.Action nextEvent = null)
        {
            SpriteRenderer sr = fadeOutObject.GetComponent<SpriteRenderer>();
            Color tempColor = sr.color;
            while (tempColor.a < 1f)
            {
                tempColor.a += Time.deltaTime / duringTime;
                sr.color = tempColor;

                if (tempColor.a >= 1f) tempColor.a = 1f;

                yield return null;
            }

            sr.color = tempColor;
            if (nextEvent != null) nextEvent();
        }


        private IEnumerator CoroutineFadeOut(float duringTime, System.Action nextEvent = null)
        {
            SpriteRenderer sr = fadeOutObject.GetComponent<SpriteRenderer>();
            Color tempColor = sr.color;
            while (tempColor.a > 0f)
            {
                tempColor.a -= Time.deltaTime / duringTime;
                sr.color = tempColor;

                if (tempColor.a <= 0f) tempColor.a = 0f;

                yield return null;
            }

            sr.color = tempColor;
            if (nextEvent != null) nextEvent();
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //ManagerClear();

            if (scene.name == "Loading")
            {
                
            }
            else if (scene.name == "Lobby")
            {
                Game.Instance.lunaSceneManager.FadeOut(1.0f, () => { });
                GameObject.Find("2DCamera").gameObject.transform.position
                           = new Vector3(0,1.865f,GameObject.Find("2DCamera").gameObject.transform.position.z);
                
                Game.Instance.uiManager.Push(UI_TYPE.LOBBY);
                Game.Instance.uiManager.Push(UI_TYPE.GOODS);
                Game.Instance.uiManager.Push(UI_TYPE.GAMEOPTION);
            }
            else if (scene.name == "Game")
            {
                Game.Instance.lunaSceneManager.FadeOut(1.0f, ()=> { });

                currentStage = GameObject.Instantiate(Game.Instance.dataManager.SelectStagePrefab,
                    new Vector3(0, 0, 0), Game.Instance.dataManager.SelectStagePrefab.transform.rotation);

            }
            else if (scene.name == "Dungeon")
            {
                Game.Instance.lunaSceneManager.FadeOut(1.0f, ()=> {});

                if (Game.Instance.dataManager.selectDungeon.name == "Dungeon04")
                {
                    Game.Instance.dataManager.doNotSummonMode = true;
                }
                else
                {
                    Game.Instance.dataManager.doNotSummonMode = false;
                }
                currentStage = GameObject.Instantiate(Game.Instance.dataManager.selectDungeon,
                    new Vector3(0, 0, 0), Game.Instance.dataManager.selectDungeon.transform.rotation);
            }
        }

        private void OnSceneUnLoaded(Scene scene)
        {
            if (scene.name == "Loading")
            {
                Game.Instance.uiManager.Pop(UI_TYPE.LOADING);
            }
            else if (scene.name == "Lobby")
            {
                if (Game.Instance.uiManager.CompareTop(UI_TYPE.SELECTSTAGE))
                {
                    //Game.Instance.uiManager.Pop(UI_TYPE.SELECTSTAGE);
                }
                Game.Instance.uiManager.Pop(UI_TYPE.GAMEOPTION);
                Game.Instance.uiManager.Pop(UI_TYPE.GOODS);
                Game.Instance.uiManager.Pop(UI_TYPE.LOBBY);
            }
            else if (scene.name == "Game")
            {
                PoolManager.Instance.ResetPoolManager();
                //Game.Instance.dataManager.InstHeroes.Clear();
            }
            else if (scene.name == "Dungeon")
            {
                PoolManager.Instance.ResetPoolManager();
                //Game.Instance.dataManager.InstHeroes.Clear();
            }

            Game.Instance.dataManager.doNotSummonMode = false;
            Resources.UnloadUnusedAssets();
        }

        public bool IsScenes(params string[] names)
        {
            bool temp = false;
            foreach (string name in names)
            {
                if (SceneManager.GetActiveScene().name == name)
                {
                    temp = true;
                }
            }

            return temp;
        }
    }
}