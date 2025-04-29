using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEnemy : MonoBehaviour
{
    public float patrolRadius = 5f; // 巡逻范围半径
    public float moveSpeed = 2f; // 移动速度
    public float minIdleTime = 1f; // 最小停顿时间
    public float maxIdleTime = 3f; // 最大停顿时间
    public string levelName = "Level1"; // 关卡名称
    private Vector3 startPosition; // 初始位置
    private Vector3 targetPosition; // 当前目标位置
    private bool isMoving = false; // 是否正在移动
    private float idleTimer = 0f; // 停顿计时器
    private Animator animator; // 动画控制器

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        SetNewTargetPosition();
    }

    void Update()
    {
        RandomMove();
    }

    private void RandomMove()
    {
        if (isMoving)
        {
            // 移动到目标位置
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 调整朝向
            Vector3 direction = targetPosition - transform.position;
            if (direction.magnitude > 0.1f) // 避免零向量
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 平滑旋转
            }

            // 检查是否到达目标位置
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                animator?.Play("Idle"); // 播放待机动画
                isMoving = false;
                idleTimer = Random.Range(minIdleTime, maxIdleTime); // 随机停顿时间
            }
        }
        else
        {
            // 停顿计时
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                SetNewTargetPosition();
                animator?.Play("Walk"); // 播放行走动画
                isMoving = true;
            }
        }
    }

    private void SetNewTargetPosition()
    {
        // 在巡逻范围内随机生成一个目标位置
        Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
        targetPosition = startPosition + new Vector3(randomPoint.x, 0, randomPoint.y);
    }

    private void OnDrawGizmosSelected()
    {
        // 在 Scene 视图中绘制巡逻范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleData.Init(levelName);
            SceneLoader.Instance.LoadScene(SceneLoader.battleScene);
        }
    }
}