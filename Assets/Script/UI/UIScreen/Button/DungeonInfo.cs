using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
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

        LunaSession.Current.SelectDungeonString = dungeonString;

        string stageDir = "Prefabs/11Dungeons/"
                + LunaSession.Current.SelectDungeonString;

        LunaSession.Current.selectDungeon
            = Resources.Load<GameObject>(stageDir);

        LunaUI.Open(UI_TYPE.DUNGEONINFO);

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
