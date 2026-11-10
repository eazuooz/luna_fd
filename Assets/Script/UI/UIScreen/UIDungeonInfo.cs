using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class UIDungeonInfo : UILuna
{
    private LunaStageRow row;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        var stage = LunaSession.Current.selectDungeon?.GetComponent<LunaStageDefinition>();
        if (stage == null) { LunaUI.Close(type); return; }
        row = stage.Row;
        var profile = LunaDataTable.Instance.playerData;
        bool unlocked = LunaStageTables.IsUnlocked(row, profile);
        transform.Find("BackGround/Title/Text").GetComponent<Text>().text = row.DisplayName;
        transform.Find("BackGround/SelectButton/LevelText").GetComponent<Text>().text =
            unlocked ? "권장 Lv. " + row.RecommendedLevel : row.UnlockAfter + "층 클리어 후";
        transform.Find("BackGround/StartButton").GetComponent<Button>().interactable = unlocked;
        LunaStagePreview.Fill(transform, stage);
        LunaStagePreview.AddBriefing(transform, row, 165f);
    }

    public void ReleaseThisUI() => LunaUI.Close(type);

    public void OnClickSelectDungeon(int index)
    {
        if (row == null || !LunaStageTables.IsUnlocked(row, LunaDataTable.Instance.playerData)) return;
        LunaUI.Close(type);
        LunaUI.Close(UI_TYPE.SELECTSTAGE);
        foreach (var unit in LunaSession.Current.lobbyUnits)
            if (unit != null) unit.GetComponent<UnitLogic>().unitState.IsLobbyStartMove = true;
        LunaScenes.FadeIn(1.5f, () => LunaScenes.Load("Dungeon"));
    }
}
