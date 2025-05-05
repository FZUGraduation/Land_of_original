using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TalkPeopleConfigData : ConfigData
{
    public Sprite headIcon;
    public bool isHero = false;

    public string GetTalkName()
    {
        if (isHero)
        {
            return SaveSlotData.Instance.heroDatas[0].ConfigData.key;
        }
        else
        {
            return key;
        }
    }

    public Sprite GetTalkIcon()
    {
        if (isHero)
        {
            return SaveSlotData.Instance.heroDatas[0].ConfigData.icon;
        }
        else
        {
            return icon;
        }
    }
}
