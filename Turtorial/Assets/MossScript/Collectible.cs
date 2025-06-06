using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 1f;
    [SerializeField] private GameObject collectEffect;

    private Vector3 startPosition;
    private LevelGoal levelGoal;
    private CollectibleUI collectibleUI;

    private void Start()
    {
        startPosition = transform.position;
        levelGoal = FindObjectOfType<LevelGoal>();
        collectibleUI = FindObjectOfType<CollectibleUI>();
    }

    private void Update()
    {
        // 旋转效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 上下浮动效果
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // 播放收集音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Collect");
        }

        // 通知关卡目标
        if (levelGoal != null)
        {
            levelGoal.AddCollectible();
        }

        // 通知UI更新
        if (collectibleUI != null)
        {
            collectibleUI.AddOne();
        }

        // 播放收集效果
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // 销毁收集物
        Destroy(gameObject);
    }
} 