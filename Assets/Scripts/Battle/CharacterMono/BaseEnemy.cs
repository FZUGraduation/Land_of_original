

public class BaseEnemy : BaseCharacter
{

    // protected override void OnActionStart(object[] args)
    // {
    //     base.OnActionStart(args);
    //     if (IsDeath() || !blackboard.boolDir["InAction"]) return;


    // }

    public override string GetCurrSkillName()
    {
        var enemyData = GetBattleData() as BattleEnemyData;
        return enemyData.currSkillName;
    }
}

