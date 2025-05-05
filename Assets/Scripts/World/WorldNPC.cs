using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WorldNPC : MonoBehaviour
{
    public GameObject talkSign; // 交互提示物体
    public float talkRadius = 2f; // 交互范围半径
    private EnemyState currentState = EnemyState.RandomMove; // 当前状态
    private GameObject player; // 玩家对象
    private bool canTalk = false; // 是否可以交互
    void Awake()
    {
        FrameEvent.Instance.On(FrameEvent.CreateWorldPlayer, OnCreateWorldPlayer, this);
    }

    void Update()
    {
        // 检查与玩家的距离
        if (player && Vector3.Distance(transform.position, player.transform.position) < talkRadius)
        {
            canTalk = true; // 可以交互
            talkSign.SetActive(true); // 显示交互提示
        }
        else
        {
            canTalk = false; // 不能交互
            talkSign.SetActive(false); // 隐藏交互提示
        }
        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
            // 触发交互事件
            // WindowManager.Instance.ShowDialog(UIDefine.UITalk, UIIndex.STACK, talkKey);
            // InteractWithPlayer();
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
