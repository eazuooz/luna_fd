using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the round's settled result; the round owns progress and rewards.</summary>
public class UIResult : UILuna
{
    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        var round = LunaScenes.CurrentGame;
        if (round == null) throw new System.InvalidOperationException("Result form requires its completed round.");
        bool won = round.isStageWin;
        var plate = transform.Find("ImagePlate");
        plate.Find("ImageTitle/ResultText").GetComponent<Text>().text = round is EndlessDungeon endless
            ? endless.dungeonLevel + "층 돌파" : won ? "승리!" : "패배!";
        var detail = transform.Find("ResultBoard/ResultText").GetComponent<Text>();
        detail.transform.parent.Find("MainText").gameObject.SetActive(false);
        detail.rectTransform.anchoredPosition = Vector2.zero;
        detail.rectTransform.sizeDelta = new Vector2(600f, 80f);
        detail.alignment = TextAnchor.MiddleCenter;
        detail.fontSize = 24;
        detail.lineSpacing = 1f;
        detail.resizeTextForBestFit = true;
        detail.resizeTextMinSize = 20;
        detail.resizeTextMaxSize = 24;
        string reason = round.ResultReason switch
        {
            LunaGame.EndReason.TimeExpired => "시간 초과",
            LunaGame.EndReason.TowerDestroyed => "아군 거점 파괴",
            LunaGame.EndReason.PlayerDefeated => "루나 쓰러짐",
            _ => "승리!"
        };
        detail.text = won ? reason : "<color=#FF8272>" + reason + "</color>";
        if (!won && (round is BasicDungeon || round is BossDungeon))
            detail.text += "\n적 " + (round is BossDungeon ? "보스" : "거점") + " 체력 " +
                Mathf.CeilToInt(round.RemainingObjectiveHealth * 100f) + "% 남음";
        bool campaign = round is BasicDungeon;
        bool nextAvailable = round is BasicDungeon basic && won && basic.HasNextStage;
        plate.Find("NextStage").gameObject.SetActive(nextAvailable);
        var retry = plate.Find("CurrentStage");
        if (!nextAvailable) retry.localPosition = new Vector2(0f, -225f);
        retry.Find("CurrentStage_Text").GetComponent<Text>().text = campaign ? LunaSession.Current.CurrentStageLevelString() : "다시 도전";
        plate.Find("NextStage/NextStage_Text").GetComponent<Text>().text = campaign ? LunaSession.Current.NextStageLevelString() : string.Empty;
        transform.Find("Coin/Text").GetComponent<Text>().text = "골드 +" + LunaDataTable.Instance.playerData.RewardGold +
            (round.RewardStones > 0 ? "  별석 +" + round.RewardStones : string.Empty);

        var contents = transform.Find("Item_ScrollView/Item_Scroll View_Viewport/Item_Content");
        var prefab = Resources.Load<GameObject>("Prefabs/09UI/ProfileInformation");
        int count = 0;
        foreach (var reward in LunaSession.Current.pickUpData)
        {
            if (!LunaDataTable.Instance.itemTable.TryGetValue(reward.Key, out var item) || item.IconImage == null) continue;
            var row = Instantiate(prefab, contents, false);
            var rect = row.GetComponent<RectTransform>();
            rect.localScale = Vector3.one; rect.localPosition = Vector3.zero; rect.localRotation = Quaternion.identity;
            rect.sizeDelta = new Vector2(100f, 100f);
            var profile = row.GetComponent<ProfileInformation>();
            profile.SetProfilePicture(LunaUIViews.CreateSprite(item.IconImage));
            profile.SetCountText("x" + reward.Value);
            count++;
        }
        contents.GetComponent<HorizontalLayoutGroup>().spacing = count == 2 ? -400 : count == 3 ? -300 : count == 4 ? -150 : 0;
    }

    public void ReStage()
    {
        string scene = LunaScenes.CurrentGame is BasicDungeon ? "Game" : "Dungeon";
        LunaUI.Close(UI_TYPE.RESULT);
        LunaScenes.FadeIn(1.5f, () => LunaScenes.Load(scene));
    }
    public void GoNextStage() => (LunaScenes.CurrentGame as BasicDungeon)?.ToNextStage();
    public void ReleaseBt()
    {
        LunaUI.Close(UI_TYPE.RESULT);
        IdentifyFunc.PickingUnits(unit => UnitLogic.From(unit).unitState.IsLobbyBackMove = true, IdentifyFunc.IsAlly, IdentifyFunc.IsNotTower);
        LunaScenes.FadeIn(1.5f, () => LunaScenes.Load("Lobby"));
    }
    public void PartyBt() => LunaUI.Open(UI_TYPE.CHARACTERLIST);
}
