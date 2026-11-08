using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;


[CreateAssetMenu(fileName = "DungeonTable", menuName = "Scriptable Object Asset/DungeonTable")]
public class DungeonTable : ScriptableObject
{
    public List<GameObject> dungeonList
        = new List<GameObject>();

    public DungeonTable()
    {

    }
}
