using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private List<LevelData> levels;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject transitionPanel;
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] levelMusic;
    [SerializeField] private float musicFadeTime = 1f;

    private int currentLevelIndex = -1;
    private bool isLoading = false;
    private bool hasInitialized = false;

    // 事件
    public delegate void LevelLoadedHandler(LevelData levelData);
    public event LevelLoadedHandler OnLevelLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初始化音乐源
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 播放主菜单音乐
        PlayMainMenuMusic();
        // 移除自动加载，等待开始按钮点击
        hasInitialized = true;
    }

    public void StartGame()
    {
        if (levels.Count > 0)
        {
            LoadLevel(0);
        }
        else
        {
            Debug.LogError("No levels configured in LevelManager!");
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (isLoading || levelIndex < 0 || levelIndex >= levels.Count)
            return;

        StartCoroutine(LoadLevelAsync(levelIndex));
    }

    public void LoadNextLevel()
    {
        if (currentLevelIndex < levels.Count - 1)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            // 游戏通关
            Debug.Log("Game Complete!");
            // 触发游戏完成事件
            OnGameComplete?.Invoke();
        }
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    private IEnumerator LoadLevelAsync(int levelIndex)
    {
        isLoading = true;

        // 显示过渡画面
        if (transitionPanel != null)
            transitionPanel.SetActive(true);

        // 等待过渡动画
        yield return new WaitForSeconds(transitionTime * 0.5f);

        // 显示加载画面
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // 加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levels[levelIndex].sceneName);
        asyncLoad.allowSceneActivation = false;

        // 等待场景加载
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 等待剩余的过渡时间
        yield return new WaitForSeconds(transitionTime * 0.5f);

        // 激活场景
        asyncLoad.allowSceneActivation = true;
        currentLevelIndex = levelIndex;

        // 等待场景完全加载
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 等待一帧确保场景完全加载
        yield return new WaitForEndOfFrame();

        // 初始化关卡
        InitializeLevel(levels[levelIndex]);

        // 隐藏加载画面
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // 隐藏过渡画面
        if (transitionPanel != null)
            transitionPanel.SetActive(false);

        isLoading = false;
    }

    private void InitializeLevel(LevelData levelData)
    {
        // 设置玩家出生点
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && levelData.playerSpawnPoint != null)
        {
            player.transform.position = levelData.playerSpawnPoint.position;
        }

        // 初始化关卡特定的游戏对象
        if (levelData.levelObjects != null)
        {
            foreach (var obj in levelData.levelObjects)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // 播放关卡音乐
        if (levelData.backgroundMusic != null && musicSource != null)
        {
            StartCoroutine(CrossFadeMusic(levelData.backgroundMusic));
        }

        // 设置关卡特效
        if (levelData.levelParticles != null)
        {
            Instantiate(levelData.levelParticles);
        }

        // 触发关卡加载事件
        OnLevelLoaded?.Invoke(levelData);
    }

    public LevelData GetCurrentLevelData()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            return levels[currentLevelIndex];
        return null;
    }

    public bool IsLastLevel()
    {
        return currentLevelIndex == levels.Count - 1;
    }

    // 游戏完成事件
    public delegate void GameCompleteHandler();
    public event GameCompleteHandler OnGameComplete;

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null && musicSource != null)
        {
            StartCoroutine(CrossFadeMusic(mainMenuMusic));
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newMusic)
    {
        if (musicSource == null) yield break;

        // 保存当前音量
        float startVolume = musicSource.volume;
        
        // 淡出当前音乐
        float timer = 0;
        while (timer < musicFadeTime)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, timer / musicFadeTime);
            yield return null;
        }

        // 切换音乐
        musicSource.clip = newMusic;
        musicSource.Play();

        // 淡入新音乐
        timer = 0;
        while (timer < musicFadeTime)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, startVolume, timer / musicFadeTime);
            yield return null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
    }
} 