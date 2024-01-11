using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EverydayDevup.Framework
{
    public class SoundManager : MonoBehaviour, IManager
    {
        private AudioClip startStageMusic01;

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

        void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            startStageMusic01 = Resources.Load<AudioClip>("Sounds/Escape from hell (extract)");

            Game.Instance.dataManager.StageSounds.Add(gameObject.GetComponent<AudioSource>());
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //ManagerClear();

            if (scene.name == "Loading")
            {
                //gameObject.GetComponent<AudioSource>().loop = true;
                //gameObject.GetComponent<AudioSource>().Stop();
            }
            else if (scene.name == "Lobby")
            {

            }
            else if (scene.name == "Game" || scene.name == "Dungeon")
            {
                if(gameObject.GetComponent<AudioSource>().clip == null || gameObject.GetComponent<AudioSource>().clip != startStageMusic01)
                {
                    gameObject.GetComponent<AudioSource>().clip = startStageMusic01;
                    gameObject.GetComponent<AudioSource>().loop = true;
                    gameObject.GetComponent<AudioSource>().Play();
                }
            }
        }

        void OnSceneUnLoaded(Scene scene)
        {
            if (scene.name == "Loading")
            {

            }
            else if (scene.name == "Lobby")
            {
                gameObject.GetComponent<AudioSource>().loop = true;
                gameObject.GetComponent<AudioSource>().Stop();
            }
            else if (scene.name == "Game")
            {

            }
        }
    }

}