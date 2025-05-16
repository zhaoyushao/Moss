using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string portalID;
    [SerializeField] private string targetPortalID;
    [SerializeField] private float transitionDuration = 1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject activationEffect;
    [SerializeField] private GameObject transitionEffect;

    private bool isTransitioning = false;
    private static string lastUsedPortalID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(TransitionToNewScene());
        }
    }

    private IEnumerator TransitionToNewScene()
    {
        isTransitioning = true;
        lastUsedPortalID = portalID;

        // 播放传送音效
        AudioManager.Instance.Play("PortalActivate");

        // 显示激活特效
        if (activationEffect != null)
        {
            activationEffect.SetActive(true);
        }

        // 显示过渡特效
        if (transitionEffect != null)
        {
            transitionEffect.SetActive(true);
        }

        // 等待过渡动画
        yield return new WaitForSeconds(transitionDuration);

        // 保存目标传送门ID到PlayerPrefs
        PlayerPrefs.SetString("TargetPortalID", targetPortalID);

        // 加载新场景
        SceneManager.LoadScene(targetSceneName);
    }

    private void Start()
    {
        // 检查是否是目标传送门
        string savedTargetPortalID = PlayerPrefs.GetString("TargetPortalID", "");
        
        if (portalID == savedTargetPortalID)
        {
            // 找到玩家并将其传送到这个传送门
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
                
                // 清除保存的传送门ID
                PlayerPrefs.DeleteKey("TargetPortalID");
            }
        }

        // 初始化特效为关闭状态
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
        if (transitionEffect != null)
        {
            transitionEffect.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        // 在Scene视图中绘制传送门连接线
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // 如果有目标场景，显示场景名称
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
            $"To: {targetSceneName}\nID: {portalID}");
        #endif
    }
} 