using UnityEngine;
using System.Collections.Generic;

public class PlayerUpgrade : MonoBehaviour
{
    [System.Serializable]
    public class Ability
    {
        public string name;
        public bool isUnlocked;
        public int cost;
        public Sprite icon;
    }

    [Header("Abilities")]
    public Ability doubleJump;
    public Ability dashAbility;
    public Ability wallJump;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Double Jump Settings")]
    public float doubleJumpForce = 10f;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 12f;
    public float wallSlideSpeed = 2f;
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private bool canDoubleJump = false;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private bool isWallSliding = false;
    private int facingDirection = 1;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDashing)
        {
            HandleDash();
            return;
        }

        // 处理冷却时间
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }

        // 检查墙壁滑行
        CheckWallSliding();

        // 处理能力输入
        HandleAbilityInputs();
    }

    private void HandleAbilityInputs()
    {
        // 二段跳
        if (doubleJump.isUnlocked && Input.GetButtonDown("Jump") && !playerController.isOnGround && canDoubleJump)
        {
            PerformDoubleJump();
        }

        // 冲刺
        if (dashAbility.isUnlocked && Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartDash();
        }

        // 墙壁跳跃
        if (wallJump.isUnlocked && isWallSliding && Input.GetButtonDown("Jump"))
        {
            PerformWallJump();
        }
    }

    private void PerformDoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        canDoubleJump = false;
        AudioManager.Instance.Play("DoubleJump");
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;
        AudioManager.Instance.Play("Dash");
    }

    private void HandleDash()
    {
        dashTimeLeft -= Time.deltaTime;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);

        if (dashTimeLeft <= 0)
        {
            isDashing = false;
            rb.velocity = Vector2.zero;
        }
    }

    private void CheckWallSliding()
    {
        facingDirection = (int)transform.localScale.x;
        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, 
            wallCheckDistance, wallLayer);

        if (wallCheck && !playerController.isOnGround && rb.velocity.y < 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void PerformWallJump()
    {
        rb.velocity = new Vector2(-facingDirection * wallJumpForce, wallJumpForce);
        isWallSliding = false;
        AudioManager.Instance.Play("WallJump");
    }

    public void UnlockAbility(string abilityName)
    {
        switch (abilityName.ToLower())
        {
            case "doublejump":
                doubleJump.isUnlocked = true;
                break;
            case "dash":
                dashAbility.isUnlocked = true;
                break;
            case "walljump":
                wallJump.isUnlocked = true;
                break;
        }
    }

    // 在玩家落地时重置二段跳
    public void ResetDoubleJump()
    {
        canDoubleJump = true;
    }
} 