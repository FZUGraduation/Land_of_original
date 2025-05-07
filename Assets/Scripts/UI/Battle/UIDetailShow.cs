
using TMPro;
using UnityEngine;

public class UIDetailShow : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;

    void Awake()
    {
        BattleData.Instance.On(BattleData.ShowDetail, OnShowSkillDetail, this);
        BattleData.Instance.On(BattleData.HideDetail, OnHideSkillDetail, this);
        gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        BattleData.Instance?.OffAll(this);
    }
    public void Init(object data, Vector3 pos)
    {
        switch (data)
        {
            case SkillConfigData skillConfig:
                title.text = skillConfig.key;
                desc.text = skillConfig.desc;
                transform.position = pos;
                break;
            case BaseEffect effect:
                title.text = effect.effectName;
                desc.text = effect.description;
                transform.position = pos;
                break;
        }
    }

    private void OnShowSkillDetail(object[] obj)
    {
        if (obj.Length < 2)
        {
            return;
        }
        Init(obj[0], (Vector3)obj[1]);
        gameObject.SetActive(true);
    }

    private void OnHideSkillDetail(object[] obj)
    {
        gameObject.SetActive(false);
    }
}
