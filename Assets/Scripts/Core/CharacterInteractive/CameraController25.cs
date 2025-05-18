using UnityEngine;

public class CameraController25 : MonoBehaviour
{
    [HideInInspector]
    public CameraController25 Instance;
    public Transform target; // 主角的 Transform
    public Vector3 offset = new(0, 5, -10); // 相机相对于主角的偏移量
    public float smoothSpeed = 0.125f; // 平滑跟随的速度
    public Vector4 cameraBounds = new(0, 0, 0, 0); // 相机xz边界
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("CameraFollow: Multiple instances of CameraFollow detected in the scene. Only one CameraFollow can exist at a time. The duplicate CameraFollow will be destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        FrameEvent.Instance.On(FrameEvent.CreateWorldPlayer, OnCreateWorldPlayer, this);
    }
    void Start()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            FrameEvent.Instance.OffAll(this);
        }
    }
    private void LateUpdate()
    {
        if (target == null)
        {
            // Debug.LogWarning("CameraFollow: No target set for the camera to follow.");
            return;
        }

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 平滑地移动相机到目标位置
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 限制相机在指定的边界内
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, cameraBounds.x, cameraBounds.y);
        smoothedPosition.z = Mathf.Clamp(smoothedPosition.z, cameraBounds.z, cameraBounds.w);

        // 设置相机的位置
        transform.position = smoothedPosition;

        // 保持相机朝向主角
        // transform.LookAt(target);
    }
    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void OnCreateWorldPlayer(object[] args)
    {
        var player = args[0] as GameObject;
        if (player == null)
        {
            Debug.LogError("WorldEnemy: Player object is null.");
            return;
        }
        target = player.transform;
        transform.position = target.position + offset;
        transform.LookAt(target);
    }
}
