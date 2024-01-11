using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        MATERIAL,
        RELICS,
        DESIGN,
        CHEST,
        KEY,
    }



    [Header("리소스")]
    //public GameObject resource;
    public GameObject cunnectPrefab;
    public Texture itemImage;
    public Texture IconImage;

    [Header("아이템 기본 사항")]
    public ItemType type;
    public bool isActive;
    public string itemName;
    [Multiline] public string itemDiscription;
    public int itemNumber;
    public int level;
    public int itemCount;
    public int price;
    public int dropStageLevel;









    //[Header("능력치")]


}
