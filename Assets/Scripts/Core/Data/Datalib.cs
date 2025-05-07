using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Datalib : Singleton<Datalib>
{
    public Dictionary<Type, List<ConfigData>> dataDict = null;
    public Dictionary<string, (string intro, string main)> bgmConfig = new Dictionary<string, (string intro, string main)>() { };
#if UNITY_EDITOR
    public List<EntityCreator> creatorList = null;
#endif
    const string configDataRoot = "Assets/OriginData/Data/";
#if UNITY_EDITOR
    public delegate string GetSubPathDel(ConfigData data);
    public static readonly List<(Type type, string dpName, string path, GetSubPathDel subPath)> configTypeOrder = new List<(Type, string, string, GetSubPathDel)>(){
            (typeof(ItemConfigData), "物品", "物品", null),
            (typeof(HeroConfigData),"角色","角色",null),
            (typeof(EquipmentConfigData),"装备","装备",null),
            (typeof(TalkConfigData),"对话","对话",GetTalkCategoryName),
            (typeof(TalkPeopleConfigData),"对话人物","对话人物",null),
            (typeof(EnemyConfigData),"敌人","敌人",null),
            (typeof(BattleLevelConfigData),"关卡","关卡",null),
            (typeof(StatValueConfigData),"数值","数值",null),
            (typeof(EffectConfigData),"技能效果","技能效果",GetEffectTypeName),
            (typeof(SkillConfigData),"技能","技能",null),
            (typeof(TalentConfigData),"天赋","天赋",null),
            (typeof(OutSideGrowthConfigData),"局外成长","局外成长",null),
            (typeof(TreasureConfigData),"宝箱","宝箱",null),};
    public static string GetTalkCategoryName(ConfigData data)
    {
        var itemData = data as TalkConfigData;
        return itemData.category switch
        {
            TalkCategory.Normal => "普通对话",
            TalkCategory.Battle => "战斗对话",
            TalkCategory.NPC => "NPC对话",
            TalkCategory.Aside => "旁白",
            _ => "未知",
        };
    }
    public static string GetEffectTypeName(ConfigData data)
    {
        var itemData = data as EffectConfigData;
        return itemData.effectType switch
        {
            EffectType.None => "无",
            EffectType.StatModifuer => "数值效果",
            EffectType.SpecialEffect => "特殊效果",
            _ => "未知",
        };
    }
#endif

    public void CreateData()
    {
#if UNITY_EDITOR
        bool needSave = false;
        //如果该类型未创建，则创建一个新的Asset
        var configDataType = typeof(ConfigData);
        foreach (var info in configTypeOrder)
        {
            //单纯当一个空类型来做Root，不实际引用。
            var path = $"{configDataRoot}/{info.dpName}.asset";
            var mainAsset = AssetDatabase.LoadAssetAtPath<SerializedScriptableObject>(path);

            if (mainAsset == null)
            {
                if (configDataType.IsAssignableFrom(info.type))
                {
                    var creatorType = typeof(EntityCreator);
                    var asset = SerializedScriptableObject.CreateInstance(creatorType) as EntityCreator;
                    asset.type = info.type;
                    asset.name = info.dpName;
                    asset.datas = new List<ConfigData>();
                    AssetDatabase.CreateAsset(asset, path);
                    needSave = true;
                }
                else
                {
                    var asset = SerializedScriptableObject.CreateInstance(info.type) as SerializedScriptableObject;
                    asset.name = info.dpName;
                    AssetDatabase.CreateAsset(asset, path);
                }
                // 添加 Addressables 标签
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    AddressableAssetGroup group = settings.DefaultGroup;
                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                    entry.labels.Add("ConfigData");
                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                }
            }
        }
        if (needSave)
        {
            AssetDatabase.SaveAssets();
        }
#endif
    }

    /// <summary>
    /// 从指定路径加载数据，并填充数据字典和创建器列表。
    /// </summary>
    /// <param name="forceReload">如果设置为<c>true</c>，则强制重新加载数据。</param>
    public void LoadData(bool forceReload)
    {
#if UNITY_EDITOR
        if (dataDict == null || forceReload)
        {
            bool dirty = false;
            dataDict = new Dictionary<Type, List<ConfigData>>();
            creatorList = new List<EntityCreator>();
            var configDataType = typeof(ConfigData);
            foreach (var info in configTypeOrder)
            {
                var path = $"Assets/OriginData/Data/{info.dpName}.asset";

                if (configDataType.IsAssignableFrom(info.type))
                {
                    var mainAssets = AssetDatabase.LoadMainAssetAtPath(path) as EntityCreator;
                    if (mainAssets == null)
                    {
                        Debug.LogError("Failed to load asset: " + path);
                        continue;
                    }
                    else
                    {
                        Debug.Log("Load asset: " + path);
                    }
                    creatorList.Add(mainAssets);
                    if (mainAssets.datas.Count == 0)
                    {
                        var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                        dirty = true;
                        //list.Add(mainAsset as ConfigData);
                        foreach (var asset in subAssets)
                        {
                            if (asset is ConfigData)
                            {
                                mainAssets.datas.Add(asset as ConfigData);
                            }
                        }
                    }
                    mainAssets.datas.Sort((a, b) => string.Compare(a.key, b.key, StringComparison.Ordinal));
                    dataDict[info.type] = mainAssets.datas;
                    if (dirty)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
                // else
                // {
                //     var mainAssets = AssetDatabase.LoadMainAssetAtPath(path) as SerializedScriptableObject;
                //     if (info.type == typeof(GlobalDB))
                //     {
                //         globalDB = mainAssets as GlobalDB;
                //     }
                // }
            }
        }
#endif
    }


    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    /// <summary>  运行时 load data，不要在Editor相关的代码中调用。 </summary>
    public async UniTask LoadDataAsync()
    {
        await semaphore.WaitAsync();
        dataDict = new Dictionary<Type, List<ConfigData>>();
        creatorList = new List<EntityCreator>();
        try
        {
            var mainAsset = await Addressables.LoadAssetsAsync<ScriptableObject>("ConfigData", null).Task;
            if (mainAsset == null)
            {
                Debug.Log("Failed to load asset: ConfigData");
                return;
            }
            else
            {
                foreach (var asset in mainAsset)
                {
                    if (asset is EntityCreator)
                    {
                        var creator = asset as EntityCreator;
                        creatorList.Add(creator);
                        dataDict.Add(creator.type, creator.datas);
                    }
                    // else if (asset is GlobalDB)
                    // {
                    //     globalDB = asset as GlobalDB;
                    // }
                }
                Debug.Log("Load OSDatalib success");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("An exception occurred: " + ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public T GetData<T>(string key) where T : ConfigData
    {
        var type = typeof(T);
        // 先找同类型的数据
        if (dataDict.ContainsKey(type))
        {
            var list = dataDict[type];
            var data = list.Find(item => item.key == key);
            if (data != null)
            {
                return data as T;
            }
        }
        // 如果没有找到同类型的数据，尝试找子类数据
        foreach (var item in dataDict)
        {
            if (item.Key.IsSubclassOf(type))
            {
                var list = item.Value;
                return list.Find(item => item.key == key) as T;
            }
        }
        return null;
    }
    public List<T> GetDatas<T>() where T : ConfigData
    {
        var type = typeof(T);
        if (dataDict.ContainsKey(type) == true)
        {
            return dataDict[type].ConvertAll(item => item as T);
        }
        return null;
    }

    public void AddData(ConfigData data)
    {
        var type = data.GetType();
        if (dataDict.ContainsKey(type) == false)
        {
            var list = new List<ConfigData>();
            list.Add(data);
            dataDict.Add(type, list);
        }
        else
        {
            var list = dataDict[type];
            list.Add(data);
            list.Sort((a, b) => a.key.CompareTo(b.key));
        }
        var mainAsset = creatorList.Find(item => item.type == type);
        AssetDatabase.AddObjectToAsset(data, mainAsset);
        AssetDatabase.SaveAssetIfDirty(mainAsset);
    }

    public void RemoveData(ConfigData data)
    {
        if (dataDict.ContainsKey(data.GetType()) == true)
        {
            var type = data.GetType();
            var values = dataDict[type];
            values.Remove(data);
            values.Sort((a, b) => a.key.CompareTo(b.key));
            var mainAsset = creatorList.Find(item => item.type == type);
            AssetDatabase.RemoveObjectFromAsset(data);
            EditorUtility.SetDirty(mainAsset);
            AssetDatabase.SaveAssetIfDirty(mainAsset);
            AssetDatabase.Refresh();
            // if(values.Count == 0){
            //     dataDict.Remove(type);
            // }
        }
    }
};

