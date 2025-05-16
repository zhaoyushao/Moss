using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 startPosition;
    private float floatTimer;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // 旋转效果
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 上下浮动效果
        floatTimer += Time.deltaTime;
        float newY = startPosition.y + Mathf.Sin(floatTimer * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 通知游戏管理器增加分数
            GameManager.Instance.AddScore(scoreValue);
            
            // 播放收集动画或特效（如果有的话）
            // TODO: 在这里添加粒子效果或动画

            // 销毁物品
            Destroy(gameObject);
        }
    }
} 