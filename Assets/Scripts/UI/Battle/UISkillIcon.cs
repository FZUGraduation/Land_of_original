using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image skillIcon = null;
    public Image bg = null;
    public Image slientIcon = null;
    public TextMeshProUGUI coolDownText = null;
    public Button button = null;
    public GameObject desc = null;
    public Transform mpCostParent;
    private string skillName;
    private BattleHeroData heroData;
    private SkillConfigData skillConfig;
    private Action<string> onClick;

    void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    public void Init(BattleHeroData heroData, SkillConfigData skillConfig, Action<string> onClick, bool isSlient = false)
    {
        int coolDown = heroData.GetSkillCoolDown(skillConfig.key);
        bool isInCoolDown = coolDown > 0;
        skillIcon.color = isInCoolDown ? Color.gray : Color.white;
        coolDownText.gameObject.SetActive(isInCoolDown);
        coolDownText.text = isInCoolDown ? coolDown.ToString() : string.Empty;
        this.heroData = heroData;
        this.skillConfig = skillConfig;
        skillName = skillConfig.key;
        this.onClick = onClick;
        skillIcon.sprite = skillConfig.icon;
        //如果mp不够，则按钮不可点击
        button.interactable = skillConfig.mpCost <= BattleData.Instance.MP && !isInCoolDown;
        //如果被沉默，且不是普攻，则显示沉默图标
        slientIcon.gameObject.SetActive(isSlient && skillConfig.isBasicAttack == false);
        for (int i = 0; i < skillConfig.mpCost; i++)
        {
            mpCostParent.GetChild(i).gameObject.SetActive(true);
        }
    }
    public void SetSelect(bool isSelect)
    {
        if (isSelect)
        {
            Debug.Log("选择技能：" + skillConfig.key);
            BattleData.Instance.Emit(BattleData.SkillSelect, skillConfig);
            bg.color = Color.green;
        }
        else
        {
            bg.color = Color.white;
        }
    }
    public void OnClick()
    {
        onClick?.Invoke(skillName);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        BattleData.Instance.Emit(BattleData.ShowDetail, skillConfig, transform.position + new Vector3(220, -50, 0));
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        BattleData.Instance.Emit(BattleData.HideDetail);
    }
}
