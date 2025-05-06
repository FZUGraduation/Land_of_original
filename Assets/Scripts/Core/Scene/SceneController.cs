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
    public void GoToBattle(string level = "Level1")
    {
        var saveRuntime = new SaveSlotData();
        saveRuntime.AddHero("Hero1");
        saveRuntime.AddHero("Hero2");
        SaveSlotData.ReplaceInstance(saveRuntime);

        BattleData.Init(level);
        SceneLoader.Instance.LoadScene(SceneLoader.battleScene);
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
