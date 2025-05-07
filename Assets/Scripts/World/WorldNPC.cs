
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;



public class WorldNPC : MonoBehaviour
{
    public GameObject talkSign; // 交互提示物体
    public float talkRadius = 1f; // 交互范围半径
    public string talkKey; // 对话ID
    public string talkKey2; // 对话ID2
    // private EnemyState currentState = EnemyState.RandomMove; // 当前状态
    private GameObject player; // 玩家对象
    private bool canTalk = false; // 是否可以交互
    private InputAction interactAction; // 定义一个 InputAction
    void Awake()
    {
        FrameEvent.Instance.On(FrameEvent.CreateWorldPlayer, OnCreateWorldPlayer, this);
        // 初始化 InputAction，绑定到 "E" 键
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.Enable(); // 启用 InputAction
    }

    void Start()
    {
        // 获取当前物体的初始 Y 位置
        float startY = talkSign.transform.position.y;
        // 使用 DoTween 实现上下浮动
        talkSign.transform.DOLocalMoveY(startY + 0.5f, 1f)
            .SetEase(Ease.InOutSine) // 设置缓动效果
            .SetLoops(-1, LoopType.Yoyo); // 无限循环，Yoyo 模式（往返）
        talkSign.SetActive(false);
    }

    void Update()
    {
        // 检查与玩家的距离
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (player && !canTalk && distance < talkRadius)
        {
            canTalk = true; // 可以交互
            interactAction.Enable(); // 启用交互
            talkSign.SetActive(true); // 显示交互提示
            WindowManager.Instance.ShowTost("按E键交互"); // 显示提示信息
        }
        else if (canTalk && distance >= talkRadius)
        {
            canTalk = false; // 不能交互
            talkSign.SetActive(false); // 隐藏交互提示
        }
        if (canTalk && interactAction.WasPressedThisFrame())
        {
            // 触发交互事件
            string key = SaveSlotData.Instance.talkedKey.Contains(talkKey) ? talkKey2 : talkKey;
            if (!SaveSlotData.Instance.talkedKey.Contains(key))
            {
                SaveSlotData.Instance.talkedKey.Add(key);
            }
            WindowManager.Instance.ShowDialog(UIDefine.UITalk, UIIndex.STACK, key);
            talkSign.SetActive(false); // 隐藏交互提示
            interactAction.Disable();
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
