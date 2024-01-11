using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    [CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Object Asset/TutorialData")]
    public class TutorialData : ScriptableObject
    {
        public List<Vector3> tutorialList = new List<Vector3>();
        public int TutorialIndex = 0;



    }

}
