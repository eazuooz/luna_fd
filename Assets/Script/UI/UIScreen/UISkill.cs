using System.Collections.Generic;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Native HUD presentation. PlayerLogic owns skill rules, cooldowns, summons and equipment effects.</summary>
public class UISkill : UILuna
{
    public sealed class SkillInfo
    {
        public PlayerSkillSnapshot skillData;
        public GameObject skillButton;
        public GameObject slotEffect;
        public bool isActive;
        public float currentCoolTime;
        internal UnityAction Click;
    }
    private readonly List<SkillInfo> lunaSkillDatas = new List<SkillInfo>();
    private readonly List<SkillInfo> equipDesignSkillDatas = new List<SkillInfo>();
    private GameObject maskObject;
    private SkillInfo pendingThunder;
    private int targetingPlayerId;
    private float previousTimeScale;
    private LunaGame round;
    public int BoundPlayerId { get; private set; }
    public bool Auto { get; set; }
    private PlayerLogic Player
    {
        get
        {
            if (round == null || !round.IsActive || !ReferenceEquals(round, LunaScenes.CurrentGame)) return null;
            var player = round.Player as PlayerLogic;
            return player != null && player.Data != null && player.stat.Hp > 0f && !player.unitState.IsDeath ? player : null;
        }
    }
    private UIPlayer Hud => Object.FindAnyObjectByType<UIPlayer>();

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        ClearButtons();
        maskObject = transform.Find("mask").gameObject;
        maskObject.SetActive(false);
        Auto = false;
        BoundPlayerId = 0;
        round = LunaScenes.CurrentGame;
        BindPlayer();
    }

    private void BindPlayer()
    {
        var player = Player;
        // Native UI loading can finish after the player has died. Keep the form
        // inert until this round owns a live player, and rebind on resurrection.
        if (player == null || player.Abilities == null || player.ID == BoundPlayerId) return;
        FinishThunderTargeting();
        ClearButtons();
        BoundPlayerId = player.ID;
        var equipped = player.Abilities.Loadout.Skills;
        for (int i = 0; i < equipped.Count && i < 3; i++)
            CreateButton(equipped[i], "Skill_Button_" + i, false);
        var equipment = player.Abilities.Loadout.Equipment;
        for (int i = 0; i < equipment.Count && i < 3; i++)
            CreateButton(equipment[i], "Equip_" + i, true);
    }

    private void CreateButton(PlayerSkillSnapshot data, string path, bool equipment)
    {
        var info = new SkillInfo { skillData = data, skillButton = transform.Find(path).gameObject };
        info.skillButton.SetActive(true);
        Transform image = info.skillButton.transform.Find(equipment ? "Equip_Image" : "Skill_Button_Image");
        image.GetComponent<Image>().sprite = data.skillImage;
        Transform slot = info.skillButton.transform.Find("SKillSlotEffect");
        if (slot != null) { info.slotEffect = slot.gameObject; info.slotEffect.SetActive(false); }
        Transform mana = info.skillButton.transform.Find("Mana/SkillPoint");
        if (mana != null) mana.GetComponent<Text>().text = data.mana.ToString();
        if (!equipment)
        {
            info.Click = () => RequestSkill(info);
            info.skillButton.GetComponent<Button>().onClick.AddListener(info.Click);
        }
        (equipment ? equipDesignSkillDatas : lunaSkillDatas).Add(info);
    }

    protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
        BindPlayer();
        PlayerLogic player = Player;
        UIPlayer hud = Hud;
        foreach (SkillInfo info in lunaSkillDatas)
        {
            info.currentCoolTime = player != null && player.Abilities != null ? player.Abilities.Remaining(info.skillData) : info.skillData.coolTime;
            info.isActive = player != null && player.Abilities != null && player.Abilities.Ready(info.skillData);
            bool affordable = info.isActive && hud != null && hud.Mana >= info.skillData.mana;
            info.skillButton.transform.Find("Skill_Button_Image").GetComponent<Image>().color = affordable ? Color.white : Color.grey;
            if (info.slotEffect != null) info.slotEffect.SetActive(affordable);
            var checker = info.skillButton.GetComponent<PlayerSkillCoolChecker>();
            if (checker != null) checker.skillCooltime = Mathf.CeilToInt(info.currentCoolTime);
            if (Auto && affordable && pendingThunder == null) RequestSkill(info);
        }
        if (pendingThunder == null) return;
        if (player == null || player.ID != targetingPlayerId || player.IsHiding || player.stat.Hp <= 0f) { FinishThunderTargeting(); return; }
        if (Auto)
        {
            foreach (UnitLogic target in UnitLogic.ActiveUnits)
                if (target.Data != null && target.Data.RoundId == player.Data.RoundId && target.UnitSide != player.UnitSide && LunaCombat.IsInCombat(target))
                { TrySubmitThunderTarget(target.transform.position); break; }
        }
        else if (Input.GetMouseButtonDown(0) && LunaBootstrap.GameplayCamera != null)
            TrySubmitThunderTarget(LunaBootstrap.GameplayCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    private bool RequestSkill(SkillInfo info)
    {
        PlayerLogic player = Player;
        UIPlayer hud = Hud;
        if (player == null || player.ID != BoundPlayerId || player.Abilities == null || hud == null || !player.Abilities.Ready(info.skillData) || hud.Mana < info.skillData.mana) return false;
        if (info.skillData.name == "Thunder")
        {
            if (pendingThunder != null) return false;
            pendingThunder = info; targetingPlayerId = player.ID;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0.5f;
            maskObject.SetActive(true);
            return true;
        }
        if (!player.Abilities.TryUse(info.skillData)) return false;
        return true;
    }

    public bool TrySubmitThunderTarget(Vector3 position)
    {
        if (pendingThunder == null || !float.IsFinite(position.x)) return false;
        SkillInfo info = pendingThunder;
        PlayerLogic player = Player;
        UIPlayer hud = Hud;
        position.y = position.z = 0f;
        bool accepted = player != null && player.ID == targetingPlayerId && player.Abilities != null && hud != null && hud.Mana >= info.skillData.mana && player.Abilities.TryUse(info.skillData, position);
        FinishThunderTargeting();
        return accepted;
    }

    private void FinishThunderTargeting()
    {
        if (pendingThunder == null) return;
        pendingThunder = null; targetingPlayerId = 0;
        Time.timeScale = previousTimeScale;
        if (maskObject != null) maskObject.SetActive(false);
    }

    public void OnClickSkillAuto() => Auto = !Auto;
    public bool FireBall(SkillData data) => Player != null && Player.Abilities != null && Player.Abilities.TryUse(data);

    private void ClearButtons()
    {
        foreach (SkillInfo info in lunaSkillDatas)
            if (info.skillButton != null && info.Click != null) info.skillButton.GetComponent<Button>().onClick.RemoveListener(info.Click);
        lunaSkillDatas.Clear(); equipDesignSkillDatas.Clear();
        for (int i = 0; i < 3; i++)
        {
            transform.Find("Skill_Button_" + i)?.gameObject.SetActive(false);
            transform.Find("Equip_" + i)?.gameObject.SetActive(false);
        }
    }
    protected override void OnClose(bool isShutdown, object userData)
    {
        FinishThunderTargeting(); ClearButtons();
        BoundPlayerId = 0; round = null;
        base.OnClose(isShutdown, userData);
    }
}
