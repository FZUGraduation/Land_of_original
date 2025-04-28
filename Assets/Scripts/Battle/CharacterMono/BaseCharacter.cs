using System;
using Core.EasyInteractive;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class BaseCharacter : SerializedMonoBehaviour, ISelectable, IFocusable
{
    [OdinSerialize, HideReferenceObjectPicker, OpenView]
    public BehaviourTreeData TreeData;
    [Button("把当前物体设为脏数据")]
    public void SetDirty() => EditorUtility.SetDirty(this);
    public BehaviourTreeData GetBtData() => TreeData;
    public Transform statusBarTransform;
    protected BlackBoard blackboard = null;
    protected BattleCharacterData characterData = null;
    private Renderer selectRenderder = null;
    public int BattleID
    {
        get => characterData.battleID;
    }
    // 交互相关
    private bool _enableSelect = true;
    private bool _enableFocus = true;
    public bool enableSelect => _enableSelect;
    public bool enableFocus => _enableFocus;
    public Type interactTag => GetType();
    public bool IsInAction() => characterData.ActionType == BattleActionType.InAction && blackboard.boolDir["inAction"];
    public bool IsDeath() => characterData.ActionType == BattleActionType.Dead;
    public bool IsInSkill() => blackboard.boolDir["inSkill"];
    public BattleCharacterData GetBattleData() => characterData;
    public StatValueRuntimeData GetStat(StatType statType) => characterData.GetStat(statType);
    protected virtual void OnEnable()
    {
        BattleData.Instance.On(BattleData.BattleStart, OnStartBattle, this);
        BattleData.Instance.On(BattleData.ActionStart, OnActionStart, this);
        BattleData.Instance.On(BattleData.SkillSelect, OnSkillSelect, this);
        BattleData.Instance.On(BattleData.SkillTargetSelect, OnSkillTargetSelect, this);
        BattleData.Instance.On(BattleData.CharacterDeath, OnCharacterDeath, this);
        BattleData.Instance.On(BattleData.ActionEnd, OnActionEnd, this);
    }
    protected virtual void OnDisable()
    {
        BattleData.Instance?.OffAll(this);
    }

    public void Init(BattleCharacterData characterData)
    {
        this.characterData = characterData;
        this.characterData.GetStat(StatType.HP).OnBaseValueChanged += OnHpChange;
        blackboard = new BlackBoard();
        blackboard.objectDir["owner"] = this;
        blackboard.objectDir["animator"] = GetComponentInChildren<Animator>();
        blackboard.boolDir["inSkill"] = false;
        blackboard.boolDir["inHero"] = characterData.IsHero;
        BattleData.Instance.Emit(BattleData.GenerateCharacter, characterData, statusBarTransform);
    }

    protected void OnStartBattle()
    {
        TreeData.InitBlackboard(blackboard);
        TreeData?.OnStart();
    }

    protected virtual void OnActionStart(object[] args)
    {
        // blackboard.boolDir["inAction"] = true;
        int battleId = (int)args[0];
        if (battleId != characterData.battleID || IsDeath()) return;
        blackboard.boolDir["inAction"] = true;
        Debug.Log((int)args[0] + "行动开始");
        // 行动时的外发光之类的效果，或者脚底下的光环之类的
        transform.localScale = Vector3.one * 1.5f;
    }

    private void OnActionEnd(object[] obj)
    {
        int battleID = (int)obj[0];
        if (battleID != characterData.battleID) return;
        transform.localScale = Vector3.one;
    }

    /// <summary>-1：取消选择，1：选择</summary>
    public void OnCharacterSelect(int type)
    {
        if (selectRenderder == null)
        {
            selectRenderder = GetComponentInChildren<Renderer>();
        }
        if (selectRenderder == null) return;
        if (type < 0)
        {
            selectRenderder.material.color = Color.gray;
        }
        else if (type > 0)
        {
            selectRenderder.material.color = Color.yellow;
        }
        // else if (type == 1)
        // {
        //     selectRenderder.material.color = Color.red;
        // }
    }

    private void OnSkillSelect(object[] objects)
    {
        var skillConfig = (SkillConfigData)objects[0];
        SkillTarget target = skillConfig.target;
        _enableFocus = false;
        _enableSelect = false;
        if (characterData.IsHero)
        {
            if (target == SkillTarget.SingleHero || target == SkillTarget.AllHero)
            {
                _enableFocus = true;
                _enableSelect = true;
            }
            else if (target == SkillTarget.Self)
            {
                if (characterData.battleID == BattleData.Instance.ActionCharacter)
                {
                    _enableFocus = true;
                    _enableSelect = true;
                }
            }
        }
        else
        {
            if (target == SkillTarget.SingleEnemy || target == SkillTarget.AllEnemy)
            {
                _enableFocus = true;
                _enableSelect = true;
            }
        }
    }

    protected virtual void OnSkillTargetSelect(object[] args)
    {
        _enableFocus = false;
        _enableSelect = false;
    }

    protected virtual void OnCharacterDeath(object[] args)
    {
        int battleId = (int)args[0];
        if (battleId != characterData.battleID) return;
        _enableFocus = false;
        _enableSelect = false;
        blackboard.boolDir["dieAnim"] = true;
        // gameObject.SetActive(false);
    }

    private void OnHpChange(int changeValue)
    {
        if (changeValue < 0)
        {
            blackboard.boolDir["getHurt"] = true;
        }
        // else if (changeValue > 0)
        // {
        //     blackboard.boolDir["getHeal"] = true;
        // }
    }

    #region 交互
    public void OnSelect()
    {
        BattleData.Instance.Emit(BattleData.SkillTargetSelect, characterData.battleID);
    }

    public void EndSelect()
    {
        BattleData.Instance.Emit(BattleData.SkillTargetSelect, characterData.battleID);
    }

    public void OnFocus()
    {
        var target = BattleData.Instance.CurrSkillConfig?.target;
        if (target == null) return;
        if (target == SkillTarget.AllHero || target == SkillTarget.AllEnemy)
        {
            BattleData.Instance.Emit(BattleData.SkillFocusCharacter, true);
            return;
        }
        OnCharacterSelect(1);
    }

    public void EndFocus()
    {
        BattleData.Instance.Emit(BattleData.SkillFocusCharacter, false);
        OnCharacterSelect(-1);
    }
    #endregion

}
