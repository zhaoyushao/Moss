using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading Settings")]
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private float minimumLoadingTime = 1f;

    private string targetSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SceneLoader initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"Starting to load scene: {sceneName}");
        targetSceneName = sceneName;
        StartCoroutine(LoadSceneSequence());
    }

    private IEnumerator LoadSceneSequence()
    {
        Debug.Log("Loading sequence started");

        // 1. 先加载LoadingScene
        Debug.Log($"Loading loading scene: {loadingSceneName}");
        AsyncOperation loadLoadingScene = SceneManager.LoadSceneAsync(loadingSceneName);
        loadLoadingScene.allowSceneActivation = false;

        // 等待LoadingScene加载到90%
        while (loadLoadingScene.progress < 0.9f)
        {
            Debug.Log($"Loading scene progress: {loadLoadingScene.progress}");
            yield return null;
        }

        // 激活LoadingScene
        loadLoadingScene.allowSceneActivation = true;
        yield return null;

        // 等待一帧，确保LoadingScene完全加载和初始化
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Loading scene loaded and initialized");

        // 2. 开始加载目标场景
        Debug.Log($"Loading target scene: {targetSceneName}");
        AsyncOperation loadTargetScene = SceneManager.LoadSceneAsync(targetSceneName);
        loadTargetScene.allowSceneActivation = false;

        float startTime = Time.time;
        float progress = 0f;

        // 加载进度
        while (!loadTargetScene.isDone)
        {
            // 计算实际加载进度
            progress = Mathf.Clamp01(loadTargetScene.progress / 0.9f);
            Debug.Log($"Target scene loading progress: {progress}");
            
            // 确保最小加载时间
            float elapsedTime = Time.time - startTime;
            float minimumProgress = elapsedTime / minimumLoadingTime;
            
            // 使用较大的进度值
            progress = Mathf.Max(progress, minimumProgress);

            // 更新UI
            if (LoadingUI.Instance != null)
            {
                LoadingUI.Instance.UpdateProgress(progress);
                Debug.Log($"Updated UI progress: {progress}");
            }
            else
            {
                Debug.LogWarning("LoadingUI.Instance is null!");
            }

            // 当加载进度达到90%且满足最小加载时间时，激活场景
            if (loadTargetScene.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                Debug.Log("Target scene ready to activate");
                loadTargetScene.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log("Target scene loaded");
    }
} 