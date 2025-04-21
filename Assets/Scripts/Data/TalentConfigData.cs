
using System.Collections.Generic;
using UnityEngine;

public class TalentConfigData : ConfigData
{
    [Tooltip("消耗点数")]
    public int needPoint;

    [Tooltip("前置天赋")]
    public List<TalentConfigData> preTalents = new();
}
