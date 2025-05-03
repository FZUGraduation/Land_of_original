
public class BaseHero : BaseCharacter
{
    private string currSkillName = "";

    private int targetBattleId = -1;
    public int SkillTargetBattleId => targetBattleId;

    protected override void Awake()
    {
        base.Awake();
        GetComponentInChildren<CharacterModelController>().SetWeapon(SaveSlotData.Instance.heroDatas[0].equipmentData[EquipmentType.Weapon]);
    }

    /// <summary> 获取当前回合使用的技能名称 </summary>
    public override string GetCurrSkillName()
    {
        return currSkillName;
    }

    protected override void OnSkillTargetSelect(object[] args)
    {
        base.OnSkillTargetSelect(args);
        if (!IsInAction()) return;
        targetBattleId = (int)args[0];
        currSkillName = BattleData.Instance.CurrSkillConfig.key;
        blackboard.boolDir["inSkill"] = true;
    }
}