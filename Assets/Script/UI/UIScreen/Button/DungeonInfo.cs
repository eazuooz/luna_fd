using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DungeonInfo : MonoBehaviour
{

    public int DungeonNumber { get; set; }
    public void OnClickSelectDungeon(int idx)
    {
        string dungeonString = "Dungeon0";
        dungeonString += idx.ToString();

        Game.Instance.dataManager.SelectDungeonString = dungeonString;

        string stageDir = "Prefabs/11Dungeons/"
                + Game.Instance.dataManager.SelectDungeonString;

        Game.Instance.dataManager.selectDungeon
            = Resources.Load<GameObject>(stageDir);

        Game.Instance.uiManager.Push(UI_TYPE.DUNGEONINFO);

    }
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Find("Button").
            GetComponent<Button>().onClick.AddListener(() => OnClickSelectDungeon(DungeonNumber));

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
