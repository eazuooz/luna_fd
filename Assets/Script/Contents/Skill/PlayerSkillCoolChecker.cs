using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillCoolChecker : EverydayDevup.Framework.LunaEntityBehaviour
{
    public int skillCooltime { get; set; }
    private Text skillCooltimeText;

    protected override void OnEntityInitialize()
    {
        skillCooltimeText = transform.Find("SkillCool").GetComponent<Text>();
        skillCooltime = 0;
    }

    protected override void OnEntityUpdate(float elapsed, float realElapsed)
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
