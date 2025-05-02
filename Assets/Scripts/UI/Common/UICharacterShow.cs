using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterShow : BaseDialog
{
    public RawImage rawImage;
    public TableView herotableView;
    public ModelRotator rotator;
    public TextMeshProUGUI nameText;
    private List<HeroRuntimeData> herodata;
    public NodeBagItem headSlot;
    public NodeBagItem bodySlot;
    public NodeBagItem legsSlot;
    public NodeBagItem backSlot;
    public NodeBagItem weaponSlot;
    // public NodeBagItem followerSlot;
    public Dictionary<EquipmentType, NodeBagItem> equipmentSlot = new();
    [FoldoutGroup("Bag")] public GameObject itemPrefab;
    [FoldoutGroup("Bag")] public Transform itemRoot;
    [FoldoutGroup("Bag")] public TextMeshProUGUI itemName;
    [FoldoutGroup("Bag")] public TextMeshProUGUI itemDesc;
    [FoldoutGroup("Bag")] public Button equipButton;//装备按钮
    [FoldoutGroup("Bag")] public Button unEquipButton;//卸下按钮
    [FoldoutGroup("Bag")] public int minSlotNum = 16;
    [FoldoutGroup("Bag")] public int columnNum = 4;
    private InventoryRuntimeData bagData;
    private NodeBagItem selectedItem;
    private int currSlotNum;
    private HeroRuntimeData selectedHeroData;
    protected override void Start()
    {
        base.Start();
        equipmentSlot.Add(EquipmentType.Head, headSlot);
        equipmentSlot.Add(EquipmentType.Body, bodySlot);
        equipmentSlot.Add(EquipmentType.Legs, legsSlot);
        equipmentSlot.Add(EquipmentType.Back, backSlot);
        equipmentSlot.Add(EquipmentType.Weapon, weaponSlot);
        herodata = SaveSlotData.Instance.heroDatas;
        herotableView.Init(herodata, OnCharacterSelected);
        InitBagSlots();
        InitEquipmentSlots();
        equipButton.onClick.AddListener(OnEquipButtonClick);
        unEquipButton.onClick.AddListener(OnUnEquipButtonClick);
    }

    private void OnCharacterSelected(int index, HeroRuntimeData data)
    {
        selectedHeroData = data;
        var configData = data.ConfigData;
        nameText.text = configData.key;
        var rttObject = RTTManager.Instance.GetRttObject(this);
        if (rttObject != null)
        {
            rttObject.ReplaceRttPrefab(configData.modelPrefab);
        }
        else
        {
            rttObject = RTTManager.Instance.CreateRttObject(this, configData.modelPrefab, 512, 1024);
            rawImage.texture = rttObject.renderTexture;
        }
        rotator.targetModel = rttObject.rttObjectRoot.transform;

        Debug.Log($"OnCellSelected: {index}, {configData.key}");
    }

    private void InitEquipmentSlots()
    {
        foreach (var item in equipmentSlot)
        {
            item.Value.transform.GetChild(2).gameObject.SetActive(true);
            if (selectedHeroData.equipmentData.ContainsKey(item.Key) == false)
            {
                item.Value.Init(null, OnBagSlotSelect);
                continue;
            }
            var equipKey = selectedHeroData.equipmentData[item.Key];
            if (string.IsNullOrEmpty(equipKey))
            {
                item.Value.Init(null, OnBagSlotSelect);
                continue;
            }
            ItemRuntimeData itemData = new(equipKey);
            item.Value.Init(itemData, OnBagSlotSelect);
            item.Value.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    private void InitBagSlots()
    {
        foreach (Transform child in itemRoot)
        {
            Destroy(child.gameObject);
        }
        bagData = SaveSlotData.Instance.bagData;
        int bagCount = bagData.itemRtDataList.Count;
        currSlotNum = Mathf.Max(minSlotNum, bagCount + (columnNum - bagCount % columnNum));
        for (int i = 0; i < currSlotNum; i++)
        {
            //想优化的话可以用对象池
            GameObject itemObj = Instantiate(itemPrefab, itemRoot);
            NodeBagItem itemSlot = itemObj.GetComponent<NodeBagItem>();
            if (i < bagCount)
            {
                itemSlot.Init(bagData.itemRtDataList[i], OnBagSlotSelect);
            }
            else
            {
                itemSlot.Init(null, OnBagSlotSelect);
            }
            //默认选中第一个
            if (i == 0)
            {
                itemSlot.OnSelect();
            }
        }
    }

    private void RefreshBagSlots()
    {
        int bagCount = bagData.itemRtDataList.Count;
        currSlotNum = Mathf.Max(minSlotNum, bagCount + (columnNum - bagCount % columnNum));
        int childNum = itemRoot.childCount;
        for (int i = 0; i < currSlotNum; i++)
        {
            NodeBagItem itemSlot = null;
            if (i < childNum)
            {
                itemSlot = itemRoot.GetChild(i).GetComponent<NodeBagItem>();
            }
            else
            {
                GameObject itemObj = Instantiate(itemPrefab, itemRoot);
                itemSlot = itemObj.GetComponent<NodeBagItem>();
            }
            if (i < bagCount)
            {
                itemSlot.Init(bagData.itemRtDataList[i], OnBagSlotSelect);
            }
            else
            {
                itemSlot.Init(null, OnBagSlotSelect);
            }
            //默认选中第一个
            if (i == 0)
            {
                itemSlot.OnSelect();
            }
        }
    }

    private void OnBagSlotSelect(NodeBagItem itemSlot)
    {
        if (selectedItem != null && selectedItem != itemSlot)
        {
            selectedItem.UnSelect();
        }
        selectedItem = itemSlot;
        if (itemSlot.itemData == null)
        {
            equipButton.gameObject.SetActive(false);
            unEquipButton.gameObject.SetActive(false);
            itemName.text = "";
            itemDesc.text = "";
            return;
        }
        itemName.text = itemSlot.itemData.ConfigData.key;
        itemDesc.text = itemSlot.itemData.ConfigData.desc;
        bool showEquipButton = itemSlot.slotType == BagSlotType.Bag && itemSlot.itemData.ConfigData.category == ItemCategory.Equipment;
        bool showUnEquipButton = itemSlot.slotType == BagSlotType.Equipment;
        equipButton.gameObject.SetActive(showEquipButton);
        unEquipButton.gameObject.SetActive(showUnEquipButton);
    }

    private void OnUnEquipButtonClick()
    {
        var equipType = EquipmentType.None;
        foreach (var item in equipmentSlot)
        {
            if (selectedItem == item.Value)
            {
                equipType = item.Key;
                break;
            }
        }
        var oldEquipKey = selectedHeroData.SwitchEquipment(equipType, null);
        if (!string.IsNullOrEmpty(oldEquipKey))
        {
            bagData.AddItem(oldEquipKey, 1);
        }
        InitEquipmentSlots();
        RefreshBagSlots();
    }

    private void OnEquipButtonClick()
    {
        var equipCfg = selectedItem.itemData.ConfigData as EquipmentConfigData;
        var oldEquipKey = selectedHeroData.SwitchEquipment(equipCfg.equipmentType, selectedItem.itemData.ConfigKey);
        bagData.UseItem(equipCfg.key, 1);
        if (!string.IsNullOrEmpty(oldEquipKey))
        {
            bagData.AddItem(oldEquipKey, 1);
        }
        InitEquipmentSlots();
        RefreshBagSlots();
    }

    void OnDestroy()
    {
        RTTManager.Instance.DestroyRttObject(this);
    }
}
