using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Demos.RPGEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BagSlotType
{
    Bag = 0,
    Equipment = 1,
}

public class NodeBagItem : MonoBehaviour
{
    public Image icon;
    public Image bg;
    public TextMeshProUGUI amountText;
    public Button button;
    public BagSlotType slotType = BagSlotType.Bag;

    [HideInInspector]
    public ItemRuntimeData itemData;
    private Action<NodeBagItem> callback;

    void Awake()
    {
        button.onClick.AddListener(OnSelect);
    }
    void OnDestroy()
    {
        if (itemData != null)
        {
            itemData.onAmountChange -= OnAmountChange;
        }
    }
    public void Init(ItemRuntimeData itemData, Action<NodeBagItem> callback)
    {
        if (this.itemData != null)
        {
            this.itemData.onAmountChange -= OnAmountChange;
        }
        if (itemData != null)
        {
            icon.sprite = itemData.ConfigData.icon;
            if (itemData.Amount > 1)
                amountText.text = itemData.Amount.ToString();
            else
                amountText.text = "";
            icon.gameObject.SetActive(true);
            this.itemData = itemData;
            itemData.onAmountChange += OnAmountChange;
        }
        else
        {
            this.itemData = null;
            icon.gameObject.SetActive(false);
            amountText.text = "";
        }
        this.callback = callback;
    }
    public void OnSelect()
    {
        Debug.Log("OnSelect Item: " + itemData?.ConfigKey);
        bg.color = Color.green;
        callback?.Invoke(this);
    }
    public void UnSelect()
    {
        bg.color = Color.white;
    }
    private void OnAmountChange(int amount)
    {
        if (amount <= 0)
        {
            icon.gameObject.SetActive(false);
            amountText.text = "";
            return;
        }
        amountText.text = amount.ToString();
    }
}
