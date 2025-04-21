using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeEnemyDetail : TableCell
{
    public Image icon;
    public TextMeshProUGUI nameText;
    // public TextMeshProUGUI descText;
    public Transform skillContent;
    public TableView skillTableView;

    public override void OnInit()
    {
        EnemyConfigData enemyConfigData = Data as EnemyConfigData;
        if (enemyConfigData != null)
        {
            // 设置单元格的显示内容
            nameText.text = enemyConfigData.key;
            // descText.text = enemyConfigData.desc;
            skillTableView.Init(enemyConfigData.skills, null);
        }
    }
}
