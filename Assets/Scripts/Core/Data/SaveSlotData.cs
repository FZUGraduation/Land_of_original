using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SaveSlotData : RuntimeData
{
    private static SaveSlotData instance;
    public static SaveSlotData Instance
    {
        get
        {
            return instance;
        }
    }
    [JsonIgnore]
    public Datalib datalib;

    public void Init()
    {
        datalib = Datalib.Instance;
    }
    public static void ReplaceInstance(SaveSlotData data)
    {
        instance = data;
        instance.Init();
    }

    #region SaveData
    private readonly Dictionary<string, bool> storyProgress = new();//用来存储剧情进度
    private int talentPoint = 0;
    private readonly Dictionary<string, bool> unlockTalent = new();//用来存储解锁的天赋
    public float bgmVolum = 1;
    public float seVolum = 1;
    public float voiceVolum = 1;
    public List<HeroRuntimeData> heroDatas = new();//用来存储解锁的英雄
    public InventoryRuntimeData bagData = new();
    #endregion


    public void AddHero(string heroKey)
    {
        if (heroDatas.Exists(e => e.ConfigKey == heroKey))
        {
            return;
        }
        heroDatas.Add(new HeroRuntimeData(heroKey));
    }
    /// <summary> 标记游戏进度 </summary>
    public void MarkStoryProgress(string storyId, bool isFinished = true)
    {
        if (storyProgress.ContainsKey(storyId))
        {
            storyProgress[storyId] = isFinished;
        }
        else
        {
            storyProgress.Add(storyId, isFinished);
        }
    }

    /// <summary> 检查游戏进度 </summary>
    public bool CheckStoryProgress(string storyId)
    {
        if (storyProgress.ContainsKey(storyId))
        {
            return storyProgress[storyId];
        }
        return false;
    }

    #region 天赋
    public int GetTalentPoint() => talentPoint;
    public void AddTalentPoint(int point)
    {
        talentPoint += point;
    }
    /// <summary> 解锁天赋 </summary>
    public void UnlockTalent(string talentkey)
    {
        if (unlockTalent.ContainsKey(talentkey) && unlockTalent[talentkey])
        {
            return;
        }
        var config = datalib.GetData<TalentConfigData>(talentkey);
        unlockTalent.Add(talentkey, true);
        talentPoint -= config.needPoint;
        SaveSlotEvent.Instance.Emit(SaveSlotEvent.UnlockTalent, talentkey);
    }

    /// <summary> 是否可以解锁某个天赋 </summary>
    public bool CanUnlockTalent(string talentkey)
    {
        var config = datalib.GetData<TalentConfigData>(talentkey);
        if (config == null)
        {
            return false;
        }
        foreach (var item in config.preTalents)
        {
            if (!CheckTalent(item.key))
            {
                return false;
            }
        }
        if (config.needPoint > talentPoint)
        {
            return false;
        }
        return true;
    }

    public bool CheckTalent(string talentId)
    {
        if (unlockTalent.ContainsKey(talentId))
        {
            return unlockTalent[talentId];
        }
        return false;
    }

    public void ResetAllTalent()
    {
        int point = 0;
        foreach (var item in unlockTalent)
        {
            if (!item.Value) continue;
            var config = datalib.GetData<TalentConfigData>(item.Key);
            point += config.needPoint;
        }
        talentPoint += point;
        unlockTalent.Clear();
        SaveSlotEvent.Instance.Emit(SaveSlotEvent.ResetAllTalent);
    }
    #endregion
}

public class SaveSlotEvent : SingletonEventCenter<SaveSlotEvent>
{
    public static readonly string SaveSlotData = GetEventName("SaveSltData");
    public static readonly string LoadSlotData = GetEventName("LoadSlotData");
    public static readonly string TalentSelect = GetEventName("TalentSelect");
    public static readonly string UnlockTalent = GetEventName("UnlockTalent");
    public static readonly string ResetAllTalent = GetEventName("ResetAllTalent");
    public static readonly string BagSlotSelect = GetEventName("BagSlotSelect");
}