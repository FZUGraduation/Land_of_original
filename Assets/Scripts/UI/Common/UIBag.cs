
using TMPro;
using UnityEngine;

public class UIBag : BaseDialog
{
    public GameObject itemPrefab;
    public Transform itemRoot;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDesc;
    public int minSlotNum = 16;
    public int columnNum = 4;
    private InventoryRuntimeData bagData;
    private NodeBagItem selectedItem;
    private int currSlotNum;


    protected override void Start()
    {
        base.Start();
        InitSlots();
    }

    private void InitSlots()
    {
        foreach (Transform child in itemRoot)
        {
            Destroy(child.gameObject);
        }
        bagData = SaveSlotData.Instance.bagData;
        if (itemPrefab == null)
        {
            Debug.LogError("itemPrefab is null");
            return;
        }
        if (itemRoot == null)
        {
            Debug.LogError("itemRoot is null");
            return;
        }
        foreach (Transform child in itemRoot)
        {
            Destroy(child.gameObject);
        }

        int bagCount = bagData.itemRtDataList.Count;
        currSlotNum = Mathf.Max(minSlotNum, bagCount + (columnNum - bagCount % columnNum));
        for (int i = 0; i < currSlotNum; i++)
        {
            //想优化的话可以用对象池
            GameObject itemObj = Instantiate(itemPrefab, itemRoot);
            NodeBagItem itemSlot = itemObj.GetComponent<NodeBagItem>();
            if (i < bagCount)
            {
                itemSlot.Init(bagData.itemRtDataList[i], OnSlotSelect);
            }
            else
            {
                itemSlot.Init(null, OnSlotSelect);
            }
            //默认选中第一个
            if (i == 0)
            {
                itemSlot.OnSelect();
            }
        }
    }

    private void OnSlotSelect(NodeBagItem itemSlot)
    {
        if (selectedItem != null && selectedItem != itemSlot)
        {
            selectedItem.UnSelect();
        }
        selectedItem = itemSlot;
        if (itemSlot.itemData == null)
        {
            itemName.text = "";
            itemDesc.text = "";
            return;
        }
        else
        {
            itemName.text = itemSlot.itemData.ConfigData.key;
            itemDesc.text = itemSlot.itemData.ConfigData.desc;
        }

    }
}
