using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class InputManager : MonoBehaviour, IManager
    {
        public GameObject lunaMouse;
        private List<LunaController> lunaControllers;
            //= new List<LunaController>();
        public void ManagerInitialize()
        {
            lunaControllers = new List<LunaController>();
            
            GameObject go = Instantiate(lunaMouse);
            DontDestroyOnLoad(go);
            LunaController lunaController = go.GetComponent<LunaController>();

            lunaControllers.Add(lunaController);

            foreach (var item in lunaControllers)
            {
                item.ControllerInit();
            }
        }
        public void ManagerClear()
        {
            foreach (var item in lunaControllers)
            {
                item.ControllerClear();
                //GameObject.Destroy(item.gameObject);
            }
        }

        public void ManagerLoop()
        {
            foreach (var item in lunaControllers)
            {
                item.ControllerLoop();
            }
        }

        public void ManagerUpdate()
        {
            foreach (var item in lunaControllers)
            {
                item.ControllerUpdate();
            }
        }
    }
}
