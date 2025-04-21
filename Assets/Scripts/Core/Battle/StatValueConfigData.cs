using Sirenix.OdinInspector;

[System.Serializable]
public class StatValueConfigData : ConfigData
{
    [BoxGroup(LEFT_VERTICAL_GROUP + "/Limit", LabelText = "约束")]
    public float baseValueMin = 0f;
    [BoxGroup(LEFT_VERTICAL_GROUP + "/Limit")]
    public float baseValueMax = 9999f;

    [BoxGroup(LEFT_VERTICAL_GROUP + "/Limit")]
    public float modifieValueMin = 0f;
    [BoxGroup(LEFT_VERTICAL_GROUP + "/Limit")]
    public float modifieValueMax = 9999f;
}