using System;
using Spine.Unity;
using UnityEngine;

namespace Luna.Gameplay
{
    /// <summary>Only authored scenery references. Native backdrop logic owns all motion and timers.</summary>
    public sealed class LunaStageBackdropView : MonoBehaviour
    {
        public Transform Door;
        public AudioSource DoorSound;
        public Transform LobbySparkles;
        public Transform[] ScrollingPlates = Array.Empty<Transform>();
        public WanderingDemonBinding[] WanderingDemons = Array.Empty<WanderingDemonBinding>();
        public UnitView[] Emitters = Array.Empty<UnitView>();
    }
    [Serializable]
    public struct WanderingDemonBinding
    {
        public Transform Root;
        public SkeletonAnimation Animation;
    }
}
