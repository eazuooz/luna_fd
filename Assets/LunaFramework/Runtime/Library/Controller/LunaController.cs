using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class LunaController : MonoBehaviour, IBase
    {
        public void ControllerInit()
        {
            OnInit();
        }

        public void ControllerUpdate()
        {
            OnUpdate();
        }

        public void ControllerClear()
        {
            OnClear();
        }

        public void ControllerLoop()
        {
            OnLoop();
        }

        public virtual void OnActive() { }

        public virtual void OnClear() { }

        public virtual void OnInActive() { }

        public virtual void OnInit() { }

        public virtual void OnLoop() { }

        public virtual void OnUpdate() { }
    }
}