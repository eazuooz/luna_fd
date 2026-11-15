
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
        RefreshSkill();


        transform.Find("Discription").GetComponent<Text>().text
            = skillData.skillDiscription;

        transform.Find("LevelupStat").GetComponent<Text>().text
            = skillData.skillLevelDiscription;

        LunaSession.Current.OnClickSkillName = "";
        foreach (string path in new[] { "LevelUpButton", "Keystone", "Melisa" }) transform.Find(path).gameObject.SetActive(false);
        var button = LunaUIPresentation.Button(transform, "UpgradeSkill", "", new Vector2(210f, -255f), new Vector2(320f, 88f), OnClickSkillLevelUp, true);
        LunaUIPresentation.Label(transform, "SkillResourceHint", "스킬 강화석은 스테이지 보상으로 획득합니다.\nLV 5 · 10 · 15 · 20에서 추가 효과가 적용됩니다.", new Vector2(210f, 110f), new Vector2(640f, 44f), 17, TextAnchor.MiddleCenter);
        ((RectTransform)transform.Find("Object")).sizeDelta = new Vector2(110f, 110f);
        var description = (RectTransform)transform.Find("Discription"); description.anchoredPosition = new Vector2(215f, 35f); description.sizeDelta = new Vector2(640f, 80f);
        var milestones = (RectTransform)transform.Find("LevelupStat"); milestones.anchoredPosition = new Vector2(215f, -110f); milestones.sizeDelta = new Vector2(640f, 165f);
        RefreshSkill();
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
        int beforeLevel = skillData.level;
        int beforeStones = LunaDataTable.Instance.itemTable[1001].itemCount;
        if (!LunaEconomy.TryLevelUpSkill(skillData)) return;
        if (!LunaRuntimeData.SaveNow())
        { skillData.level = beforeLevel; LunaDataTable.Instance.itemTable[1001].itemCount = beforeStones; }
        RefreshSkill();
    }
    private void RefreshSkill()
    {
        transform.Find("Name").GetComponent<Text>().text = skillData.skillName + " · LV " + skillData.level;
        var upgrade = transform.Find("UpgradeSkill");
        if (upgrade == null) return;
        int have = LunaDataTable.Instance.itemTable.TryGetValue(1001, out var stones) ? stones.itemCount : 0;
        int cost = LunaEconomy.SkillStoneCost(skillData.level);
        upgrade.Find("Caption").GetComponent<Text>().text = skillData.level >= LunaEconomy.SkillMaxLevel ? "최대 레벨" : "LV " + skillData.level + " → " + (skillData.level + 1) + " 강화\n스킬 강화석 " + have + " / " + cost;
        upgrade.GetComponent<Button>().interactable = skillData.level < LunaEconomy.SkillMaxLevel && have >= cost;
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
