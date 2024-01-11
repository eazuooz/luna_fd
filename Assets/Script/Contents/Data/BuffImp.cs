using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using EverydayDevup.Framework;

public class BuffImp //: MonoBehaviour
{
    private System.Action onEventBuffIcons;
    private GameObject unitGameObject;
    private List<Buff> buffs;
    private float iconHeight;

    public void AddBuff(eBuffType info, float during)
    {
        foreach (Buff buff in buffs)
        {
            if (buff.BuffType == info)
            {
                buff.BuffLeftTime += during;
                return;
            }
        }

        buffs.Add(new Buff(info, during));

        if (onEventBuffIcons != null)
            onEventBuffIcons();
    }

    public void AllBuffSpendTime(List<Buff> buffs)
    {
        foreach (Buff buff in buffs)
        {
            SpendTime(buff);
        }
    }
    public void SpendTime(Buff buff)
    {
        if (buff.BuffLeftTime >= 0)
        {
            buff.BuffLeftTime -= (Time.deltaTime * 0.69f);
        }
    }
    public IEnumerator UpdateBuffs()
    {
        while (true)
        {
            if (buffs != null
                && buffs.Count > 0)
            {

                AllBuffSpendTime(buffs);

                foreach (Buff buff in buffs)
                {
                    if (buff.IsBlinking == false && buff.BuffLeftTime <= 5.0f)
                    {
                        if (onEventBuffIcons != null)
                            onEventBuffIcons();
                    }
                    else if (buff.IsBlinking == true && buff.BuffLeftTime > 5.0f)
                    {
                        if (onEventBuffIcons != null)
                            onEventBuffIcons();
                    }
                }

                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    if (buffs[i].BuffLeftTime <= 0)
                    {
                        if (buffs[i].IconObject != null)
                            Game.Destroy(buffs[i].IconObject);
                        //Destroy(buffs[i].IconObject);

                        buffs.RemoveAt(i);

                        Debug.Log("버프삭제");

                        if (onEventBuffIcons != null)
                            onEventBuffIcons();
                    }
                }
            }
            yield return null;
        }
    }
    public void BuffIconOff()
    {
        if (buffs != null)
        {
            foreach (var buffData in buffs)
            {
                if (buffData.IconObject == null)
                {
                    continue;
                }

                buffData.IconObject.SetActive(false);
                Game.Destroy(buffData.IconObject);
                buffData.IconObject = null;
            }

            buffs.Clear();
            buffs = null;
        }
    }

    public void BuffIconInit()
    {
        buffs = new List<Buff>();
        Transform buffCanvas = unitGameObject.transform.Find("Canvas");

        iconHeight = buffCanvas.transform.position.y
            + buffCanvas.GetComponent<RectTransform>().rect.height * 0.5f;

        //Game.StartCoroutine(BuffDataUpdate());
    }

    public GameObject UnitGameObject
    {
        get { return unitGameObject; }
        set { unitGameObject = value; }
    }

    public System.Action OnEventBuffIcons
    {
        get { return onEventBuffIcons; }
        set { onEventBuffIcons = value; }
    }

    public List<Buff> Buffs
    {
        get { return buffs; }
        set { buffs = value; }
    }

    public float IconHeight
    {
        get { return iconHeight; }
        set { iconHeight = value; }
    }
}
