using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITalent : BaseDialog
{
    public Button unlockButton;
    public Button resetButton;
    public TextMeshProUGUI talentName;
    public TextMeshProUGUI talentDetail;
    public TextMeshProUGUI talentPoint;
    public TextMeshProUGUI usePointTip;
    public NodeTalentItem firstTalent;
    private TalentConfigData currTalent;
    private NodeTalentItem currTalentItem;
    protected override void Awake()
    {
        base.Awake();
        SaveSlotEvent.Instance.On(SaveSlotEvent.TalentSelect, OnTalentSelect, this);
    }

    protected override void OnDestroy()
    {
        SaveSlotEvent.Instance.OffAll(this);
    }

    protected override void Start()
    {
        base.Start();
        unlockButton.onClick.AddListener(UnlockTalentBtn);
        resetButton.onClick.AddListener(ResetAllTalentBtn);

        currTalent = Datalib.Instance.GetData<TalentConfigData>(firstTalent.talentKey);
        firstTalent.OnSelectTalent();
    }

    private void OnTalentSelect(object[] args)
    {
        NodeTalentItem talentItem = (NodeTalentItem)args[0];
        if (currTalentItem != null)
        {
            currTalentItem.OnDeselectTalent();
        }
        currTalentItem = talentItem;
        string talentKey = talentItem.talentKey;
        currTalent = Datalib.Instance.GetData<TalentConfigData>(talentKey);
        RefreshTalnetShow();
    }

    private void UnlockTalentBtn()
    {
        SaveSlotData.Instance.UnlockTalent(currTalent.key);
        RefreshTalnetShow();
    }

    private void ResetAllTalentBtn()
    {
        SaveSlotData.Instance.ResetAllTalent();
    }

    private void RefreshTalnetShow()
    {
        talentName.text = currTalent.name;
        talentDetail.text = currTalent.desc;
        talentPoint.text = ":" + SaveSlotData.Instance.GetTalentPoint().ToString();
        bool isUnlock = SaveSlotData.Instance.CheckTalent(currTalent.key);
        bool canUnlock = SaveSlotData.Instance.CanUnlockTalent(currTalent.key);
        unlockButton.gameObject.SetActive(!isUnlock);
        usePointTip.gameObject.SetActive(!isUnlock);
        if (SaveSlotData.Instance.IsUnlockPreTalent(currTalent.key))
        {
            usePointTip.text = "需要金币: " + currTalent.needPoint.ToString();
        }
        else
        {
            usePointTip.text = "需解锁前置天赋";
        }

        unlockButton.interactable = canUnlock;
    }
}
