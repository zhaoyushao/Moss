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
    
    
    // Start is called before the first frame update
    void Start()
    {
        mainCameraTrans = Camera.main.transform;//获取主摄像机
        lastCameraPos = mainCameraTrans.position;//记录摄像机上一帧的位置

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        parallaxEffect=texture.width / sprite.bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        ImageFollowCamera();
        ResetImageX();
        
        lastCameraPos = mainCameraTrans.position;
    }

    private void ImageFollowCamera()
    {
        Vector3 offsetPostion = mainCameraTrans.position - lastCameraPos;
        transform.position+=new Vector3(offsetPostion.x*followSpeed.x,offsetPostion.y*followSpeed.y,0);
    }

    private void ResetImageX()
    {
        if (Mathf.Abs(mainCameraTrans.position.x - transform.position.x) >= parallaxEffect)
        {
            transform.position = new Vector3(mainCameraTrans.position.x , transform.position.y, transform.position.z);
        }
    }
}
