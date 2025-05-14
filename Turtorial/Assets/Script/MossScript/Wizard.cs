using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;

    [Header("移动参数")]
    public float speed = 20f;

    float xVelocity;

    [Header("跳跃参数")]
    public float jumpForce = 20f;

    int jumpCount;//跳跃次数

    [Header("状态")]
    public bool isOnGround;

    [Header("环境检测")]
    public LayerMask groundLayer;

    //按键设置
    bool jumpPress;
    
    //相机
    public Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        
    }


    void Update()
    {
        xVelocity = Input.GetAxisRaw("Horizontal");
        //跳跃
        if ( jumpCount > 0 && Input.GetKeyDown(KeyCode.W))
        {
            GetComponent<Animator>().Play("Wizard_Jump");
            jumpPress = true;
            
            
            //在地面上跳跃
            if (jumpPress && isOnGround)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount--;
                jumpPress = false;
            }
            //在空中跳跃
            else if (jumpPress && jumpCount > 0 && !isOnGround)
            {
                rb.velocity = new Vector2(rb.velocity.x,jumpForce);
                jumpCount--;
                jumpPress = false;
            }
        }
        else if (Input.GetKey(KeyCode.A))//向左
        {
            GetComponent<Animator>().Play("Wizard_Walk");
            transform.localScale = new Vector3(xVelocity, 1, 1);
            rb.velocity = new Vector2(-speed,rb.velocity.y);
        }
        else if (Input.GetKey(KeyCode.D))//向右
        {
            transform.localScale = new Vector3(xVelocity, 1, 1);
            GetComponent<Animator>().Play("Wizard_Walk");
            rb.velocity = new Vector2(speed,rb.velocity.y);
        }
        else if (Input.GetKeyUp(KeyCode.W)||Input.GetKeyUp(KeyCode.A)||Input.GetKeyUp(KeyCode.D))
        {
            GetComponent<Animator>().Play("Wizard_Idel");
        }
        
        cam.transform.position = new Vector3(transform.position.x+speed, cam.transform.position.y, cam.transform.position.z);
    }

    void FixedUpdate()
    {
        isOnGroundCheck();
    }

    void isOnGroundCheck()
    {
        ////判断角色碰撞器与地面图层发生接触
        if (coll.IsTouchingLayers(groundLayer))
        {
            isOnGround = true;
            jumpCount = 1;
        }
        else
        {
            isOnGround = false;
        }
    }
}
