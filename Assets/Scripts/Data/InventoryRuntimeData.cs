using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRuntimeData : RuntimeData
{
    public List<ItemRuntimeData> itemRtDataList = new();

    /// <summary>消耗列表中的物品,返回剩余还没有消耗的数量量/// </summary>
    public int UseItem(string configKey, int consumeAmount)
    {
        for (int i = 0; i < itemRtDataList.Count; i++)
        {
            if (itemRtDataList[i].ConfigData.key == configKey)
            {
                if (itemRtDataList[i].Amount >= consumeAmount)
                {
                    itemRtDataList[i].RemoveItem(consumeAmount);
                    if (itemRtDataList[i].Amount == 0)
                    {
                        itemRtDataList.RemoveAt(i);
                    }
                    return 0;
                }
                else
                {
                    consumeAmount -= itemRtDataList[i].Amount;
                    itemRtDataList[i].RemoveAllItem();
                    itemRtDataList.RemoveAt(i);
                    i--;
                    // itemRtDataList[i].SetEmptyRuntime();
                }
            }
        }
        return consumeAmount;
    }

    /// <summary>添加物品到列表中</summary>
    public void AddItem(ItemConfigData configData, int amount)
    {
        for (int i = 0; i < itemRtDataList.Count; i++)
        {
            if (itemRtDataList[i].ConfigData == configData)
            {
                itemRtDataList[i].Add(amount);
                return;
            }
        }
        itemRtDataList.Add(new ItemRuntimeData(configData.key, amount));
    }
    public void AddItem(string key, int amount)
    {
        ItemConfigData configData = Datalib.Instance.GetData<ItemConfigData>(key);
        AddItem(configData, amount);
    }

    /// <summary>返回列表中包含该物品的数量</summary>
    public int GetListContainerAmount(ItemConfigData configData)
    {
        int result = 0;
        for (int i = 0; i < itemRtDataList.Count; i++)
        {
            if (itemRtDataList[i].ConfigData == configData)
            {
                result += itemRtDataList[i].Amount;
            }
        }
        return result;
    }
}
