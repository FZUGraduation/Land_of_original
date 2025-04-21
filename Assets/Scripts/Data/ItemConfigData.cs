using System;
using Sirenix.OdinInspector;

[Serializable]
public class ItemConfigData : ConfigData
{
    [BoxGroup(EXT_BOX_LEFT, LabelText = EXT1_CONFIG_TITLE)]
    public ItemCategory category = ItemCategory.None;

    [BoxGroup(EXT_BOX_LEFT)]
    public ItemFeature feature = ItemFeature.None;
}


[Serializable]//这个就包含了一个itemconfig，和一个amount，表示一个物品的数量，可以用来表示一个物品的消耗或者获得
public class ItemCost
{
    [TableColumnWidth(80, Resizable = false)]
    [PreviewField("@$value?.icon", height: 36, Alignment = ObjectFieldAlignment.Left)]
    [LabelText("@$value?.name")]
    public ItemConfigData configData;
    public int amount;

    public ItemCost() { }
    public ItemCost(ItemConfigData configData, int amount)
    {
        this.configData = configData;
        this.amount = amount;
    }
    public ItemCost(string name, int amount)
    {
        this.configData = Datalib.Instance.GetData<ItemConfigData>(name);
        this.amount = amount;
    }
}

public enum ItemCategory
{
    None = 0,
    Material,
    Food,
    Item,
    Equipment,
    Dice,
}
[Flags]
public enum ItemFeature
{
    None = 0,
    Usable = 1 << 1,

    Equipable = 1 << 2,

    Enhancable = 1 << 3,

    Meal = 1 << 4
}