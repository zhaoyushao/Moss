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
    
    [SerializeField] private LayerMask groundLayer;

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
    int jumpCount = 1;  // 设置初始跳跃次数

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
        jumpCount = 1;
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
        if (Input.GetKeyDown(KeyCode.W))  // 移除jumpCount > 0检查，因为在地面检测中处理
        {
            jumpPress = true;
            
            //在地面上跳跃
            if (jumpPress && isOnGround)
            {
                GetComponent<Animator>().Play("Wizard_Jump");
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount--;
                jumpPress = false;
            }
            //在空中跳跃
            else if (jumpPress && jumpCount > 0 && !isOnGround)
            {
                GetComponent<Animator>().Play("Wizard_Jump");
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount--;
                jumpPress = false;
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
    
    void FixedUpdate()
    {
        isOnGroundCheck();
    }
    
    void isOnGroundCheck()
    {
        //判断角色碰撞器与地面图层发生接触
        if (coll.IsTouchingLayers(groundLayer))
        {
            isOnGround = true;
            jumpCount = 1;  // 在地面上时重置跳跃次数
        }
        else
        {
            isOnGround = false;
        }
    }
} 
