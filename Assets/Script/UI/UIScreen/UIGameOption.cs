using Luna.Gameplay;
using GameFramework;
using GameFramework.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameOption : UILuna
{

    public Sprite soundX;
    private bool soundOnOff;
    private Image soundButtonImage;
    private Sprite soundOnSprite;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        if (soundButtonImage == null)
        {
            soundButtonImage = transform.Find("ButtonSound").GetComponent<Image>();
            soundOnSprite = soundButtonImage.sprite;
        }
        soundOnOff = GameFrameworkEntry.GetModule<ISettingManager>().GetBool("Luna.Sound.Enabled", true);
        ApplySoundState();
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

    public void OnClickGameOption()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (scene == "Lobby") LunaUI.Open(UI_TYPE.QUITPOPUP);
        else if (scene == "Game" || scene == "Dungeon") LunaUI.Open(UI_TYPE.SELECTPOPUP);
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
