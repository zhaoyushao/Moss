using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Following")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -1f);
    
    [Header("Boundaries")]
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;
    
    [Header("Look Ahead")]
    [SerializeField] private float lookAheadFactor = 3f;
    [SerializeField] private float lookAheadReturnSpeed = 0.5f;
    [SerializeField] private float lookAheadMoveThreshold = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private float currentLookAheadX = 0f;
    private float targetLookAheadX = 0f;
    private float lastTargetX = 0f;
    private bool lookDirectionChanged = false;
    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        if (target != null)
        {
            lastTargetX = target.position.x;
            // 初始化时保持Y和Z轴不变
            Vector3 initialPosition = GetDesiredPosition();
            initialPosition.y = transform.position.y;
            initialPosition.z = transform.position.z;
            transform.position = initialPosition;
            if (showDebugInfo)
            {
                Debug.Log($"相机初始化:\n" +
                    $"目标: {target.name}\n" +
                    $"目标位置: {target.position}\n" +
                    $"相机位置: {transform.position}");
            }
        }
        else
        {
            Debug.LogError("未设置跟随目标！请将Player拖拽到CameraFollow组件的Target字段。");
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogError("相机跟随目标丢失！");
            return;
        }

        // 记录移动前的位置
        Vector3 previousPosition = transform.position;
        float fixedY = transform.position.y; // 保存当前Y轴位置

        // 计算前瞻
        float moveDirection = Mathf.Sign(target.position.x - lastTargetX);
        bool hasExceededThreshold = Mathf.Abs(target.position.x - lastTargetX) > lookAheadMoveThreshold;

        if (hasExceededThreshold)
        {
            lookDirectionChanged = moveDirection != Mathf.Sign(targetLookAheadX);
            targetLookAheadX = lookAheadFactor * moveDirection;
        }

        if (lookDirectionChanged)
        {
            currentLookAheadX = 0f;
            lookDirectionChanged = false;
        }

        currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX,
            Time.deltaTime * lookAheadReturnSpeed);

        Vector3 desiredPosition = GetDesiredPosition();
        desiredPosition.y = fixedY; // 强制使用固定的Y轴位置
        
        // 使用 SmoothDamp 实现更平滑的相机移动
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition,
            ref currentVelocity, smoothSpeed);
        smoothedPosition.y = fixedY; // 确保平滑移动后Y轴也保持不变

        transform.position = smoothedPosition;
        
        // 检查目标是否移动
        bool targetMoved = (target.position - new Vector3(lastTargetX, target.position.y, target.position.z)).magnitude > 0.01f;
        
        lastTargetX = target.position.x;
    }

    private Vector3 GetDesiredPosition()
    {
        if (target == null)
            return transform.position;

        // 保持当前Y和Z轴位置
        float fixedY = transform.position.y;
        float fixedZ = transform.position.z;

        // 基础位置（只使用X坐标）
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            fixedY,
            fixedZ
        );
        
        // 添加前瞻（只影响X轴）
        desiredPosition.x += currentLookAheadX;
        
        // 限制在边界内（只限制X轴）
        if (minPosition != maxPosition)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minPosition.x, maxPosition.x);
        }
        
        return desiredPosition;
    }

    // 立即更新相机位置到目标位置
    public void UpdateCameraPositionImmediately()
    {
        if (target != null)
        {
            transform.position = GetDesiredPosition();
        }
    }

    private void OnDrawGizmos()
    {
        // 在Scene视图中显示相机边界
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((minPosition.x + maxPosition.x) / 2, (minPosition.y + maxPosition.y) / 2, 0),
            new Vector3(maxPosition.x - minPosition.x, maxPosition.y - minPosition.y, 0)
        );
    }

    // 设置新的目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetX = target.position.x;
            UpdateCameraPositionImmediately();
        }
    }

    // 设置新的边界
    public void SetBoundaries(Vector2 min, Vector2 max)
    {
        minPosition = min;
        maxPosition = max;
    }
} 