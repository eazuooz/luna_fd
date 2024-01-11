using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;
using UnityEngine.UI;
using System;

public class UIProgressBar : UILuna
{
	public UIReference refData;

	public class MinimapProfile
    {
		public GameObject MinimapObject;
		public GameObject GameObject;
    }

	private Image titleImage;
	private Text titleText;
	private float time;

	private GameObject player;
	private GameObject allyTower;
	private GameObject enemyTower;

	float minimapRectWidth;
	float betweenTowerDistacne;

	private RectTransform minimapTr;
	private GameObject minimapPlayer;
	private GameObject minimapAllyTower;
	private GameObject minimapEnemyTower;

	private List<GameObject> Boss = new List<GameObject>();
	private List<GameObject> minimapBoss = new List<GameObject>();

	public List<GameObject> heroesMinimap = new List<GameObject>();
	public List<GameObject> herose = new List<GameObject>();

	private GameObject uiPlayer;

    private LinkedList<MinimapProfile> heroes
        = new LinkedList<MinimapProfile>();

    //private Dictionary<GameObject, GameObject> heroes
    //	= new Dictionary<GameObject, GameObject>();

    public void PushProfile(GameObject instance)
    {
		MinimapProfile newProfile = new MinimapProfile();
		newProfile.GameObject = instance;
		newProfile.MinimapObject = InitMinimapProfile(instance, true);

		heroes.AddLast(newProfile);
	}

	public void ReleaseMinimapObject(GameObject minimapObject)
    {
		if (heroes.Count == 0)
			return;

		MinimapProfile releaseProfile = null;
		foreach (var item in heroes)
        {
			if(item.GameObject == minimapObject)
            {
				releaseProfile = item;
            }
        }

		releaseProfile.GameObject = null;
		Destroy(releaseProfile.MinimapObject);
		releaseProfile.MinimapObject = null;

		heroes.Remove(releaseProfile);
	}

	public override void OnInit() 
	{
		base.OnInit();

		titleImage = gameObject.transform.Find("TitleImage").gameObject.GetComponent<Image>();
		titleText = gameObject.transform.Find("TitleText").gameObject.GetComponent<Text>();

		minimapTr = transform.Find("MinimapImage") as RectTransform;
		minimapRectWidth = ( minimapTr.rect.width / 2.0f );
		betweenTowerDistacne = Mathf.Abs(Defined.EnemyTowerPosition.x - Defined.AllyTowerPosition.x);

		GameObject staegeObj = GameObject.FindGameObjectWithTag("Stage");
		uiPlayer = GameObject.FindGameObjectWithTag("UIPlayer");

		if (staegeObj.GetComponent<BasicDungeon>() != null)
			titleText.text = "¾Ç¸ùÀÇ ¼º\n" + "ÁöÇÏ " + Game.Instance.dataManager.CurrentStageLevelString();

		gameObject.transform.Find("MinimapText").gameObject.GetComponent<Text>().text
			= "¾Ç¸ùÀÇ ¼º " + "ÁöÇÏ " + Game.Instance.dataManager.CurrentStageLevelString();

		player = staegeObj.GetComponent<BasicDungeon>().gamePlayer;
		allyTower = staegeObj.GetComponent<BasicDungeon>().gameAllyTower;
		enemyTower = staegeObj.GetComponent<BasicDungeon>().gameEnemyTower;

		minimapPlayer = InitMinimapProfile(player, true);
		minimapAllyTower = InitMinimapProfile(allyTower);
		minimapEnemyTower = InitMinimapProfile(enemyTower);

        if (minimapPlayer != null)
            MoveMinimapProfile(minimapPlayer, player, true);

        if (minimapAllyTower != null)
            MoveMinimapProfile(minimapAllyTower, allyTower);

        if (minimapEnemyTower != null)
            MoveMinimapProfile(minimapEnemyTower, enemyTower);

        //for (int i = 0; i < Game.Instance.dataManager.InstHeroes.Count; i++)
        //{
        //	heroesMinimap.Add(InitMinimapProfile(Game.Instance.dataManager.InstHeroes[i]));
        //}
    }

	public override void OnActive() { }
	public override void OnInActive() { }
	public override void OnUpdate()
	{
		TitleImageProcess();

		if (minimapPlayer != null)
            MoveMinimapProfile(minimapPlayer, player, true);

		if (heroes.Count == 0)
			return;

        foreach (var item in heroes)
        {
			MoveMinimapProfile(item.MinimapObject, item.GameObject, true);
		}
    }

	public override void OnLoop() { }

	public override void OnClear() { }

	private void TitleImageProcess()
    {
		if (time > 1)
		{
			return;
		}

		time += Time.deltaTime * 0.01f;

		titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b,
			titleImage.color.a - time);

		titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b,
			titleText.color.a - time);
	}
	private GameObject InitMinimapProfile(GameObject gameObj, bool disText = false)
    {
		Sprite newMinimapSPrite = gameObj.GetComponent<Status>().objectData.minimapPicture;
		if (newMinimapSPrite == null)
			return null;

		string prefabDir = "Prefabs/09UI/minimapObject";

		GameObject minimapPrefab =
			Resources.Load(prefabDir) as GameObject;

		GameObject newMinimapObj = null;

		newMinimapObj = Instantiate(minimapPrefab);
		newMinimapObj.transform.SetParent(minimapTr);
		newMinimapObj.transform.localPosition = Vector3.zero;
		newMinimapObj.transform.localScale = Vector3.one;
		
		newMinimapObj.GetComponent<Image>().sprite = newMinimapSPrite;
		newMinimapObj.GetComponent<Image>().SetNativeSize();

        if (false == disText)
        {
			newMinimapObj.transform.Find("DistanceText").gameObject.SetActive(false);
		}

		return newMinimapObj;
	}

	private void MoveMinimapProfile(GameObject minimapObj, GameObject gameObj, bool disText = false)
	{
		if (gameObj == null)
		{
			Destroy(minimapObj);
			return;
		}

		if(minimapObj == null)
        {
			return;
        }

		if (gameObj.activeSelf == false)
        {
			minimapObj.transform.localPosition = new Vector3(-minimapRectWidth + 10.0f, 3.0f);
		}


		float rate = (gameObj.transform.position.x + 8.0f) / betweenTowerDistacne;
		Vector3 uiPos 
			= Vector3.Lerp(new Vector3(-minimapRectWidth + 10.0f, 3.0f, 0), new Vector3(minimapRectWidth -19.0f, 3.0f, 0), rate);
		minimapObj.transform.localPosition = uiPos;

        if (disText)
        {
			minimapObj.transform.Find("DistanceText").GetComponent<Text>().text
				= Convert.ToInt32(50.0f * rate).ToString() + "m"; ;
		}

	}
}
