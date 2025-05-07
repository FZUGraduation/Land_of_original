using Sirenix.OdinInspector;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    protected virtual void Awake()
    {
        GameManager.Instance.sceneController = this;
    }

    [Button]
    public void SaveData()
    {
        Debug.Log("SaveSlotData");
        GameManager.Instance.SaveSlotData();
    }

    [Button]
    public void AddHero(string key = "无名守卫")
    {
        SaveSlotData.Instance.AddHero(key);
    }

    [Button]
    public void ShowTalk(string talkKey = "TestTalk1")
    {
        WindowManager.Instance.ShowDialog(UIDefine.UITalk, UIIndex.STACK, talkKey);
    }

    [Button]
    public void AddItem(string itemKey = "Item1", int amount = 1)
    {
        SaveSlotData.Instance.bagData.AddItem(Datalib.Instance.GetData<ItemConfigData>(itemKey), amount);
    }

    [Button]
    public void InitRuntimeSlot()
    {
        var saveRuntime = new SaveSlotData();
        saveRuntime.bagData.AddItem("Item1", 1);
        SaveSlotData.ReplaceInstance(saveRuntime);
    }
}
