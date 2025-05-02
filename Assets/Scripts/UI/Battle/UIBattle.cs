using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FlyTextType
{
    Buff,
    HPChange,
}

public class UIBattle : MonoBehaviour
{
    public GameObject actionIconPrefab;//角色头像预制体
    public Transform actionIconRoot;//角色头像根节点
    public GameObject skillPrefab;//技能预制体
    public Transform skillRoot;//技能根节点
    public GameObject flyTextPrefab;//飘字预制体
    public Transform flyTextRoot;//飘字根节点
    public GameObject statusPrefab;//状态预制体
    public Transform statusRoot;//状态根节点
    public Transform mpBarRoot;//mp根节点
    public TextMeshProUGUI mpText;//蓝量
    public TextMeshProUGUI roundText;//回合数
    public Button enemyDetailBtn;
    private List<UIActionIcon> actionIconList = new();
    private Dictionary<string, UISkillIcon> skillIconDic = new();
    private Dictionary<int, CharacterStatusBar> statusBarDic = new();
    private Queue<FlyTextData> flyTextQueue = new();
    private bool isOnFlyText = false;
    private string currSkill = "";
    private Camera mainCamera;
    private int currMP = 0;

    public void Awake()
    {
        enemyDetailBtn.onClick.AddListener(OnEnemyDetailBtnClick);
        BattleData.Instance.On(BattleData.GenerateCharacter, OnGenerateCharacter, this);
        BattleData.Instance.On(BattleData.BattleStart, OnStartBattle, this);
        BattleData.Instance.On(BattleData.ActionStart, OnActionStart, this);
        BattleData.Instance.On(BattleData.SkillTargetSelect, OnSkillTargetSelect, this);
        BattleData.Instance.On(BattleData.ShowFlyText, OnShowFlyText, this);
        BattleData.Instance.On(BattleData.RoundStart, OnRoundStart, this);
        BattleData.Instance.On(BattleData.ActionEnd, OnActionEnd, this);
        BattleData.Instance.On(BattleData.AddEffect, OnAddEffect, this);
        BattleData.Instance.On(BattleData.RemoveEffect, OnRemoveEffect, this);
        BattleData.Instance.On(BattleData.CharacterDeath, OnCharacterDeath, this);
        BattleData.Instance.On(BattleData.MPChange, OnMPChange, this);
        BattleData.Instance.On(BattleData.BattleEnd, OnBattleEnd, this);
    }

    public void OnEnable()
    {

    }
    public void Start()
    {
        skillRoot.gameObject.SetActive(false);
        actionIconRoot.gameObject.SetActive(false);
        mainCamera = Camera.main;
        SetMP(BattleData.Instance.MP);

        BattleData.Instance.Emit(BattleData.BattleStart);
        BattleData.Instance.Emit(BattleData.ActionNext);
    }

    private void OnBattleEnd(object[] args)
    {
        bool isWin = (bool)args[0];
        WindowManager.Instance.ShowDialog(UIDefine.UIBattleOver, UIIndex.STACK, isWin);
    }

    private void OnEnemyDetailBtnClick()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIEnemyDetail, UIIndex.STACK, BattleData.Instance.battleLevel);
    }
    private void OnStartBattle()
    {
    }

    private void OnActionEnd(object[] obj)
    {
        int battleID = (int)obj[0];
        actionIconList.Find(a => a.battleID == battleID).gameObject.SetActive(false);
    }

    private void OnRoundStart(object[] args)
    {
        int round = (int)args[0];
        roundText.text = $"第{round}回合";
        _ = RountStartAnim(round);
    }

    async UniTaskVoid RountStartAnim(int round)
    {
        //TODO:显示回合数 动画等
        roundText.transform.DOScale(Vector3.one * 2, 0.5f).From().SetEase(Ease.OutBack);
        await roundText.transform.DOMoveY(roundText.transform.position.y - 500, 0.5f).From().AsyncWaitForCompletion();

        //生成回合顺序
        int childCount = actionIconRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(actionIconRoot.GetChild(i).gameObject);
        }
        actionIconList.Clear();
        actionIconRoot.gameObject.SetActive(true);
        var actionTurnCharacter = BattleData.Instance.GetCurrActionTurn();
        for (int i = actionTurnCharacter.Count - 1; i >= 0; i--)
        {
            var heroData = actionTurnCharacter[i];
            var icon = Instantiate(actionIconPrefab, actionIconRoot);
            var com = icon.GetComponent<UIActionIcon>();
            com.Init(heroData);
            actionIconList.Add(com);
        }

        BattleData.Instance.Emit(BattleData.ActionNext);
    }

    private void OnActionStart(object[] args)
    {
        bool isHero = (bool)args[1];
        int battleId = (int)args[0];
        if (isHero)
        {
            skillRoot.gameObject.SetActive(true);
            var heroData = (BattleHeroData)BattleData.Instance.GetCharacterData((int)args[0]);
            if (heroData.ActionType != BattleActionType.Dead)
            {
                InitHeroSkill(heroData);
            }
            // InitHeroSkill(heroData);
        }
        actionIconList.Find(a => a.battleID == battleId).SetAction();
    }

    private void InitHeroSkill(BattleHeroData heroData)
    {
        skillIconDic.Clear();
        var heroConfig = heroData.heroConfig;
        var skills = heroConfig.skills;

        int childCount = skillRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(skillRoot.GetChild(i).gameObject);
        }
        bool isSilent = heroData.GetSpecialEffectValue(SpecialEffectType.Silent) != null;
        for (int i = 0; i < skills.Count; i++)
        {
            //判断是否有解锁这个技能
            if (skills[i].needUnlock && !SaveSlotData.Instance.CheckTalent(skills[i].unlockTalent.key))
            {
                continue;
            }
            var skill = Instantiate(skillPrefab, skillRoot);
            var skillIcon = skill.GetComponent<UISkillIcon>();
            //初始化技能图标，i==0默认是普工，无法被沉默
            skillIcon.Init(heroData, skills[i], OnSkillSelect, isSilent);
            skillIconDic.Add(skills[i].key, skillIcon);
            if (i == 0)
            {
                OnSkillSelect(skills[i].key);
            }
        }
    }

    public void OnSkillSelect(string skillName)
    {
        if (currSkill == skillName || string.IsNullOrEmpty(skillName) || !skillIconDic.ContainsKey(skillName))
        {
            return;
        }
        if (skillIconDic.ContainsKey(currSkill))
        {
            skillIconDic[currSkill].SetSelect(false);
        }
        currSkill = skillName;
        skillIconDic[currSkill].SetSelect(true);
    }

    private void OnSkillTargetSelect(object[] obj)
    {
        skillRoot.gameObject.SetActive(false);
        currSkill = "";
    }

    private void OnShowFlyText(object[] obj)
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }
        flyTextQueue.Enqueue(new FlyTextData
        {
            behitGameObject = (GameObject)obj[0],
            flyTextType = (FlyTextType)obj[1],
            content = (string)obj[2],
            isCritical = (bool)obj[3],
        });
        if (!isOnFlyText)
        {
            StartCoroutine(ShowFlyTextQueue());
        }
    }

    private void ShowFlyText(FlyTextData data)
    {
        // 使用目标物体的位置
        Vector3 worldPosition = data.behitGameObject.transform.position;

        //世界坐标转屏幕坐标
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        if (data.flyTextType == FlyTextType.Buff)
        {
            screenPosition.y += 60;
        }
        else
        {
            screenPosition.y += UnityEngine.Random.Range(-30, 30);
            screenPosition.x += UnityEngine.Random.Range(-30, 30);
        }

        //实例化预制体
        GameObject numberInstance = Instantiate(flyTextPrefab, screenPosition, Quaternion.identity, flyTextRoot);
        numberInstance.transform.position = screenPosition;
        // // 设置为最顶层，防止玩家或怪物挡住跳字
        // numberInstance.transform.SetAsLastSibling();
        var text = numberInstance.GetComponent<TextMeshProUGUI>();

        //区分是否暴击
        Color color = Color.white;
        int fontSize = 35;

        if (data.flyTextType == FlyTextType.HPChange)
        {
            bool isCritical = data.isCritical;
            bool isDamage = data.content.Contains("-");
            if (!isDamage)
            {
                color = Color.green;//治疗绿色
            }
            if (isCritical)
            {
                fontSize = 50;//暴击字体变大
                text.fontStyle = FontStyles.Bold;//加粗
            }
            if (isDamage && isCritical)
            {
                color = Color.red;//造成伤害且暴击红色
            }
        }
        text.color = color;
        text.fontSize = fontSize;
        text.text = data.content;
        // 使用 DOTween 让number进行移动从Y=0移动到y=800,然后销毁
        //设置一个浮动范围//OnComplete()是动画完成后的回调函数
        int jumpfloat = UnityEngine.Random.Range(0, 100);
        //让这个数字先一边从小到大，然后再一边上浮，然后再一边消失
        numberInstance.transform.DOScale(Vector3.one * 0.2f, 0.2f).From().OnComplete(() =>
        {
            numberInstance.transform.DOScale(Vector3.one * 0.8f, 0.2f).OnComplete(() => Destroy(numberInstance));
        });
        numberInstance.transform.DOMoveY(numberInstance.transform.position.y + 50, 0.3f);
    }

    private IEnumerator ShowFlyTextQueue()
    {
        isOnFlyText = true;
        while (flyTextQueue.Count > 0)
        {
            ShowFlyText(flyTextQueue.Dequeue());
            yield return null;
            yield return null;
        }
        isOnFlyText = false;
    }
    private void OnGenerateCharacter(object[] obj)
    {
        var character = (BattleCharacterData)obj[0];
        Transform targetTransform = (Transform)obj[1];
        var status = Instantiate(statusPrefab, statusRoot);
        // status.transform.position = pos;
        var statusCom = status.GetComponent<CharacterStatusBar>();
        statusCom.Init(targetTransform);
        character.GetStat(StatType.HP).OnValueModified += (value) =>
        {
            statusCom?.SetHp(value.BaseValue, value.MaxValue);//监听血量的变化
        };
        statusBarDic.Add(character.battleID, statusCom);
    }
    protected void OnAddEffect(object[] args)
    {
        int battleId = (int)args[0];
        if (statusBarDic.ContainsKey(battleId))
        {
            var mod = (BaseEffect)args[1];
            statusBarDic[battleId].AddEffect(mod);
        }
    }

    protected void OnRemoveEffect(object[] args)
    {
        int battleId = (int)args[0];
        if (statusBarDic.ContainsKey(battleId))
        {
            var mod = (BaseEffect)args[1];
            statusBarDic[battleId].RemoveEffect(mod);
        }
    }

    private void OnCharacterDeath(object[] args)
    {
        int battleId = (int)args[0];
        if (statusBarDic.ContainsKey(battleId))
        {
            Destroy(statusBarDic[battleId].gameObject);
            statusBarDic.Remove(battleId);
        }
    }

    private void OnMPChange(object[] args)
    {
        var mp = (StatValueRuntimeData)args[0];
        if (mp.ModifiedValue == currMP)
        {
            return;
        }
        SetMP((int)mp.ModifiedValue);

    }
    private void SetMP(int mp)
    {
        mpText.text = $"MP: {mp}/{7}";
        currMP = mp;
        for (int i = 0; i < mpBarRoot.childCount; i++)
        {
            if (i < mp)
            {
                mpBarRoot.GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                mpBarRoot.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}

public struct FlyTextData
{
    public GameObject behitGameObject;
    public FlyTextType flyTextType;
    public string content;
    public bool isCritical;
}