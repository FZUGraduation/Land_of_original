
using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class ItemRuntimeData : RuntimeData
{

    public ItemConfigData ConfigData
    {
        get;
        protected set;
    }


    [JsonProperty]
    public virtual string ConfigKey
    {
        get//序列化时,ConfigKey 的 get 访问器会被调用，返回 ConfigData.key 的值。
        {
            return ConfigData.key;
        }
        set//反序列化时,ConfigKey 的 set 访问器会被调用，使用反序列化后的值来设置 ConfigData。
        {
            var data = Datalib.Instance.GetData<ItemConfigData>(value);
            if (data == null)
            {
                data = Datalib.Instance.GetData<EquipmentConfigData>(ConfigKey);
            }
            ConfigData = data;
        }
    }

    public bool IsEmpty
    {
        get { return ConfigData == null || Amount <= 0; }
    }

    public Action<int> onAmountChange;

    [DefaultValue(1), JsonProperty]
    private int amount = 1;

    public int Amount
    {
        get { return amount; }
        private set
        {
            if (amount == value)
            {
                return;
            }
            onAmountChange?.Invoke(value);
            amount = value;
        }
    }

    public ItemRuntimeData() { }
    public ItemRuntimeData(string key, int amount = 1)
    {
        Amount = amount;
        ConfigKey = key;
    }
    public ItemRuntimeData(ItemConfigData configData, int amount = 1)
    {
        Amount = amount;
        ConfigData = configData;
    }
    //给物品移除数量
    public bool RemoveItem(int removeAmount)
    {
        if (Amount < removeAmount)
        {
            Debug.LogError("RemoveItem: amount is more than the item has, item key: " + ConfigKey);
            return false;
        }
        Amount -= removeAmount;
        return true;
    }

    public void RemoveAllItem()
    {
        Amount -= amount;
    }

    //给物品添加数量
    public void Add(int amount)
    {
        Amount += amount;
    }
}
