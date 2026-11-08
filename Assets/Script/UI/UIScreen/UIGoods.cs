using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using UnityEngine.UI;

public class UIGoods : UILuna
{

	private Text goldText;
	private Text starStoneText;


	protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
		

		goldText = LunaUIViews.FindComponentByChildObject<Text>(gameObject, "GoldText");
		starStoneText = LunaUIViews.FindComponentByChildObject<Text>(gameObject, "StarStoneText");

		goldText.text = LunaDataTable.Instance.playerData.Gold.ToString();
		starStoneText.text = LunaDataTable.Instance.playerData.StarStone.ToString();

	}

	
	
	protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
		goldText.text = LunaDataTable.Instance.playerData.Gold.ToString();
		starStoneText.text = LunaDataTable.Instance.playerData.StarStone.ToString();
	}

	

	protected override void OnClose(bool isShutdown, object userData)
    {
        try
        { 
        }
        finally { base.OnClose(isShutdown, userData); }
    }

	public void OnClickAddButton()
    {
		LunaUI.Open(UI_TYPE.GAMESHOP);
    }
}
