

using UnityEngine;
using Sirenix.OdinInspector;

public class StartSceneController : SceneController
{
    void Start()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIStartMenu);
    }

    [Button]
    public void ShowConfirmBox()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIConfirmBox, UIIndex.STACK, "确认要退出游戏吗？", new System.Action(() =>
        {
            Debug.Log("OnClickShowConfirmBox");
        }));
    }
}
