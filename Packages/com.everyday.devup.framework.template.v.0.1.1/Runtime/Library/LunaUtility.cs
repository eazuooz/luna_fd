using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using EverydayDevup.Framework;

namespace EverydayDevup.Framework
{
    public class LunaUtility
    {
        public static GameObject FindChildObject(GameObject parentObject, string childObjectName)
        {
            Transform[] allChildren = parentObject.transform.GetComponentsInChildren<Transform>();

            GameObject returnObject = null;

            #region Exception Check
            int count = 0;
            foreach (Transform tr in allChildren)
            {
                if (tr.gameObject.name == childObjectName)
                {
                    count++;
                }
            }

            if (count > 1)
            {
                Debug.Log("exist same Name");
                return returnObject;
            }
            #endregion

            foreach (Transform tr in allChildren)
            {
                if (tr.gameObject.name == childObjectName)
                {
                    returnObject = tr.gameObject;
                    return returnObject;
                }
            }

            return returnObject;
        }

        public static List<GameObject> FindChildObjects(GameObject parentObject, string childObjectName)
        {
            Transform[] allChildren = parentObject.transform.GetComponentsInChildren<Transform>();

            List<GameObject> returnList = new List<GameObject>();

            foreach (Transform tr in allChildren)
            {
                if (tr.gameObject.name == childObjectName)
                {
                    returnList.Add(tr.gameObject);
                }
            }

            return returnList;
        }

        public static T FindComponentByChildObject<T>(GameObject parentObject, string childObjectName)
        {
            Transform[] allChildren = parentObject.transform.GetComponentsInChildren<Transform>();
            #region Exception Check
            int count = 0;
            foreach (Transform tr in allChildren)
            {
                if (tr.gameObject.name == childObjectName)
                {
                    count++;
                }
            }

            if (count > 1)
            {
                Debug.Log("exist same Name");
                return default(T);
            }
            #endregion

            T returnObject;
            foreach (Transform tr in allChildren)
            {
                if (tr.gameObject.name == childObjectName)
                {
                    returnObject = tr.GetComponent<T>();
                    return returnObject;
                }
            }

            return default(T);
        }

        public static List<GameObject> FindChildObjects(UnityEngine.GameObject parentObject, params string[] names)
        {
            List<GameObject> returnObjs = new List<GameObject>();
            Transform[] temps = parentObject.transform.GetComponentsInChildren<Transform>();
            foreach (var item in temps)
            {
                if (CompareNameToFindObject(item.gameObject, names))
                {
                    returnObjs.Add(item.gameObject);
                }
            }

            return returnObjs;
        }

        public static GameObject FindGameObject(Transform parent, params string[] args)
        {
            GameObject returnObject = null;

            

            return returnObject;
        }

        private static bool CompareNameToFindObject(GameObject gameObj, string[] names)
        {
            foreach (string name in names)
            {
                if (gameObj.name == name)
                {
                    return true;
                }
            }
            
            return false;
        }

        public static GameObject FindWithTagGameObject(string name)
        {
            return GameObject.FindWithTag(name); //.GetComponent<UILobby>();
        }

        public static T FindWithTagGameObjectComponent<T>(string name)
        {
            return GameObject.FindWithTag(name).GetComponent<T>();
        }

        public static void SetText(GameObject textObject, string str)
        {
            if (textObject)
            {
                textObject.GetComponent<Text>().text = str;
            }
        }

        public static void AddText(GameObject textObject, string str)
        {
            if (textObject)
            {
                textObject.GetComponent<Text>().text += str;
            }
        }

        public static Sprite CreateSprite(Texture texture)
        {
            Sprite sprite = null;
            Rect rect = Rect.zero;
            if (texture != null)
            {
                rect = new Rect(0, 0, texture.width, texture.height);
                sprite = Sprite.Create(texture as Texture2D, rect, new Vector2(0.5f, 0.5f));
            }

            return sprite;
        }

        public static Sprite CreateSprite(Texture2D texture)
        {
            Sprite sprite = null;
            Rect rect = Rect.zero;
            if (texture != null)
            {
                rect = new Rect(0, 0, texture.width, texture.height);
                sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }

            return sprite;
        }
    }
}


