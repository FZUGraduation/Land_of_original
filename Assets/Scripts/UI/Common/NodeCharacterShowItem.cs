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
        HeroConfigData heroConfigData = Data as HeroConfigData;
        if (heroConfigData != null)
        {
            // 设置单元格的显示内容
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
