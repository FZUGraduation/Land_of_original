using System;

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
    public GameObject coinGO;
    public Image bg;
    public TextMeshProUGUI amountText;
    public Button button;
    public BagSlotType slotType = BagSlotType.Bag;

    [HideInInspector]
    public ItemRuntimeData itemData;
    private Action<NodeBagItem> callback;

    private bool canSelect = true;
    void Awake()
    {
        if (canSelect)
        {
            button.onClick.AddListener(OnSelect);
        }
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
        if (this.itemData != null && itemData.ConfigData.key == "金币")
        {
            coinGO.SetActive(true);
            icon.gameObject.SetActive(false);
        }
        else
        {
            coinGO.SetActive(false);
        }
    }

    public void InitWithConfig(ItemConfigData configData, int num)
    {
        if (configData != null)
        {
            icon.sprite = configData.icon;
            if (num > 1)
                amountText.text = num.ToString();
            else
                amountText.text = "";
            icon.gameObject.SetActive(true);
        }
        else
        {
            icon.gameObject.SetActive(false);
            amountText.text = "";
        }
        canSelect = false;
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
