using System.Collections.Generic;

public enum LevelBattleType
{
    Normal,
    Elite,
    Boss
}

public class BattleEnemyConfigData
{
    public EnemyConfigData enemyConfig;
    public int position;
}
[System.Serializable]
public class BattleLevelConfigData : ConfigData
{
    public List<BattleEnemyConfigData> enemyDatas = new();
    public LevelBattleType battleType;
}
