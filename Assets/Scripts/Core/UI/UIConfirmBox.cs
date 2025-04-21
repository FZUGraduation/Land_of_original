using System;
using TMPro;
using UnityEngine.UI;

// public enum ConfirmType
// {
//     Confirm,
//     Cancel,
// }
public class UIConfirmBox : BaseDialog
{
    public TextMeshProUGUI desc;
    public Button confirmBtn;
    public Button cancelBtn;
    public override void Init(params object[] data)
    {
        desc.text = (string)data[0];
        Action confirmAction = data[1] as Action;
        confirmBtn.onClick.AddListener(() =>
        {
            confirmAction?.Invoke();
            Close();
        });
        // Action cancelAction = data[2] as Action;
        cancelBtn.onClick.AddListener(() =>
        {
            // cancelAction?.Invoke();
            Close();
        });
    }
}
