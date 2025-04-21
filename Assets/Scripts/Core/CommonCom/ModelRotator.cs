
using UnityEngine;
using UnityEngine.EventSystems;

public class ModelRotator : MonoBehaviour, IDragHandler
{
    public Transform targetModel;
    public float rotationSpeed = 10f;
    // private Vector3 lastMousePosition;

    // /// <summary>
    // /// 当鼠标点击时触发
    // /// </summary>
    // /// <param name="eventData">点击事件数据</param>
    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     Debug.Log("Model clicked!");
    //     // 这里可以添加点击后的逻辑，比如高亮显示模型或其他交互
    // }

    /// <summary>
    /// 当鼠标拖动时触发
    /// </summary>
    /// <param name="eventData">拖动事件数据</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (targetModel == null) return;

        // 获取鼠标移动的距离
        Vector3 delta = eventData.delta;

        // 根据鼠标移动的距离计算旋转角度
        // float rotationX = delta.y * rotationSpeed * Time.deltaTime;
        float rotationY = -delta.x * rotationSpeed * Time.deltaTime;

        // 旋转模型
        targetModel.Rotate(Vector3.up, rotationY, Space.World); // 绕世界坐标的 Y 轴旋转
        // targetModel.Rotate(Vector3.right, rotationX, Space.Self); // 绕自身的 X 轴旋转
    }
}
