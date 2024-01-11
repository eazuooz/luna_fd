using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    private GameObject thisSlider;
    public GameObject Chaser { get; set; }
    public float CurAlpha { get; set; }
    public float HitCount { get; set; }


    void Start()
    {
        thisSlider = transform.Find("Canvas").Find("Slider").gameObject;

        ColorAlphaSet(0.4f);
        HitCount = 0.0f;
    }

    void Update()
    {
        if( Chaser != null &&
            Chaser.GetComponent<Status>() != null &&
            Chaser.activeSelf == true )
        {
            Chase();
            ValueCheck();
            ColorCheck();
        }
        else
        {
            EndHealthBar();
        }
    }

    public void InitHealthBar(GameObject chaser)
    {
        Chaser = chaser;
    }

    public void Hit()
    {
        HitCount = 0.3f;
    }

    void ColorAlphaSet(float alpha)
    {
        if (alpha < 0 || alpha > 1.0f)
            return;

        if (alpha == CurAlpha)
            return;

        Color color;

        color = thisSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color;
        color.a = alpha;
        thisSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;

        color = thisSlider.transform.Find("Background").GetComponent<Image>().color;
        color.a = alpha;
        color = thisSlider.transform.Find("Background").GetComponent<Image>().color = color;

        CurAlpha = alpha;
    }

    void Chase()
    {
        transform.position = Chaser.transform.Find("Canvas").transform.position + new Vector3(0, Chaser.transform.Find("Canvas").GetComponent<RectTransform>().rect.height / 2, 0);
    }

    void ValueCheck()
    {
        if(thisSlider.GetComponent<Slider>() != null)
            thisSlider.GetComponent<Slider>().value = Chaser.GetComponent<Status>().stat.Hp / Chaser.GetComponent<Status>().HealthBarHpPoint;
    }

    void ColorCheck()
    {
        if (Chaser.GetComponent<Status>().unitState.IsLobbyMove == true)
        {
            ColorAlphaSet(0.0f);
        }
        else
        {
            if(HitCount > 0.0f)
            {
                HitCount -= Time.deltaTime * 1.0f;
                ColorAlphaSet(1.0f);
            }
            else
            {
                ColorAlphaSet(0.4f);
            }
        }
    }

    void EndHealthBar()
    {
        Destroy(gameObject);
    }
}
