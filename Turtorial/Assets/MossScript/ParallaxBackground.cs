using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform mainCameraTrans;//主摄像机
    private Vector3 lastCameraPos;//摄像机上一帧的位置
    public float parallaxEffect;//视差效果
    public Vector2 followSpeed;//跟随速度
    
    private float spriteWidth;  // 精灵的宽度
    private SpriteRenderer spriteRenderer;
    private Transform leftCopy;   // 左边的复制体
    private Transform rightCopy;  // 右边的复制体
    
    // Start is called before the first frame update
    void Start()
    {
        mainCameraTrans = Camera.main.transform;//获取主摄像机
        lastCameraPos = mainCameraTrans.position;//记录摄像机上一帧的位置

        // 获取精灵渲染器和宽度
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteWidth = spriteRenderer.bounds.size.x;

        // 创建左右复制体
        CreateCopies();
    }

    private void CreateCopies()
    {
        // 创建左边的复制体
        GameObject leftObj = new GameObject(gameObject.name + "_Left");
        leftCopy = leftObj.transform;
        leftCopy.parent = transform.parent;
        SpriteRenderer leftRenderer = leftObj.AddComponent<SpriteRenderer>();
        leftRenderer.sprite = spriteRenderer.sprite;
        leftRenderer.sortingOrder = spriteRenderer.sortingOrder;
        leftCopy.position = transform.position + Vector3.left * spriteWidth;

        // 创建右边的复制体
        GameObject rightObj = new GameObject(gameObject.name + "_Right");
        rightCopy = rightObj.transform;
        rightCopy.parent = transform.parent;
        SpriteRenderer rightRenderer = rightObj.AddComponent<SpriteRenderer>();
        rightRenderer.sprite = spriteRenderer.sprite;
        rightRenderer.sortingOrder = spriteRenderer.sortingOrder;
        rightCopy.position = transform.position + Vector3.right * spriteWidth;

        Debug.Log($"创建背景复制体 - 宽度: {spriteWidth}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        ImageFollowCamera();
        CheckAndResetPositions();
        lastCameraPos = mainCameraTrans.position;
    }

    private void ImageFollowCamera()
    {
        Vector3 offsetPosition = mainCameraTrans.position - lastCameraPos;
        Vector3 movement = new Vector3(offsetPosition.x * followSpeed.x, offsetPosition.y * followSpeed.y, 0);
        
        // 移动所有背景
        transform.position += movement;
        if (leftCopy != null) leftCopy.position += movement;
        if (rightCopy != null) rightCopy.position += movement;
    }

    private void CheckAndResetPositions()
    {
        // 获取相机视图的边界
        float cameraX = mainCameraTrans.position.x;
        float viewportHalfWidth = Camera.main.orthographicSize * Screen.width / Screen.height;

        // 检查中间背景
        float distanceToCamera = Mathf.Abs(cameraX - transform.position.x);
        if (distanceToCamera >= spriteWidth)
        {
            // 将最远的背景移动到前面
            float direction = Mathf.Sign(cameraX - transform.position.x);
            transform.position = new Vector3(cameraX + direction * (spriteWidth - distanceToCamera % spriteWidth), 
                transform.position.y, transform.position.z);
        }

        // 确保左右复制体始终在正确的位置
        if (leftCopy != null)
        {
            leftCopy.position = new Vector3(transform.position.x - spriteWidth, 
                transform.position.y, transform.position.z);
        }
        if (rightCopy != null)
        {
            rightCopy.position = new Vector3(transform.position.x + spriteWidth, 
                transform.position.y, transform.position.z);
        }
    }

    private void OnDestroy()
    {
        // 清理复制体
        if (leftCopy != null) Destroy(leftCopy.gameObject);
        if (rightCopy != null) Destroy(rightCopy.gameObject);
    }
}
