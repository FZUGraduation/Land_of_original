using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIEnemyDetail : BaseDialog
{
    public TableView tableView;
    public override void Init(params object[] data)
    {
        string level = (string)data[0];
        var battleLevelData = Datalib.Instance.GetData<BattleLevelConfigData>(level);
        HashSet<EnemyConfigData> enemyDataSet = new();
        foreach (var enemy in battleLevelData.enemyDatas)
        {
            enemyDataSet.Add(enemy.enemyConfig);
        }
        List<EnemyConfigData> enemyData = enemyDataSet.ToList();
        tableView.Init(enemyData, null);
    }
}
