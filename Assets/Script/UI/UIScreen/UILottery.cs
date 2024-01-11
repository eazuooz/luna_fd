using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using EverydayDevup.Framework;
public class UILottery : UILuna
{
    public UIReference refData;

    private GameObject lotteryCardPrefab;
    private bool isLotteryEnd = false;
    public override void OnInit()
    {
        base.OnInit();

        GameObject lotteryContent 
            = LunaUtility.FindChildObject(this.gameObject, "LotteryContent");

        GameObject lotteryScrollView
            = LunaUtility.FindChildObject(this.gameObject, "LotteryScroll View");

        if (LunaDataTable.Instance.LotteryResult.Count <= 5)
        {
            lotteryScrollView.transform.localPosition = new Vector3(0.0f, -188.0f, 0.0f);

        }

        foreach (string name in LunaDataTable.Instance.LotteryResult)
        {
            GameObject lotteryCardPrefab = Resources.Load<GameObject>("Prefabs/09UI/LotteryCard");
            GameObject card = Instantiate(lotteryCardPrefab);
            card.GetComponent<RectTransform>().SetParent(lotteryContent.transform);
            card.GetComponent<RectTransform>().localScale = lotteryCardPrefab.transform.localScale;
            card.GetComponent<RectTransform>().localPosition = lotteryCardPrefab.transform.localPosition;

            


            string dir = "Prefabs/09UI/UICharcter/" + name + "UI";

            GameObject chracterUIPrefab = Resources.Load<GameObject>(dir);
            //GameObject chracterSkeletonGraphic = Resources.Load<GameObject>(dir);
            GameObject instObject = Instantiate(chracterUIPrefab);

            instObject.GetComponent<RectTransform>().SetParent(card.transform);
            instObject.GetComponent<RectTransform>().localScale = chracterUIPrefab.transform.localScale;
            instObject.GetComponent<RectTransform>().localPosition = chracterUIPrefab.transform.localPosition;
            instObject.GetComponent<RectTransform>().localPosition -= new Vector3(0, 70, 0);


            Canvas canvas = instObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI_Top";

            if (LunaDataTable.Instance.UnitTable[name].UnitType == eUnitType.Hero)
            {
                dir = "UI/UIPlayer/CardPicture/Heroes/" + name + "Card";
            }
            else
            {
                dir = "UI/UIPlayer/CardPicture/Soldiers/" + name + "Card";
            }
            
            Texture2D texture = Resources.Load<Texture2D>(dir);
            if (texture == null)
            {
                continue;
            }

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            card.GetComponent<Image>().sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        StartCoroutine(EndLottery());
    }

    public override void OnActive() { }

    public override void OnInActive() { }

    public override void OnUpdate()
    {
        if (isLotteryEnd && Input.GetMouseButtonDown(0))
        {
            Game.Instance.uiManager.Pop(type);
            isLotteryEnd = false;
        }
    }

    public override void OnLoop() { }

    public override void OnClear() { }

    IEnumerator EndLottery()
    {
        yield return new WaitForSeconds(2.0f);

        isLotteryEnd = true;
    }
}
