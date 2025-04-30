using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattleData : BaseEventCenter
{
    public static BattleData Instance { get; private set; }

    private List<BattleHeroData> heros = new();
    private List<BattleEnemyData> enemys = new();
    private List<BattleCharacterData> allCharacters = new();
    private Dictionary<int, BattleCharacterData> characterDict = new();
    private StatValueRuntimeData mp;//蓝量全队共享
    private int actionBattleId = -1;
    private int round = 0;
    private bool isBattleFinish = false;
    private SkillConfigData currSkillConfig;
    public readonly string battleLevel;

    public int Round { get => round; private set => round = value; }
    public int MP { get => (int)mp.BaseValue; }
    public List<BattleHeroData> Heros { get => heros; }
    public List<BattleEnemyData> Enemys { get => enemys; }
    public int ActionCharacter { get => actionBattleId; private set => actionBattleId = value; }
    public SkillConfigData CurrSkillConfig { get => currSkillConfig; private set => currSkillConfig = value; }
    public BattleCharacterData GetCharacterData(int battleID) => characterDict[battleID];

    /// <summary> 初始化战斗关卡 </summary>
    private BattleData(string battleLevel)
    {
        this.battleLevel = battleLevel;
        var battleLevelData = Datalib.Instance.GetData<BattleLevelConfigData>(battleLevel);
        int battleID = 1000;
        foreach (var enemy in battleLevelData.enemyDatas)
        {
            var battleEnemy = new BattleEnemyData(enemy.enemyConfig, enemy.position, ++battleID);
            enemys.Add(battleEnemy);
            allCharacters.Add(battleEnemy);
            characterDict.Add(battleEnemy.battleID, battleEnemy);
        }
        battleID = 2000;
        foreach (var heroData in SaveSlotData.Instance.heroDatas)
        {
            var battleHero = new BattleHeroData(heroData, ++battleID);
            heros.Add(battleHero);
            allCharacters.Add(battleHero);
            characterDict.Add(battleHero.battleID, battleHero);
        }
        RegisterEvent();
        mp = new StatValueRuntimeData(3, 0, 7);
        mp.OnValueModified += (value) => Emit(MPChange, value);//蓝量变化事件`
    }
    /// <summary> 初始化战斗数据 </summary>
    public static void Init(string battleLevel)
    {
        if (Instance != null)
        {
            return;//说明已经在战斗了
        }
        Instance = new BattleData(battleLevel);
    }

    public static void Clear()
    {
        if (Instance == null) return;
        Instance.ClearEvent();
        Instance = null;
    }

    /// <summary> 按速度降序排序 </summary>
    private void SortCharacters()
    {
        allCharacters.Sort((a, b) =>
        {
            if (a.ActionType == BattleActionType.Dead && b.ActionType != BattleActionType.Dead)
                return 1;//1表示a在b后面
            else if (a.ActionType != BattleActionType.Dead && b.ActionType == BattleActionType.Dead)
                return -1;//-1表示a在b前面
            else if (a.GetStat(StatType.SPEED).ModifiedValue > b.GetStat(StatType.SPEED).ModifiedValue)
                return -1;
            else if (a.GetStat(StatType.SPEED).ModifiedValue < b.GetStat(StatType.SPEED).ModifiedValue)
                return 1;
            else
                return 0;
        });
    }

    /// <summary> 获取当前行动顺序 </summary>
    public List<BattleCharacterData> GetCurrActionTurn()
    {
        List<BattleCharacterData> datas = new();
        foreach (var character in allCharacters)
        {
            if (character.ActionType != BattleActionType.Dead)
                datas.Add(character);
        }
        return datas;
    }


    /// <summary> 新回合开始 </summary>
    private void StartRound()
    {
        if (isBattleFinish) return;
        ActionCharacter = -1;
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].ActionType != BattleActionType.Dead)
                allCharacters[i].ActionType = BattleActionType.WaitAction;
        }
        Round++;
        SortCharacters();
        Debug.Log($"第{Round}回合开始");
        Emit(RoundStart, Round);
    }

    private async UniTask EndRound()
    {
        Debug.Log($"第{Round}回合结束");
        Emit(RoundEnd, Round);
        // await UniTask.Yield();
        await UniTask.Delay(500);
        StartRound();
    }

    /// <summary> 下一个行动 </summary>
    private void OnNextAction()
    {
        if (isBattleFinish) return;
        _ = OnNextActionAsync();
    }

    private async UniTask OnNextActionAsync()
    {
        bool findNext = await UpdateActionCharacter();

        if (Round == 0)
        {
            StartRound();
        }
        else if (!findNext)
        {
            _ = EndRound();
        }
        else
        {
            _ = StartAction();
        }
    }

    /// <summary> 更新行动角色 </summary>
    private async UniTask<bool> UpdateActionCharacter()
    {
        int currActionIndex = -1;
        for (int i = 0; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].battleID == ActionCharacter)
            {
                currActionIndex = i;
                break;
            }
        }
        if (currActionIndex == -1)//如果没有找到当前行动角色，说明是第一次行动
        {
            ActionCharacter = allCharacters[0].battleID;
            allCharacters[0].ActionType = BattleActionType.InAction;
            return true;
        }

        await EndAction();


        Debug.Log($"{ActionCharacter}行动结束");

        if (currActionIndex == allCharacters.Count - 1) return false;//返回false说明一个回合结束

        allCharacters[currActionIndex].ActionType = BattleActionType.ActionEnd;//当前行动角色行动结束
        for (int i = currActionIndex + 1; i < allCharacters.Count; i++)
        {
            if (allCharacters[i].ActionType != BattleActionType.Dead)
            {
                ActionCharacter = allCharacters[i].battleID;
                allCharacters[i].ActionType = BattleActionType.InAction;
                return true;
            }
        }
        return false;//返回false说明一个回合结束
    }

    /// <summary> 行动结束,计算mod </summary>
    private async UniTask EndAction()
    {
        if (characterDict[ActionCharacter].IsHero)
        {
            mp.BaseValue += 1;//英雄回合结束蓝量+1
        }
        var characterData = GetCharacterData(ActionCharacter);
        var hpStat = characterData.GetStat(StatType.HP);
        var hpMods = hpStat.GetAllModifiers();
        bool haveMod = false;
        foreach (var mod in hpMods)
        {
            if (mod.executeTime == ModifierApplication.ActionEnd)
            {
                CaculateHPChange(mod, characterData, characterData);
                haveMod = true;
            }
        }
        characterData.OnActionEnd();
        if (haveMod)
        {
            await UniTask.Delay(500);
        }
        CheckCharacterDeath();
        Emit(ActionEnd, ActionCharacter);
    }

    /// <summary> 行动开始,计算mod </summary>
    private async UniTask StartAction()
    {
        var characterData = GetCharacterData(ActionCharacter);
        var hpStat = characterData.GetStat(StatType.HP);
        var mods = hpStat.GetAllModifiers();
        bool haveMod = false;
        foreach (var mod in mods)
        {
            if (mod.executeTime == ModifierApplication.ActionStart)
            {
                CaculateHPChange(mod, characterData, characterData);
                haveMod = true;
            }
        }
        if (haveMod)
        {
            await UniTask.Delay(500);
        }
        CheckCharacterDeath();
        characterData.BeforeAction();
        if (!isBattleFinish)
        {
            Emit(ActionStart, ActionCharacter, characterData.IsHero);
        }
    }

    private void OnSkillSelect(object[] objects)
    {
        CurrSkillConfig = (SkillConfigData)objects[0];
    }

    private void OnSkillFocusCharacter(object[] args)
    {
        bool isFocus = (bool)args[0];
        if (isFocus && CurrSkillConfig.target == SkillTarget.AllHero)
        {
            foreach (var hero in Heros)
            {
                hero.characterMono.OnCharacterSelect(1);
            }
        }
        else if (isFocus && CurrSkillConfig.target == SkillTarget.AllEnemy)
        {
            foreach (var enemy in Enemys)
            {
                enemy.characterMono.OnCharacterSelect(1);
            }
        }
        else
        {
            foreach (var hero in Heros)
            {
                hero.characterMono.OnCharacterSelect(-1);
            }
            foreach (var enemy in Enemys)
            {
                enemy.characterMono.OnCharacterSelect(-1);
            }
        }
    }

    private void OnSkillExcute(object[] args)
    {
        int battleID = (int)args[0];
        string skillName = (string)args[1];
        int targetID = (int)args[2];
        var characterData = GetCharacterData(battleID);
        SkillConfigData skillconfig = characterData.GetSkillConfig(skillName);
        if (skillconfig == null)
        {
            Debug.LogError($"技能{skillName}不存在");
            return;
        }
        List<int> targets;
        var attackerData = GetCharacterData(battleID);
        attackerData.OnUseSkill(skillName);
        if (attackerData.IsHero)
        {
            mp.BaseValue -= skillconfig.mpCost;
            targets = GetHeroSkillTargets(skillconfig, targetID);
        }
        else
        {
            targets = (characterData as BattleEnemyData).currTargets;//GetEnemySkillTargets(skillconfig, battleID);
        }

        Debug.Log($"{battleID}释放技能{skillName}，目标：{string.Join(",", targets)}");

        foreach (var target in targets)
        {
            var targetData = GetCharacterData(target);
            ApplySkillEffects(attackerData, targetData, skillconfig);
        }
        CurrSkillConfig = null;
        CheckCharacterDeath();
    }

    public List<int> GetEnemySkillTargets(SkillConfigData skillConfig, int battleID)
    {
        List<int> targets = new();
        if (skillConfig.target == SkillTarget.Self)
        {
            targets.Add(battleID);
        }
        else if (skillConfig.target == SkillTarget.SingleHero)
        {
            if (skillConfig.autoTargetSelectType == SkillAutoSelectType.Random || skillConfig.autoTargetSelectType == SkillAutoSelectType.None)
            {
                targets.Add(GetRandomCharacter(true));
            }
            else
            {
                int id = GetMinOrMaxStatusBattleID(StatType.HP, skillConfig.autoTargetSelectType == SkillAutoSelectType.MinHp, true);
                targets.Add(id);
            }
        }
        else if (skillConfig.target == SkillTarget.SingleEnemy)
        {
            if (skillConfig.autoTargetSelectType == SkillAutoSelectType.Random || skillConfig.autoTargetSelectType == SkillAutoSelectType.None)
            {
                targets.Add(GetRandomCharacter(false));
            }
            else
            {
                int id = GetMinOrMaxStatusBattleID(StatType.HP, skillConfig.autoTargetSelectType == SkillAutoSelectType.MinHp, false);
                targets.Add(id);
            }
        }
        else if (skillConfig.target == SkillTarget.AllHero)
        {
            foreach (var hero in Heros)
            {
                if (hero.ActionType != BattleActionType.Dead)
                    targets.Add(hero.battleID);
            }
        }
        else if (skillConfig.target == SkillTarget.AllEnemy)
        {
            foreach (var enemy in Enemys)
            {
                if (enemy.ActionType != BattleActionType.Dead)
                    targets.Add(enemy.battleID);
            }
        }
        return targets;
    }

    private List<int> GetHeroSkillTargets(SkillConfigData skillConfig, int targetBattleID)
    {
        List<int> targets = new();
        if (skillConfig.target == SkillTarget.Self || skillConfig.target == SkillTarget.SingleHero || skillConfig.target == SkillTarget.SingleEnemy)
        {
            targets.Add(targetBattleID);
        }
        else if (skillConfig.target == SkillTarget.AllHero)
        {
            foreach (var hero in Heros)
            {
                if (hero.ActionType != BattleActionType.Dead)
                    targets.Add(hero.battleID);
            }
        }
        else if (skillConfig.target == SkillTarget.AllEnemy)
        {
            foreach (var enemy in Enemys)
            {
                if (enemy.ActionType != BattleActionType.Dead)
                    targets.Add(enemy.battleID);
            }
        }
        return targets;
    }

    private void ApplySkillEffects(BattleCharacterData attacker, BattleCharacterData target, SkillConfigData skillConfig)
    {
        List<EffectConfigData> effects = skillConfig.effects;
        if (skillConfig.effectView != null && skillConfig.effectView.effectViewPrefab)
        {
            //加载特效
            string path = "Prefabs/Battle/EffectView";
            ResourceManager.Instance.LoadAsync<GameObject>(path, (obj) =>
            {
                var effectView = GameObject.Instantiate(obj, target.characterMono.transform);
                effectView.transform.localPosition = Vector3.zero;
                effectView.transform.localScale = Vector3.one;
                effectView.transform.localRotation = Quaternion.identity;
                var skillEffectView = effectView.GetComponent<EffectView>();
                skillEffectView.SetInfo(skillConfig.effectView, target.characterMono.transform.position);
            });
        }
        foreach (var effect in effects)
        {
            if (effect.statModifier != null)//处理数值效果
            {
                if (CheckHit(effect.statModifier, attacker) == false) continue;

                var mod = new StatModifierEffect(effect.statModifier, attacker);

                if (mod.executeTime == ModifierApplication.InstantAttackOrHeal)
                {
                    CaculateHPChange(mod, attacker, target);
                }
                else
                {
                    target.GetStat(mod.excuteStat).AddModifier(mod);
                    Debug.Log($"{target.battleID}受到{attacker.battleID}的{effect.name}效果");
                    Emit(ShowFlyText, target.characterMono.gameObject, FlyTextType.Buff, mod.effectName, false);
                }
            }
            if (effect.specialEffect != null && effect.specialEffect.type != SpecialEffectType.None)//处理特殊效果
            {
                if (CheckHit(effect.specialEffect, attacker) == false) continue;

                var specialEffect = new SpecialEffect(effect.specialEffect, attacker);

                if (specialEffect.executeTime == SpecialEffectTriggerType.InstantTrigger)
                {
                    //具体特殊效果具体做
                }
                else
                {
                    target.AddSpecialEffect(specialEffect);
                    Emit(ShowFlyText, target.characterMono.gameObject, FlyTextType.Buff, specialEffect.effectName, false);
                }

            }
        }
    }

    /// <summary> 计算HPChange </summary>
    public void CaculateHPChange(StatModifierEffect mod, BattleCharacterData attacker, BattleCharacterData target)
    {
        bool isCritical = false;
        float number = mod.value;
        if (mod.statCalculatetype == BasicModifierType.Percent)
        {
            var sourceObject = mod.sourceObject == SouceStatType.Self ? attacker : target;
            number = sourceObject.GetStat(mod.baseStat).ModifiedValue * mod.value;
        }
        if (mod.canCrit && UnityEngine.Random.value < attacker.GetStat(StatType.CRITRATE).ModifiedValue)
        {
            number *= attacker.GetStat(StatType.CRITDAMAGE).ModifiedValue;//乘以爆伤
            isCritical = true;
        }
        //如果需要计算防御，且是对HP生效，且是负数，说明是伤害，需要计算防御
        if (mod.canDef && mod.excuteStat == StatType.HP && number < 0)
        {
            float def = target.GetStat(StatType.DEF).ModifiedValue;
            number /= 1 + def * 0.01f;
            //最终伤害 = 攻击力 / (1 + 防御力 * k)  其中k为平衡系数，用于调整防御收益曲线。
        }
        number *= UnityEngine.Random.Range(0.98f, 1.02f);//数值随机浮动
        target.GetStat(mod.excuteStat).BaseValue += number;
        number = (int)number;
        if (number != 0)
            Emit(ShowFlyText, target.characterMono.gameObject, FlyTextType.HPChange, number.ToString(), isCritical);
    }

    /// <summary> 检查是否命中 </summary>
    public bool CheckHit(BaseEffect effect, BattleCharacterData attacker)
    {
        if (effect.needCaculateHitrate && UnityEngine.Random.value > (attacker.GetStat(StatType.HITRATE).ModifiedValue + 1) * effect.baseHitRate)
        {
            // Emit(ShowFlyText, target.characterMono.gameObject, FlyTextType.Buff, "未命中", false);
            Debug.Log(effect.effectName + "未命中");
            return false;
        }
        return true;
    }

    public void CheckCharacterDeath()
    {
        foreach (var character in allCharacters)
        {
            if (character.ActionType == BattleActionType.Dead) continue;
            if (character.GetStat(StatType.HP).BaseValue <= 0.001)
            {
                character.ActionType = BattleActionType.Dead;
                Debug.Log($"{character.battleID}死亡");
                Emit(CharacterDeath, character.battleID);
            }
        }
        CheckWinOrLose();
    }

    public void CheckWinOrLose()
    {
        bool allDead = true;
        foreach (var enemy in Enemys)
        {
            if (enemy.ActionType != BattleActionType.Dead)
            {
                allDead = false;
                break;
            }
        }
        if (allDead)
        {
            Debug.Log("胜利");
            isBattleFinish = true;
            Emit(BattleEnd, true);
        }
        allDead = true;
        foreach (var hero in Heros)
        {
            if (hero.ActionType != BattleActionType.Dead)
            {
                allDead = false;
                break;
            }
        }
        if (allDead)
        {
            Debug.Log("失败");
            isBattleFinish = true;
            Emit(BattleEnd, false);
        }
    }

    public int GetMinOrMaxStatusBattleID(StatType statType, bool isMin, bool isHero)
    {
        int battleID = -1;
        float value = isMin ? 9999999 : -1;
        foreach (var character in allCharacters)
        {
            if (isHero && !character.IsHero) continue;
            if (character.ActionType != BattleActionType.Dead)
            {
                var stat = character.GetStat(statType);
                if (isMin && stat.BaseValue < value)
                {
                    value = stat.BaseValue;
                    battleID = character.battleID;
                }
                else if (!isMin && stat.BaseValue > value)
                {
                    value = stat.BaseValue;
                    battleID = character.battleID;
                }
            }
        }
        return battleID;
    }

    public int GetRandomCharacter(bool isHero)
    {
        List<int> battleIDs = new();
        foreach (var character in allCharacters)
        {
            if (isHero && !character.IsHero) continue;
            if (character.ActionType != BattleActionType.Dead)
            {
                battleIDs.Add(character.battleID);
            }
        }
        return battleIDs[UnityEngine.Random.Range(0, battleIDs.Count)];
    }

    private void RegisterEvent()
    {
        On(ActionNext, OnNextAction, this);
        On(SkillSelect, OnSkillSelect, this);
        On(SkillExcute, OnSkillExcute, this);
        On(SkillFocusCharacter, OnSkillFocusCharacter, this);
    }

    #region 事件define
    public readonly static string GenerateCharacter = GetEventName("GenCharacter");
    public readonly static string BattleStart = GetEventName("BattleStart");
    public readonly static string RoundStart = GetEventName("RoundStart");//回合开始
    public readonly static string RoundEnd = GetEventName("RoundEnd");//回合结束
    public readonly static string ActionStart = GetEventName("ActionStart");//行动开始,参数1：行动角色ID; 参数2：是否是英雄
    public readonly static string ActionEnd = GetEventName("ActionEnd");//行动结束,参数1：行动角色ID
    public readonly static string ActionNext = GetEventName("ActionNext");//下一个行动
    public readonly static string ChooseSkill = GetEventName("ChooseSkill");//选择技能
    public readonly static string ChooseTarget = GetEventName("ChooseSkill");//选择技能使用对象
    public readonly static string SkillSelect = GetEventName("SkillSelect");//技能开始释放,skillconfig
    public readonly static string SkillFocusCharacter = GetEventName("SkillFocusCharacter");//技能聚焦,SkillTarget=>allhero或allenemy
    public readonly static string SkillTargetSelect = GetEventName("SkillTargetSelect");//技能目标选择,battleid(技能选择的对象),skillname(技能名字)
    public readonly static string SkillExcute = GetEventName("SkillExcute");//技能执行,battleid(技能释放者),skillname(技能名字),targets(技能目标)

    public readonly static string AddEffect = GetEventName("AddEffect");//添加buff,参数1：battleID,参数2：mod
    public readonly static string RemoveEffect = GetEventName("RemoveEffect");//移除buff,参数1：battleID,参数2：mod
    public readonly static string CharacterDeath = GetEventName("CharacterDeath");//角色死亡,参数1：battleID
    public readonly static string BattleEnd = GetEventName("BattleEnd");//战斗结束,参数1：是否胜利
    public readonly static string ExitBattle = GetEventName("ExitBattle");//退出战斗
    //battleUI事件
    public readonly static string ShowDetail = GetEventName("ShowSkillDetail");//显示技能面板
    public readonly static string HideDetail = GetEventName("HideSkillDetail");//隐藏技能面板
    public readonly static string MPChange = GetEventName("MPChange");//显示技能目标选择
    public readonly static string ShowFlyText = GetEventName("ShowFlyText");//显示飘字,参数1：飘字对象,参数2：飘字number,参数3：是否暴击
    #endregion
}
