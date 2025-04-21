using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableView : MonoBehaviour
{
    public Transform content; // 列表的父物体
    public GameObject cellPrefab; // 单元格的预制体

    private readonly List<TableCell> cells = new(); // 存储所有单元格
    private TableCell selectedCell = null; // 当前选中的单元格

    /// <summary>
    /// 初始化 TableView
    /// </summary>
    /// <param name="data">数据列表</param>
    /// <param name="onCellSelected">单元格被选中时的回调</param>
    public void Init<T>(List<T> data, Action<int, T> onCellSelected, int defaultSelectedIndex = 0)
    {
        // 清空旧的单元格
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        cells.Clear();

        // 创建新的单元格
        for (int i = 0; i < data.Count; i++)
        {
            GameObject cellObject = Instantiate(cellPrefab, content);
            TableCell cell = cellObject.GetComponent<TableCell>();
            cell.Init(i, data[i], OnCellClicked);
            cells.Add(cell);
        }

        // 默认选中一个cell
        if (cells.Count > defaultSelectedIndex)
        {
            OnCellClicked(cells[defaultSelectedIndex]);
        }

        // 单元格点击回调
        void OnCellClicked(TableCell clickedCell)
        {
            if (selectedCell == clickedCell)
            {
                return;
            }
            // 如果有选中的单元格，取消选中
            if (selectedCell != null)
            {
                selectedCell.SetSelected(false);
            }

            // 设置当前单元格为选中状态
            selectedCell = clickedCell;
            selectedCell.SetSelected(true);

            // 调用外部回调
            onCellSelected?.Invoke(clickedCell.Index, (T)clickedCell.Data);
        }
    }
}
