using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeEnemySkillDetail : TableCell
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public override void OnInit()
    {
        SkillConfigData skillConfigData = Data as SkillConfigData;
        if (skillConfigData != null)
        {
            // 设置单元格的显示内容
            icon.sprite = skillConfigData.icon;
            nameText.text = skillConfigData.key;
            descText.text = skillConfigData.desc;
        }
    }

}
