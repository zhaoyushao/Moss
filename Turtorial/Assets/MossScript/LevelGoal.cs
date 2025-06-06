using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private bool requireAllCollectibles = true;
    [SerializeField] private int requiredCollectibles = 0;

    [Header("Visual Settings")]
    [SerializeField] private GameObject goalEffect;
    [SerializeField] private float effectDuration = 2f;

    private bool isPlayerInGoal = false;
    private int collectedItems = 0;

    private void Start()
    {
        // 测试脚本是否正在运行
        Debug.Log("LevelGoal script is running!");
        
        // 检查碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("No Collider2D component found on LevelGoal!");
        }
        else
        {
            Debug.Log($"Collider2D found: {collider.GetType().Name}, Is Trigger: {collider.isTrigger}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Something entered trigger: {other.gameObject.name}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered goal!");
            isPlayerInGoal = true;
            CheckLevelCompletion();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Something exited trigger: {other.gameObject.name}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left goal!");
            isPlayerInGoal = false;
        }
    }

    public void AddCollectible()
    {
        collectedItems++;
        Debug.Log($"Collectible added! Total: {collectedItems}");
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (!isPlayerInGoal) return;

        bool canComplete = true;
        if (requireAllCollectibles)
        {
            canComplete = collectedItems >= requiredCollectibles;
            Debug.Log($"Checking completion: {collectedItems}/{requiredCollectibles} collectibles");
        }

        if (canComplete)
        {
            Debug.Log("Level can be completed!");
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        Debug.Log($"Level completed! Moving to {nextLevelName}");
        
        // 播放完成效果
        if (goalEffect != null)
        {
            Instantiate(goalEffect, transform.position, Quaternion.identity);
        }

        // 延迟加载下一关
        Invoke(nameof(LoadNextLevel), effectDuration);
    }

    private void LoadNextLevel()
    {
        Debug.Log("Loading next level...");
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogError("SceneLoader instance not found!");
        }
    }

    // 在编辑器中可视化触发器范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
} 