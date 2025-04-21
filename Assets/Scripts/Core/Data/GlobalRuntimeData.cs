using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GlobalRuntimeData : RuntimeData
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
    }

    #region SaveData
    public float bgmVolum = 1;
    public float seVolum = 1;
    public float voiceVolum = 1;
    #endregion
}
