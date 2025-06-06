using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int maxJumpCount = 2;  // 最大跳跃次数
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;  // 地面检测距离
    [SerializeField] private int groundCheckRays = 3;  // 地面检测射线数量

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator animator;
    private float moveInput;
    private bool facingRight = true;
    private PlayerUpgrade playerUpgrade;
    
    [Header("状态")]
    public bool isOnGround;
    private int jumpCount;  // 当前跳跃次数
    private bool wasOnGround;  // 上一帧是否在地面上

    // 动画参数名称
    private static readonly string IS_RUNNING = "isRunning";
    private static readonly string IS_JUMPING = "isJumping";
    private static readonly string IS_FALLING = "isFalling";
    private static readonly string IS_WALL_SLIDING = "isWallSliding";
    private static readonly string IS_DASHING = "isDashing";

    private Vector3 lastPosition;
    private bool isMonitoring = true;
    //按键设置
    bool jumpPress;
    

    private void OnValidate()
    {
        if (groundLayer.value == 0)
        {
            Debug.LogError("请设置Ground Layer！在Project Settings中设置Layer 8为Ground，并在此处选择。");
        }

        // 检查碰撞器
        if (coll == null)
        {
            coll = GetComponent<Collider2D>();
            if (coll == null)
            {
                Debug.LogError("Player缺少Collider2D组件！");
            }
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        playerUpgrade = GetComponent<PlayerUpgrade>();

        // 确保初始位置正确
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        // 检查并修正Tilemap位置
        FixTilemapPosition();

        // 初始化跳跃次数
        jumpCount = maxJumpCount;
    }

    private void FixTilemapPosition()
    {
        Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
        foreach (Tilemap tilemap in tilemaps)
        {
            if (((1 << tilemap.gameObject.layer) & groundLayer) != 0)
            {
                // 找到Grid父对象
                Grid grid = tilemap.GetComponentInParent<Grid>();
                if (grid != null)
                {
                    // 修正Grid位置
                    if (grid.transform.position.z != 0)
                    {
                        Debug.LogWarning($"Grid的Z轴位置不为0！正在修正...\n" +
                            $"原位置: {grid.transform.position}");
                        
                        grid.transform.position = new Vector3(
                            grid.transform.position.x,
                            grid.transform.position.y,
                            0
                        );
                    }
                }

                // 检查Tilemap的碰撞器设置
                TilemapCollider2D tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
                if (tilemapCollider == null)
                {
                    Debug.LogError($"Tilemap '{tilemap.name}' 缺少TilemapCollider2D！");
                    continue;
                }
            }
        }
    }

    private void Update()
    {
        // 获取水平输入
        moveInput = Input.GetAxisRaw("Horizontal");

        //跳跃
        if (Input.GetKeyDown(KeyCode.W))
        {
            //在地面上跳跃
            if (isOnGround)
            {
                Jump();
            }
            //在空中跳跃
            else if (jumpCount > 0)
            {
                Jump();
            }
        }

        UpdateMovement();
    }

    private void UpdateMovement()
    {
        // 移动
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 翻转角色
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }

        // 更新动画
        UpdateAnimations();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            // 更新奔跑动画
            animator.SetBool(IS_RUNNING, Mathf.Abs(moveInput) > 0.1f);
            
            // 更新跳跃/下落动画
            animator.SetBool(IS_JUMPING, rb.velocity.y > 0.1f);
            animator.SetBool(IS_FALLING, rb.velocity.y < -0.1f);
            

            // 更新墙壁滑行状态（如果有PlayerUpgrade组件）
            if (playerUpgrade != null)
            {
                animator.SetBool(IS_WALL_SLIDING, false); // 这里需要从PlayerUpgrade获取实际状态
                animator.SetBool(IS_DASHING, false); // 这里需要从PlayerUpgrade获取实际状态
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 播放着地音效
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !isOnGround)
        {
            AudioManager.Instance.Play("Land");
        }
    }

    private void LateUpdate()
    {
        // 在所有更新之后强制设置Z轴为0
        if (transform.position.z != 0)
        {
            isMonitoring = false; // 暂时禁用监控以避免循环日志
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            lastPosition = transform.position;
            isMonitoring = true;
        }
    }
    
    private void FixedUpdate()
    {
        // 检测是否在地面上
        isOnGroundCheck();
    }
    
    void isOnGroundCheck()
    {
        wasOnGround = isOnGround;  // 保存上一帧的地面状态

        if (coll == null)
        {
            Debug.LogError("Collider is null!");
            return;
        }

        // 获取碰撞器的底部中心点和宽度
        Bounds bounds = coll.bounds;
        float colliderWidth = bounds.size.x;
        float colliderHeight = bounds.size.y;
        
        // 计算射线起点（从碰撞器底部中心开始）
        Vector2 colliderBottom = new Vector2(
            bounds.center.x,
            bounds.min.y
        );

        // 计算射线起点
        float raySpacing = colliderWidth / (groundCheckRays - 1);
        bool hitGround = false;

        // 发射多条射线
        for (int i = 0; i < groundCheckRays; i++)
        {
            Vector2 rayStart = new Vector2(
                colliderBottom.x - colliderWidth/2 + (raySpacing * i),
                colliderBottom.y
            );

            // 添加调试信息
            if (showDebugInfo)
            {
                Debug.Log($"Ray {i} start position: {rayStart}, Ground Layer: {groundLayer.value}");
            }

            RaycastHit2D hit = Physics2D.Raycast(
                rayStart,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            if (hit.collider != null)
            {
                hitGround = true;
                if (showDebugInfo)
                {
                    Debug.Log($"Ray {i} hit: {hit.collider.gameObject.name} at distance {hit.distance}");
                }
                break;
            }

            // 调试信息
            if (showDebugInfo)
            {
                Debug.DrawRay(
                    rayStart,
                    Vector2.down * groundCheckDistance,
                    hit.collider != null ? Color.green : Color.red,
                    0.1f
                );
            }
        }

        // 更新地面状态
        isOnGround = hitGround;

        // 如果刚落地，重置跳跃次数
        if (isOnGround && !wasOnGround)
        {
            jumpCount = maxJumpCount;
            //Debug.Log("Landed! Jump count reset to: " + jumpCount);
        }

        // 调试信息
        if (showDebugInfo)
        {
            Debug.Log($"Ground Check - isOnGround: {isOnGround}, Jump Count: {jumpCount}, Position: {transform.position}, Bounds: {bounds}");
        }
    }

    private void Jump()
    {
        if (jumpCount > 0)
        {
            GetComponent<Animator>().Play("Wizard_Jump");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount--;
            //Debug.Log($"Jump! Remaining jumps: {jumpCount}");
        }
    }
} 
