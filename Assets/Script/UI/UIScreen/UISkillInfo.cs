
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EverydayDevup.Framework;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Spine.Unity;

public class UISkillInfo : UILuna
{
    public UIReference refData;

    private GameObject uiUpgrade;
    private SkillData skillData;
    private UILobby uiLobby;
    private string skillName;
    public override void OnInit()
    {
        base.OnInit();

        uiUpgrade = GameObject.FindWithTag("UIUpgrade");
        uiLobby = LunaUtility.FindWithTagGameObjectComponent<UILobby>("UILobby");

        skillName = Game.Instance.dataManager.OnClickSkillName;
        skillData = LunaDataTable.Instance.skiiTable[skillName];

        transform.Find("Object").GetComponent<Image>().sprite = skillData.skillImage;
        transform.Find("Name").GetComponent<Text>().text
            = "<" + skillData.skillName + "LV " + skillData.level.ToString() + ">";


        transform.Find("Discription").GetComponent<Text>().text
            = skillData.skillDiscription;

        transform.Find("LevelupStat").GetComponent<Text>().text
            = skillData.skillLevelDiscription;

        Game.Instance.dataManager.OnClickSkillName = "";
    }

    public override void OnActive()
    {

    }

    public override void OnInActive() { }

    public override void OnUpdate() { }

    public override void OnLoop() { }

    public override void OnClear() { }

    public void OnClickSkillLevelUp()
    {
        skillData.level += 1;
    }
    

    public void OnClickRelease()
    {
        Game.Instance.uiManager.Pop(type);
    }

    public void OnClickEquipButton()
    {
        uiUpgrade.GetComponent<UIUpgrade>().TurnOn_Off_SkillEffect(true);
        Game.Instance.uiManager.Pop(type);
        Game.Instance.dataManager.OnClickSkillName = skillName;

    }


}
