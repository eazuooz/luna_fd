using EverydayDevup.Framework;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameOption : UILuna
{
    public UIReference refData;
    public Sprite soundX;
    private bool soundOnOff;
    private Image soundButtonImage;
    private Sprite soundOnSprite;

    public override void OnInit()
    {
        if (soundButtonImage == null)
        {
            soundButtonImage = transform.Find("ButtonSound").GetComponent<Image>();
            soundOnSprite = soundButtonImage.sprite;
        }
        soundOnOff = GameFrameworkEntry.GetModule<ISettingManager>().GetBool("Luna.Sound.Enabled", true);
        ApplySoundState();
    }

    public override void OnActive() { }
    public override void OnInActive() { }
    public override void OnUpdate() { }
    public override void OnLoop() { }
    public override void OnClear() { }

    public void OnClickGameOption()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (scene == "Lobby") Game.Instance.uiManager.Push(UI_TYPE.QUITPOPUP);
        else if (scene == "Game" || scene == "Dungeon") Game.Instance.uiManager.Push(UI_TYPE.SELECTPOPUP);
    }

    public void OnClickSoundOnOff()
    {
        soundOnOff = !soundOnOff;
        ApplySoundState();
        GameFrameworkEntry.GetModule<ISettingManager>().SetBool("Luna.Sound.Enabled", soundOnOff);
        LunaRuntimeData.SaveNow();
    }

    private void ApplySoundState()
    {
        AudioListener.volume = soundOnOff ? 1f : 0f;
        soundButtonImage.sprite = soundOnOff ? soundOnSprite : soundX;
    }
}
