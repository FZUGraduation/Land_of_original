using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TableCell : MonoBehaviour
{
    [Tooltip("是否可以点击")]
    public bool canClick = true; // 是否可以点击
    [ShowIf("canClick")]
    public Button cellButton; // 单元格的按钮

    public int Index { get; private set; } // 单元格的索引
    public object Data { get; private set; } // 单元格的数据

    private Action<TableCell> onClickCallback; // 点击回调

    /// <summary>
    /// 初始化单元格
    /// </summary>
    /// <param name="index">单元格索引</param>
    /// <param name="data">单元格数据</param>
    /// <param name="onClickCallback">点击回调</param>
    public void Init(int index, object data, Action<TableCell> onClickCallback)
    {
        Index = index;
        Data = data;
        this.onClickCallback = onClickCallback;
        // 绑定点击事件
        if (cellButton)
        {
            cellButton.onClick.RemoveAllListeners();
            cellButton.onClick.AddListener(OnClick);
        }
        OnInit();
    }

    public virtual void OnInit() { }
    /// <summary>
    /// 设置单元格的选中状态
    /// </summary>
    /// <param name="isSelected">是否选中</param>
    public virtual void SetSelected(bool isSelected) { }
    private void OnClick()
    {
        onClickCallback?.Invoke(this);
    }
}
