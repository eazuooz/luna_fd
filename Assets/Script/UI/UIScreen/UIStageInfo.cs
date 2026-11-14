using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIStageInfo : UILuna
{
    private LunaStageRow row;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        var stage = LunaSession.Current.SelectStagePrefab.GetComponent<LunaStageDefinition>();
        row = stage.Row;
        bool unlocked = LunaStageTables.IsUnlocked(row, LunaDataTable.Instance.playerData);
        transform.Find("BackGround/Title/Text").GetComponent<Text>().text = row.DisplayName;
        transform.Find("BackGround/SelectButton/LevelText").GetComponent<Text>().text =
            unlocked ? "권장 Lv. " + row.RecommendedLevel : (row.Id - 1) + "층 클리어 후";
        transform.Find("BackGround/StartButton").GetComponent<Button>().interactable = unlocked;
        transform.Find("BackGround/Title/Skull").gameObject.SetActive(row.IsBossStage);
        LunaStagePreview.Layout(transform, true);
        LunaStagePreview.Fill(transform, stage);
        LunaStagePreview.AddBriefing(transform, row, 128f);
    }

    public void OnClickStartGame()
    {
        var profile = LunaDataTable.Instance.playerData;
        if (row == null || !LunaStageTables.IsUnlocked(row, profile)) return;
        profile.SaveStageName = row.Key;
        LunaSession.Current.CurrentStageName = row.Key;
        LunaSession.Current.SetStagePrefab(row.Key);
        LunaUI.Close(type);
        LunaUI.Close(UI_TYPE.SELECTSTAGE);
        LunaScenes.CurrentLobbyGame?.OpenDoor();
        foreach (var unit in LunaSession.Current.lobbyUnits)
            if (unit != null) unit.GetComponent<UnitLogic>().unitState.IsLobbyStartMove = true;
        LunaScenes.FadeIn(1.5f, () => LunaScenes.Load("Game"));
    }

    public void PushPopupUI() => LunaUI.Open(type);
    public void ReleaseThisUI() => LunaUI.Close(type);
}
