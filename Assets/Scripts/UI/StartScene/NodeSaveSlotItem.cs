
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeSaveSlotItem : MonoBehaviour
{
    public Text slotName;
    public Text slotInfo;
    public Image slotIcon;
    public Button startButton;
    public Button deleteButton;
    private int index;
    Action<int> onSelect;
    void Awake()
    {
        startButton.onClick.AddListener(OnStartClick);
        deleteButton.onClick.AddListener(OnDeleteClick);
    }
    public void Init(int i, Action<int> onSelect)
    {
        OnInit(i);
        this.onSelect = onSelect;
    }

    private void OnInit(int i)
    {
        index = i;
        slotName.text = "存档 " + i;
        var data = GameManager.Instance.GetSlotData(i);
        if (data != null)
        {
            slotInfo.text = "存档时间：" + data.saveTime;
        }
        else
        {
            slotInfo.text = "空存档";
        }
    }

    private void OnStartClick()
    {
        onSelect?.Invoke(index);
    }

    private void OnDeleteClick()
    {
        WindowManager.Instance.ShowDialog(UIDefine.UIConfirmBox, UIIndex.STACK, "确认要删除这个存档么", new Action(() =>
        {
            GameManager.Instance.DeleteSaveSlot(index);
            OnInit(index);
        }));
    }
}
