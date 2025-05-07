using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGetItem : BaseDialog
{
    public Transform contentTransform;
    public GameObject itemPrefab;

    public override void Init(params object[] data)
    {
        List<ItemCost> itemCosts = (List<ItemCost>)data[0];
        if (itemCosts == null || itemCosts.Count == 0)
        {
            Debug.LogError("没有物品可以领取");
            Close();
            return;
        }
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            Destroy(contentTransform.GetChild(i).gameObject);
        }
        foreach (var item in itemCosts)
        {
            var itemShow = Instantiate(itemPrefab, contentTransform);
            itemShow.GetComponent<NodeBagItem>().InitWithConfig(item.configData, item.amount);
            SaveSlotData.Instance.bagData.AddItem(item.configData, item.amount);
        }
    }

}