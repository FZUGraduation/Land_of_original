

using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System;

public class StartSceneController : SceneController
{
    private InputAction interactAction; // 定义一个 InputAction
    void Start()
    {
        // 初始化 InputAction，绑定到 "ESC" 键
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        interactAction.Enable(); // 启用 InputAction
        WindowManager.Instance.ShowDialog(UIDefine.UIStartMenu);
    }

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            WindowManager.Instance.ShowDialog(UIDefine.UIConfirmBox, UIIndex.STACK, "是否要退出游戏", new Action(() =>
            {
                Application.Quit();
            }));
        }
    }

    [Button]
    public void ShowConfirmBox()
    {
        WindowManager.Instance.ShowTost("111");
    }
}
