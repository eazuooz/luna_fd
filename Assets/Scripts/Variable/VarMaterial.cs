//------------------------------------------------------------
// Game Framework - MIT License
// Copyright © 2013–2021 Jiang Yin (EllanJiang)
// Modified © 2025 얌얌코딩
// Homepage: https://www.yamyamcoding.com/
// Feedback: mailto:eazuooz@gmail.com
//------------------------------------------------------------


using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarMaterial : Variable<Material>
    {
        public VarMaterial()
        {
        }

        public static implicit operator VarMaterial(Material value)
        {
            VarMaterial varValue = ReferencePool.Acquire<VarMaterial>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Material(VarMaterial value)
        {
            return value.Value;
        }
    }
}
