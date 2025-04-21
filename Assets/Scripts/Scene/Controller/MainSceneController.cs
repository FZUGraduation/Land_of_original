using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class MainSceneController : SceneController
{
    void Start()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIMainScene);
    }
}
