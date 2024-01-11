using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;

public class UIStageText : UILuna
{
	public UIReference refData;

	private Image titleImage;
	private Text titleText;
	private float time;

	private GameObject player;
	


	public override void OnInit()
	{
		base.OnInit();

		titleImage = gameObject.transform.Find("TitleImage").gameObject.GetComponent<Image>();
		titleText = gameObject.transform.Find("TitleText").gameObject.GetComponent<Text>();


		GameObject staegeObj = GameObject.FindGameObjectWithTag("Stage");

		titleText.text 
			+= "¾Ç¸ñÀÇ ¼º " + Game.Instance.dataManager.CurrentStageLevelString();
	}

	public override void OnActive() { }
	public override void OnInActive() { }
	public override void OnUpdate()
	{
		TitleImageProcess();


	}

	public override void OnLoop() { }

	public override void OnClear() { }

	private void TitleImageProcess()
	{
		if (time > 1)
		{
			return;
		}

		time += Time.deltaTime * 0.002f;

		titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b,
			titleImage.color.a - time);

		titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b,
			titleText.color.a - time);
	}




}
