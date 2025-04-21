
using UnityEngine;
using UnityEngine.UI;

public class NodeTalentItem : MonoBehaviour
{
    public string talentKey = "";
    public Image icon;
    public Button button;
    public Image bg;
    void Awake()
    {
        SaveSlotEvent.Instance.On(SaveSlotEvent.UnlockTalent, OnUnlockTalent, this);
        // SaveSlotEvent.Instance.On(SaveSlotEvent.TalentSelect, OnTalentSelect, this);
        SaveSlotEvent.Instance.On(SaveSlotEvent.ResetAllTalent, RefreshTalentShow, this);
        // SaveSlotEvent.Instance.On(SaveSlotEvent.TalentSelect, OnDeselectTalent, this);
    }

    void OnDestroy()
    {
        SaveSlotEvent.Instance.OffAll(this);
    }
    void Start()
    {
        button.onClick.AddListener(OnSelectTalent);
        icon.sprite = Datalib.Instance.GetData<TalentConfigData>(talentKey).icon;

        RefreshTalentShow();
    }

    private void OnUnlockTalent(object[] args)
    {
        string talentKey = (string)args[0];
        if (talentKey == this.talentKey)
        {
            RefreshTalentShow();
        }
    }
    private void RefreshTalentShow()
    {
        bool isUnlock = SaveSlotData.Instance.CheckTalent(talentKey);
        icon.color = isUnlock ? Color.white : Color.gray;
    }

    public void OnSelectTalent()
    {
        Debug.Log("Select Talent: " + talentKey);
        bg.color = Color.red;
        SaveSlotEvent.Instance.Emit(SaveSlotEvent.TalentSelect, this);
    }
    public void OnDeselectTalent()
    {
        bg.color = Color.white;
    }
}
