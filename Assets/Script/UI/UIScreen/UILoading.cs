using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UILoading : UILuna
{
	public UIReference refData;

    private string sceneName = "Lobby";
    private float loadingTIme;

    private Image tabToStartImage;

	public override void OnInit()
	{
		base.OnInit();
        LunaDataTable.Instance.playerData
            = Resources.Load<PlayerData>("ScriptableObjects/PlayerData");


        //Scriptabl Load
        LunaDataTable.Instance.InitializeLunaDatas();
        LunaDataTable.Instance.InitializeUnitNames();
        LunaDataTable.Instance.InitializeObjectDatas();
        LunaDataTable.Instance.InitializeItemDatas();
        LunaDataTable.Instance.InitializeSKillDatas();
        LunaDataTable.Instance.InitializeStageDatas();

        Transform tabToStartTr = gameObject.transform.Find("TabToStart");
        tabToStartImage = tabToStartTr.GetComponent<Image>();

        StartCoroutine(LoadAsynSceneCoroutine());


        MyUnits.SetMyDeckUnitPrafab();
     


    }

    IEnumerator LoadAsynSceneCoroutine()
    {
        AsyncOperation operation 
            = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        tabToStartImage.color 
            = new Color
            (tabToStartImage.color.r
            ,tabToStartImage.color.g
            ,tabToStartImage.color.b
            ,0);

        while (!operation.isDone)
        {
            //InfiniteLoopDetector.Run();
            loadingTIme = +Time.time;
            
            tabToStartImage.color 
                = new Color
                    (tabToStartImage.color.r
                    ,tabToStartImage.color.g
                    ,tabToStartImage.color.b
                    ,Mathf.Abs(Mathf.Sin(loadingTIme) * 2.0f));

            if (Input.GetMouseButtonDown(0))
            {
                Game.Instance.lunaSceneManager.FadeIn(1.0f, () =>
                {
                    operation.allowSceneActivation = true;
                });
            }

            yield return null;
        }
    }
}

