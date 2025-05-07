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
    public string saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    [JsonProperty]
    private Dictionary<string, int> storyProgress = new();//用来存储剧情进度
    [JsonProperty]
    private Dictionary<string, bool> unlockTalent = new();//用来存储解锁的天赋
    [JsonProperty]
    public Dictionary<string, bool> unlockTreasure = new();//已经获得的宝箱
    public List<string> talkedKey = new();//用来存储已经对话
    public float bgmVolum = 0.5f;
    public float seVolum = 0.5f;
    public float voiceVolum = 0.5f;
    public List<HeroRuntimeData> heroDatas = new();//用来存储解锁的英雄
    public InventoryRuntimeData bagData = new();
    public List<string> passLevels = new();//用来存储通关的关卡
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
    public void MarkStoryProgress(string storyId, int isFinished = 1)
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
            return storyProgress[storyId] == 1;
        }
        return false;
    }

    #region 天赋
    public int GetTalentPoint() => bagData.GetListContainerAmount("金币");

    /// <summary> 解锁天赋 </summary>
    public void UnlockTalent(string talentkey)
    {
        if (unlockTalent.ContainsKey(talentkey) && unlockTalent[talentkey])
        {
            return;
        }
        var config = datalib.GetData<TalentConfigData>(talentkey);
        unlockTalent.Add(talentkey, true);
        bagData.UseItem("金币", config.needPoint);
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
        if (config.needPoint > GetTalentPoint())
        {
            return false;
        }
        return true;
    }

    public bool IsUnlockPreTalent(string talentkey)
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
        bagData.AddItem("金币", point);
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