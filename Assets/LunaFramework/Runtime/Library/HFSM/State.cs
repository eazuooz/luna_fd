using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public abstract class State
    {
        public List<State> mNext = new List<State>();
        public int mLevel = -1;

        public abstract void OnEnter(GameObject gameObject);
        public abstract void OnExcute(GameObject gameObject);
        public abstract void OnExit(GameObject gameObject);
    }
}


