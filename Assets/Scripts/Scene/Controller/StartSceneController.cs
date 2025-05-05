

using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;

public class StartSceneController : SceneController
{
    void Start()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIStartMenu);
    }

    [Button]
    public void ShowConfirmBox()
    {
        WindowManager.Instance.ShowTost("111");
    }
}
