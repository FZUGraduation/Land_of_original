
using System.Collections.Generic;

public class BattleHeroData : BattleCharacterData
{
    public HeroConfigData heroConfig;
    public HeroRuntimeData heroRuntime;

    public BattleHeroData(HeroRuntimeData heroData, int battleID) : base(battleID)
    {
        //将玩家局外数值赋值给玩家局内数值
        var hpValue = heroData.GetFinalValue(StatType.HP);
        hp.InitValue(hpValue, 0, hpValue);
        mp.InitValue(heroData.GetFinalValue(StatType.MP));
        atk.InitValue(heroData.GetFinalValue(StatType.ATK));
        def.InitValue(heroData.GetFinalValue(StatType.DEF), 0, 2000);
        speed.InitValue(heroData.GetFinalValue(StatType.SPEED));
        critRate.InitValue(heroData.GetFinalValue(StatType.CRITRATE));
        critDamage.InitValue(heroData.GetFinalValue(StatType.CRITDAMAGE));
        hitRate.InitValue(heroData.GetFinalValue(StatType.HITRATE));
        heroConfig = heroData.ConfigData;
        heroRuntime = heroData;
    }

    public override SkillConfigData GetSkillConfig(string skillName)
    {
        return heroConfig.skills.Find(s => s.key == skillName);
    }
}
