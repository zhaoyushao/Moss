using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadingTime = 1f;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;

    private static LevelLoader instance;
    public static LevelLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LevelLoader>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("LevelLoader");
                    instance = obj.AddComponent<LevelLoader>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelAsync(sceneName));
    }

    private IEnumerator LoadLevelAsync(string sceneName)
    {
        // 显示加载画面
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // 重置进度条
        if (progressBar != null)
            progressBar.value = 0f;
        if (progressText != null)
            progressText.text = "0%";

        // 开始加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;
        float progress = 0f;

        // 等待场景加载
        while (!asyncLoad.isDone)
        {
            // 计算加载进度
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // 更新UI
            if (progressBar != null)
                progressBar.value = progress;
            if (progressText != null)
                progressText.text = $"{Mathf.Round(progress * 100)}%";

            // 确保最小加载时间
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 等待一帧确保场景完全加载
        yield return new WaitForEndOfFrame();

        // 隐藏加载画面
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            LoadLevel(SceneManager.GetSceneByBuildIndex(currentSceneIndex + 1).name);
        }
        else
        {
            Debug.LogWarning("No next level available!");
        }
    }

    public void LoadMainMenu()
    {
        LoadLevel("MainMenu");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 