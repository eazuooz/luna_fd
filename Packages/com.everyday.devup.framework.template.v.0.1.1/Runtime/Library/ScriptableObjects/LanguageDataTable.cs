#define LANGUAGE_KOR 
//#define LANGUAGE_ENG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "LanguageTable", menuName = "Scriptable Object Asset/LanguageTable")]
public class LanguageDataTable : ScriptableObject
{
    public enum eLanguageKey
    {
        ITEM_LENE_SWORD,

    }

    public class LanguageValue
    {
        public string korean;
        public string english;
    }

    public SerializeDictionary<eLanguageKey, LanguageValue> languageDictionary
        = new SerializeDictionary<eLanguageKey, LanguageValue>();

    public LanguageDataTable()
    {
        if (languageDictionary.Count == 0)
        {
            
        }

        //LanguageValue temp = new LanguageValue();
        //temp.korean = "∑ª¥¿¿« ∞À";
        //temp.english = "Lene's Sword";

        //languageDictionary.Add(eLanguageKey.ITEM_LENE_SWORD, temp);
    }
}
