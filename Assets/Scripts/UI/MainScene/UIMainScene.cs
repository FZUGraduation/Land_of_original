using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainScene : BaseDialog
{
    public Button talentButton;
    public Button bagButton;
    public Button characterButton;
    protected override void Awake()
    {
        base.Awake();
        talentButton.onClick.AddListener(OnTalentBtn);
        characterButton.onClick.AddListener(OnCharacterBtn);
    }

    private void OnTalentBtn()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UITalent);
    }

    private void OnCharacterBtn()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UICharacterShow);
    }
}
