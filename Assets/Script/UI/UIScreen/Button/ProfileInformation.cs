using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileInformation : MonoBehaviour
{
    // Start is called before the first frame update
    //private GameObject button;
    //private GameObject text;

    public void DisableCountText()
    {
        GameObject text = gameObject.transform.Find("Text").gameObject;
        if (text)
        {
            text.SetActive(false);
        }
    }

    public void SetCountText(string str)
    {
        GameObject text = gameObject.transform.Find("Text").gameObject;
        text.GetComponent<Text>().text = str;
    }

    public void SetProfilePicture(Sprite sprite)
    {
        GameObject button = gameObject.transform.Find("Button").gameObject;
        if (button)
            button.GetComponent<Image>().sprite = sprite;
        else
            gameObject.SetActive(false);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
