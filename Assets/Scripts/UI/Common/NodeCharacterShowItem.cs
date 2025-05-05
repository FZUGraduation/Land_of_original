using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeCharacterShowItem : TableCell
{
    public Image icon;
    public Image bgImage;
    public override void OnInit()
    {
        var heroRuntimeData = Data as HeroRuntimeData;
        if (heroRuntimeData != null)
        {
            // 设置单元格的显示内容
            icon.sprite = heroRuntimeData.ConfigData.icon;
        }
    }
    public override void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            bgImage.color = Color.red;
        }
        else
        {
            bgImage.color = Color.white;
        }
    }
}
