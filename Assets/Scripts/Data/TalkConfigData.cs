using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class TalkConfigData : ConfigData
{
    [BoxGroup(STATS_BOX), LabelText("类别")]
    public TalkCategory category;
    [BoxGroup(STATS_BOX), LabelText("优先级")]
    public int priority;
    public List<SingleTalkData> talkDataList;
}

[Serializable]
public class SingleTalkData
{
    [OnValueChanged("OnTypeChanged")]
    public TalkType type = TalkType.Dialogue;
    [ShowIf("type", TalkType.Dialogue)]
    public bool isLeft = true;
    [ShowIf("type", TalkType.Dialogue)]
    public TalkPeopleConfigData talker;
    [TextArea(3, 10), LabelText("对话内容"), ShowIf("type", TalkType.Dialogue)]
    public string content;
    [LabelText("对话选项"), ShowIf("type", TalkType.Option)]
    public List<TalkOption> optionList;

    public void OnTypeChanged()
    {
        if (type == TalkType.Dialogue)
        {
            optionList = null;
        }
        else if (type == TalkType.Option)
        {
            content = null;
            isLeft = true;
            talker = null;
        }
    }
}
[Serializable]
public class TalkOption
{
    [LabelText("选项内容")]
    public string content;
    [LabelText("选项类型")]
    public TalkOptionType optionType;
    [LabelText("跳转对话"), ShowIf("optionType", TalkOptionType.JumpTalk)]
    public TalkConfigData nextTalk;
    [LabelText("战斗"), ShowIf("optionType", TalkOptionType.Battle)]
    public BattleLevelConfigData battle;
}

public enum TalkType
{
    Dialogue,
    Option,
    End
}

public enum TalkCategory
{
    Normal,//普通对话
    Battle,//战斗触发的对话
    NPC,//与npc交谈触发的对话
    Aside,//旁白
}

public enum TalkOptionType
{
    JumpTalk,
    Continue,
    End,
    Battle,
}