using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;

public class UIGoods : UILuna
{
	public UIReference refData;

	private Text goldText;
	private Text starStoneText;


	public override void OnInit() 
	{
		base.OnInit();

		goldText = LunaUtility.FindComponentByChildObject<Text>(gameObject, "GoldText");
		starStoneText = LunaUtility.FindComponentByChildObject<Text>(gameObject, "StarStoneText");

		goldText.text = LunaDataTable.Instance.playerData.Gold.ToString();
		starStoneText.text = LunaDataTable.Instance.playerData.StarStone.ToString();

	}

	public override void OnActive() { }
	public override void OnInActive() { }
	public override void OnUpdate()
	{
		goldText.text = LunaDataTable.Instance.playerData.Gold.ToString();
		starStoneText.text = LunaDataTable.Instance.playerData.StarStone.ToString();
	}

	public override void OnLoop() { }

	public override void OnClear() { }

	public void OnClickAddButton()
    {
		Game.Instance.uiManager.Push(UI_TYPE.GAMESHOP);
    }
}
