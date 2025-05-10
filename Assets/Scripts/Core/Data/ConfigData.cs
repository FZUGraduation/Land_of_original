
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
[System.Serializable]
public class ConfigData : SerializedScriptableObject
{
    [HideLabel, PreviewField(55)]
    [VerticalGroup("TDSplit")]
    [HorizontalGroup("TDSplit/LRSplit", 0.6f)]
    [VerticalGroup("TDSplit/LRSplit/Left")]
    [BoxGroup("TDSplit/LRSplit/Left/General Settings", LabelText = "通用")]
    [HorizontalGroup("TDSplit/LRSplit/Left/General Settings/Split", 55, LabelWidth = 67)]
    public Sprite icon;

    [OnValueChanged("Refresh")]
    [HorizontalGroup("TDSplit/LRSplit/Left/General Settings/Split")]
    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    [LabelText("键")]
    public string key = "New Instance";

    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    [InlineButton("CopyKey", "C")]
    [LabelText("显示名")]
    public string displayedName = "";

    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    [InlineButton("NullRefence", "N")]
    public GameObject prefab;

    [VerticalGroup("TDSplit/LRSplit/Right")]
    [BoxGroup("TDSplit/LRSplit/Right/Desc", LabelText = "描述")]
    [HideLabel, TextArea(4, 14)]
    public string desc;

    [VerticalGroup("TDSplit/LRSplit/Right")]
    [BoxGroup("TDSplit/LRSplit/Right/Notes", LabelText = "备注")]
    [HideLabel, TextArea(4, 9)]
    public string note;

    protected const string STATS_BOX = LEFT_VERTICAL_GROUP + "/Stat";

    protected const string EXT_BOX_LEFT = LEFT_VERTICAL_GROUP + "/Ext";

    protected const string EXT_BOX_RIGNT = RIGHT_VERTICAL_GROUP + "/Ext";

    protected const string LEFT_VERTICAL_GROUP = "TDSplit/LRSplit/Left";
    protected const string RIGHT_VERTICAL_GROUP = "TDSplit/LRSplit/Right";

    protected const string DOWN_VERTICAL_GROUP = "TDSplit/";

    protected const string BASE_CONFIG_TITLE = "状态";
    protected const string EXT1_CONFIG_TITLE = "属性";
    protected const string EXT2_CONFIG_TITLE = "扩展2";
    protected const string DOWN_CONFIG_TITLE = "复杂类型";


    private void CopyKey()
    {

        this.displayedName = key;
    }

    private void NullRefence()
    {
        this.prefab = null;
    }

    public virtual object ShadowCopy()
    {
        return this.MemberwiseClone();
    }

    public virtual void Refresh()
    {
#if UNITY_EDITOR
        this.name = key;
        EditorUtility.SetDirty(this);
#endif
    }

    public override string ToString()
    {
        return key;
    }

    public virtual void GetIcon()
    {

    }
}

