using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillCoolChecker : MonoBehaviour
{
    public int skillCooltime { get; set; }
    private Text skillCooltimeText;

    void Start()
    {
        skillCooltimeText = transform.Find("SkillCool").GetComponent<Text>();
        skillCooltime = 0;
    }

    void Update()
    {
        if(skillCooltimeText != null)
        {
            if (skillCooltime == 0)
                skillCooltimeText.text = "";
            else
                skillCooltimeText.text = skillCooltime.ToString();
        }
    }
}
