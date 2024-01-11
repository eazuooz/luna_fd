using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;


[CreateAssetMenu(fileName = "DungeonTable", menuName = "Scriptable Object Asset/DungeonTable")]
public class DungeonTable : ScriptableObject
{
    public List<GameObject> dungeonList
        = new List<GameObject>();

    public DungeonTable()
    {

    }
}
