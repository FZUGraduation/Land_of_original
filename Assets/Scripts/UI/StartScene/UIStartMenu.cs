using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStartMenu : BaseDialog
{
    public Button startBtn;
    protected override void Awake()
    {
        base.Awake();
        startBtn.onClick.AddListener(OnStartBtnClick);
    }

    private void OnStartBtnClick()
    {
        AudioManager.Instance.PlaySE("013_Confirm_03");
        _ = WindowManager.Instance.ShowDialogAsync(UIDefine.UISlotSelect);
    }
}
