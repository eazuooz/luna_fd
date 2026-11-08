
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luna.Gameplay;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Spine.Unity;

public class UISkillInfo : UILuna
{

    private GameObject uiUpgrade;
    private SkillData skillData;
    private UILobby uiLobby;
    private string skillName;
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        

        uiUpgrade = GameObject.FindWithTag("UIUpgrade");
        uiLobby = LunaUIViews.FindWithTagGameObjectComponent<UILobby>("UILobby");

        skillName = LunaSession.Current.OnClickSkillName;
        skillData = LunaDataTable.Instance.skiiTable[skillName];

        transform.Find("Object").GetComponent<Image>().sprite = skillData.skillImage;
        transform.Find("Name").GetComponent<Text>().text
            = "<" + skillData.skillName + "LV " + skillData.level.ToString() + ">";


        transform.Find("Discription").GetComponent<Text>().text
            = skillData.skillDiscription;

        transform.Find("LevelupStat").GetComponent<Text>().text
            = skillData.skillLevelDiscription;

        LunaSession.Current.OnClickSkillName = "";
    }

    

    

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed); }

    

    protected override void OnClose(bool isShutdown, object userData)
    {
        try
        { 
        }
        finally { base.OnClose(isShutdown, userData); }
    }

    public void OnClickSkillLevelUp()
    {
        skillData.level += 1;
    }
    

    public void OnClickRelease()
    {
        LunaUI.Close(type);
    }

    public void OnClickEquipButton()
    {
        uiUpgrade.GetComponent<UIUpgrade>().TurnOn_Off_SkillEffect(true);
        LunaUI.Close(type);
        LunaSession.Current.OnClickSkillName = skillName;

    }


}
