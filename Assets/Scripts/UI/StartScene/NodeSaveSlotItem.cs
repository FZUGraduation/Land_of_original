
using System;
using TMPro;
using UnityEngine;

public class NodeSaveSlotItem : MonoBehaviour
{
    public TextMeshProUGUI slotName;
    private int index;
    Action<int> onSelect;
    void Awake()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);
    }
    public void Init(int i, Action<int> onSelect)
    {
        index = i;
        slotName.text = "存档 " + i;
        this.onSelect = onSelect;
    }
    private void OnClick()
    {
        onSelect?.Invoke(index);
    }
}
