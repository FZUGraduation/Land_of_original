using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    static public GameManager instance = null;
    public SceneController sceneController = null;
    private int currSaveSlotIndex = 0;

    public string SaveFilePath
    {
        get; private set;
    }
    public string SaveSlotPath
    {
        get; private set;
    }

    // Start is called before the first frame update
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InstantiateGameManager()
    {
        if (!IsInitialized)
        {
            var go = Resources.Load<GameObject>("Prefabs/GameManager");
            if (go != null)
            {
                Instantiate(go);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            // Other settings...
        };
        SaveSlotPath = $"{Application.persistentDataPath}/saveslots/";
        transform.gameObject.name = "GlobalBehavior";
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        // 确保保存目录存在
        if (!Directory.Exists(SaveSlotPath))
        {
            Directory.CreateDirectory(SaveSlotPath);
        }

        _ = Init();
        Debug.Log("Game Manager Instantiated");
    }

    public async UniTask Init()
    {
        await Datalib.Instance.LoadDataAsync();
    }

    public bool ToJsonAndSave(string path, object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
            Debug.Log("SaveSlot Saved");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public T FromJsonAndLoad<T>(string path) where T : class
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var deserializedData = JsonConvert.DeserializeObject<T>(json);
                T tmpObj = deserializedData;
                Debug.Log("Loaded" + path);
                return tmpObj;
            }
            else
            {
                File.WriteAllText(path, "");
                Debug.Log($"File not found {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    public SaveSlotData GetSlotData(int index)
    {
        SaveFilePath = $"{SaveSlotPath}saveslot_{index}.txt";
        if (File.Exists(SaveFilePath))
        {
            var saveData = FromJsonAndLoad<SaveSlotData>(SaveFilePath);
            return saveData;
        }
        else
        {
            return null;
        }
    }

    public void DeleteSaveSlot(int index)
    {
        SaveFilePath = $"{SaveSlotPath}saveslot_{index}.txt";
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log($"Delete SaveSlot_{index}");
        }
    }

    public void LoadSaveSlotData(int index)
    {
        SaveFilePath = $"{SaveSlotPath}saveslot_{index}.txt";
        if (File.Exists(SaveFilePath))
        {
            var saveData = FromJsonAndLoad<SaveSlotData>(SaveFilePath);
            if (saveData != null)
            {
                global::SaveSlotData.ReplaceInstance(saveData);
                Debug.Log($"SaveSlot_{index} Loaded");
            }
        }
        else
        {
            var saveData = new SaveSlotData();
            ToJsonAndSave(SaveFilePath, saveData);
            global::SaveSlotData.ReplaceInstance(saveData);
            Debug.Log("Create new SaveSlot");
        }
        currSaveSlotIndex = index;
    }
    public void SaveSlotData()
    {
        SaveFilePath = $"{SaveSlotPath}saveslot_{currSaveSlotIndex}.txt";
        global::SaveSlotData.Instance.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        ToJsonAndSave(SaveFilePath, global::SaveSlotData.Instance);
    }

    public void GoToBattle(string level = "Level1")
    {
        BattleData.Init(level);
        SceneLoader.Instance.LoadScene(SceneLoader.battleScene);
    }
}
