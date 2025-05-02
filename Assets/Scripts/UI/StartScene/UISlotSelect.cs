
using UnityEngine;

public class UISlotSelect : BaseDialog
{
    public Transform slotRoot;

    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < 3; i++)
        {
            var slot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/StartScene/NodeSaveSlotItem"), slotRoot);
            slot.name = $"SlotItem_{i}";
            slot.GetComponent<NodeSaveSlotItem>().Init(i, OnSlotSelect);
        }
    }

    private void OnSlotSelect(int index)
    {
        Debug.Log("选择存档：" + index + 1);
        GameManager.Instance.LoadSaveSlotData(index);
        if (SaveSlotData.Instance.CheckStoryProgress(StoryProgress.heroSelect) == false)
        {
            _ = WindowManager.Instance.ShowDialogAsync(UIDefine.UIStartHeroSelect);
        }
        else
        {
            SceneLoader.Instance.LoadScene(SceneLoader.mainScene);
        }
        Close();
    }
}
