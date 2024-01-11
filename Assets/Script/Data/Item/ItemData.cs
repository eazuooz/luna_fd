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



    [Header("���ҽ�")]
    //public GameObject resource;
    public GameObject cunnectPrefab;
    public Texture itemImage;
    public Texture IconImage;

    [Header("������ �⺻ ����")]
    public ItemType type;
    public bool isActive;
    public string itemName;
    [Multiline] public string itemDiscription;
    public int itemNumber;
    public int level;
    public int itemCount;
    public int price;
    public int dropStageLevel;









    //[Header("�ɷ�ġ")]


}
