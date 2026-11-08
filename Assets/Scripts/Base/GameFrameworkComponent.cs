//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public abstract class GameFrameworkComponent : MonoBehaviour
    {
        protected virtual void Awake()
        {
            GameEntry.RegisterComponent(this);
        }
    }
}
