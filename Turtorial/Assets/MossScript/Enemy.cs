using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;
    
    [Header("Detection")]
    [SerializeField] private float playerDetectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;

    private Vector3 startPosition;
    private bool movingRight = true;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 检测玩家
        Collider2D player = Physics2D.OverlapCircle(transform.position, playerDetectionRange, playerLayer);
        
        if (player != null)
        {
            // 玩家在检测范围内，追击玩家
            ChasePlayer(player.transform);
        }
        else
        {
            // 没有检测到玩家，执行巡逻
            Patrol();
        }
    }

    private void Patrol()
    {
        if (movingRight)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            if (transform.position.x >= startPosition.x + patrolDistance)
            {
                movingRight = false;
                FlipSprite();
            }
        }
        else
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
            if (transform.position.x <= startPosition.x - patrolDistance)
            {
                movingRight = true;
                FlipSprite();
            }
        }
    }

    private void ChasePlayer(Transform player)
    {
        float direction = player.position.x - transform.position.x;
        
        // 根据玩家位置调整朝向
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight))
        {
            movingRight = !movingRight;
            FlipSprite();
        }

        // 向玩家方向移动
        rb.velocity = new Vector2(Mathf.Sign(direction) * moveSpeed * 1.5f, rb.velocity.y);
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 获取碰撞点的方向
            Vector2 hitDirection = collision.contacts[0].normal;
            
            // 如果玩家从上方踩到敌人
            if (hitDirection.y < -0.5f)
            {
                // 销毁敌人
                Destroy(gameObject);
                
                // 给玩家一个向上的力（弹跳效果）
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);
                }
            }
            else
            {
                // 如果是侧面碰撞，玩家受伤
                GameManager.Instance.LoseLife();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 绘制巡逻范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startPosition, new Vector3(patrolDistance * 2, 1, 0));
        
        // 绘制玩家检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
    }
} 