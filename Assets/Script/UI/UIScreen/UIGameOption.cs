using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.SceneManagement;

public class UIGameOption : UILuna
{
	public UIReference refData;
    public Sprite soundX;
    private bool soundOnOff { get; set; }

	public override void OnInit() 
    {
        soundOnOff = true;
    }

	public override void OnActive() { }

	public override void OnInActive() { }

	public override void OnUpdate() { }

	public override void OnLoop() { }

	public override void OnClear() { }

    public void OnClickGameOption()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Loading")
        {

        }
        else if (scene.name == "Lobby")
        {
            Game.Instance.uiManager.Push(UI_TYPE.QUITPOPUP);
        }
        else if (scene.name == "Game")
        {
            Game.Instance.uiManager.Push(UI_TYPE.SELECTPOPUP);

        }
        else if (scene.name == "Dungeon")
        {
            Game.Instance.uiManager.Push(UI_TYPE.SELECTPOPUP);
        }
       
    }

    public void OnClickSoundOnOff()
    {
        if(soundOnOff == true)
        {
            soundOnOff = false;

            foreach(var audio in Game.Instance.dataManager.StageSounds)
            {
                if(audio != null)
                    audio.volume = 0.0f;
            }

            foreach (var audio in Game.Instance.dataManager.ObjectSounds)
            {
                if (audio != null)
                    audio.volume = 0.0f;
            }
        }
        else
        {
            soundOnOff = true;

            foreach (var audio in Game.Instance.dataManager.StageSounds)
            {
                if (audio != null)
                    audio.volume = Defined.BackgroundVolumeSize;
            }

            foreach (var audio in Game.Instance.dataManager.ObjectSounds)
            {
                if (audio != null)
                    audio.volume = Defined.EffectVolumeSize;
            }
        }
    }
}
