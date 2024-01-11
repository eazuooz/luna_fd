using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EverydayDevup.Framework
{
    public class AssetBundleRef : MonoBehaviour
    {
        private GameObject go;
        private AssetBundle assetBundle;

        public AssetBundle AssetBundle
        {
            get { return assetBundle; }
            set { assetBundle = value; }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

    }
}