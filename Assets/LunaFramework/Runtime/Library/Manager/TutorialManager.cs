using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EverydayDevup.Framework
{
    public class TutorialManager : MonoBehaviour, IManager
    {
        public TutorialData tutorialData;
        public GameObject tutorialPrefab;
        private GameObject tutorialObject;

        public GameObject TutorialObject
        {
            get { return tutorialObject; }
            private set { tutorialObject = value; }
        }
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

        public void EndTutorialObject()
        {
            if (TutorialObject != null)
            {
                Destroy(TutorialObject);
                TutorialObject = null;
            }
        }
        public void SetTutorial(GameObject parent, string childObjName, int order)
        {
            if (Game.Instance.tutorialManager.tutorialData.TutorialIndex != order)
                return;

            EndTutorialObject();

            GameObject tutorialObject 
                = LunaUtility.FindChildObject(parent, childObjName);

            if (Game.Instance.tutorialManager.tutorialData.TutorialIndex == order)
            {
                CreateTouchGuide(tutorialObject);
                tutorialData.TutorialIndex++;
            }
        }
        public void CreateTouchGuide(GameObject tuorialObject)
        {
            tutorialObject = Instantiate(tutorialPrefab);
            tutorialObject.GetComponent<RectTransform>().parent 
                = tuorialObject.GetComponent<RectTransform>().transform;

            tutorialObject.GetComponent<RectTransform>().transform.localPosition = Vector3.zero;
            tutorialObject.GetComponent<RectTransform>().transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
        }

    }
}