using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luna.Gameplay;
using UnityEngine.UI;

public class UIStageText : UILuna
{

	private Image titleImage;
	private Text titleText;
	private float time;

	private GameObject player;
	


	protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
		

		titleImage = gameObject.transform.Find("TitleImage").gameObject.GetComponent<Image>();
		titleText = gameObject.transform.Find("TitleText").gameObject.GetComponent<Text>();


		GameObject staegeObj = GameObject.FindGameObjectWithTag("Stage");

		titleText.text 
			+= "악목의 성 " + LunaSession.Current.CurrentStageLevelString();
	}

	
	
	protected override void OnUpdate(float elapsed, float realElapsed)
    {
        base.OnUpdate(elapsed, realElapsed);
		TitleImageProcess();


	}

	

	protected override void OnClose(bool isShutdown, object userData)
    {
        try
        { 
        }
        finally { base.OnClose(isShutdown, userData); }
    }

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
