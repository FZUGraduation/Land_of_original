
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldTreasure : MonoBehaviour
{
    public string treasureKey; // 宝箱ID
    public float talkRadius = 1f; // 交互范围半径
    public GameObject talkSign; // 交互提示物体
    private Animator animator;
    private bool isOpen = false;
    private bool canOpen = false; // 是否可以交互
    private GameObject player; // 玩家对象
    private InputAction interactAction; // 定义一个 InputAction

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (SaveSlotData.Instance.unlockTreasure.ContainsKey(treasureKey) && SaveSlotData.Instance.unlockTreasure[treasureKey])
        {
            // animator.Play("Open"); // 播放开宝箱动画
            // isOpen = true; // 宝箱已打开
            Destroy(gameObject); // 销毁宝箱对象
            return;
        }
        talkSign.SetActive(false); // 隐藏交互提示
        FrameEvent.Instance.On(FrameEvent.CreateWorldPlayer, OnCreateWorldPlayer, this);
        // 初始化 InputAction，绑定到 "E" 键
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.Enable(); // 启用 InputAction
    }

    void Update()
    {
        if (isOpen) return; // 如果宝箱已打开，则不进行交互检查

        // 检查与玩家的距离
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (player && !canOpen && distance < talkRadius)
        {
            canOpen = true; // 可以交互
            interactAction.Enable(); // 启用交互
            talkSign.SetActive(true); // 显示交互提示
        }
        else if (canOpen && distance >= talkRadius)
        {
            canOpen = false; // 不能交互
            talkSign.SetActive(false); // 隐藏交互提示
        }

        if (canOpen && interactAction.WasPressedThisFrame())
        {
            // 触发交互事件
            // animator.Play("Open"); // 播放开宝箱动画
            // talkSign.SetActive(false); // 隐藏交互提示
            // interactAction.Disable();
            isOpen = true; // 宝箱已打开
            SaveSlotData.Instance.unlockTreasure.Add(treasureKey, true); // 更新宝箱状态
            var itemCosts = Datalib.Instance.GetData<TreasureConfigData>(treasureKey).itemCosts;
            WindowManager.Instance.ShowDialog(UIDefine.UIGetItem, UIIndex.STACK, itemCosts);
            Destroy(gameObject); // 销毁宝箱对象
        }
    }

    private void OnCreateWorldPlayer(object[] args)
    {
        player = args[0] as GameObject;
        if (player == null)
        {
            Debug.LogError("WorldEnemy: Player object is null.");
            return;
        }
    }
}
