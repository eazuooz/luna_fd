using System;
using Luna.Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class StageButton : MonoBehaviour
{
    public void Bind(LunaStageRow row, bool unlocked, bool selected, Action clicked)
    {
        transform.Find("CheckImage").gameObject.SetActive(selected);
        transform.Find("BossImage").gameObject.SetActive(row.IsBossStage);
        transform.Find("DungeonInfos")?.gameObject.SetActive(false);
        var label = transform.Find("Text").GetComponent<Text>();
        label.text = row.DisplayName + (unlocked ? string.Empty : "  잠김");
        label.resizeTextMaxSize = label.fontSize;
        label.resizeTextMinSize = 15;
        label.resizeTextForBestFit = true;
        GetComponent<Button>().onClick.AddListener(() => clicked());
    }
}
