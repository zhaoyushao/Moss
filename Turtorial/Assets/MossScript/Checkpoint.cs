using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private string checkpointID;
    [SerializeField] private GameObject activationEffect;
    [SerializeField] private float effectDuration = 2f;
    
    [Header("Visual Settings")]
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 检查是否是最后激活的存档点
        string lastCheckpoint = PlayerPrefs.GetString("LastCheckpoint", "");
        if (checkpointID == lastCheckpoint)
        {
            Activate(false);
        }
        else
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            Activate(true);
            SavePlayerState();
        }
    }

    private void Activate(bool playEffect)
    {
        isActivated = true;
        spriteRenderer.sprite = activeSprite;

        if (playEffect)
        {
            // 播放激活音效
            AudioManager.Instance.Play("CheckpointActivate");

            // 显示激活特效
            if (activationEffect != null)
            {
                StartCoroutine(ShowActivationEffect());
            }
        }

        // 保存当前存档点ID
        PlayerPrefs.SetString("LastCheckpoint", checkpointID);
        PlayerPrefs.Save();
    }

    private IEnumerator ShowActivationEffect()
    {
        activationEffect.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        activationEffect.SetActive(false);
    }

    private void SavePlayerState()
    {
        // 保存玩家位置
        PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);

        // 保存当前场景
        PlayerPrefs.SetString("LastScene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // 保存玩家状态（生命值、分数等）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameState();
        }

        PlayerPrefs.Save();
    }

    public static void LoadLastCheckpoint()
    {
        string lastCheckpoint = PlayerPrefs.GetString("LastCheckpoint", "");
        if (string.IsNullOrEmpty(lastCheckpoint))
        {
            Debug.LogWarning("No checkpoint found to load!");
            return;
        }

        string lastScene = PlayerPrefs.GetString("LastScene", "");
        if (lastScene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            // 如果在不同场景，加载正确的场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(lastScene);
        }
        else
        {
            // 在同一场景中，直接移动玩家到存档点
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float x = PlayerPrefs.GetFloat("PlayerPosX", 0);
                float y = PlayerPrefs.GetFloat("PlayerPosY", 0);
                player.transform.position = new Vector3(x, y, 0);
            }

            // 恢复玩家状态
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameState();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 在Scene视图中显示存档点
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
            $"Checkpoint ID: {checkpointID}");
        #endif
    }
} 