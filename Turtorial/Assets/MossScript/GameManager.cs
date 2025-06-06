using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CollectibleUI collectibleUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject victoryMenu;

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int totalCollectibles = 5; // 第一关的收集物总数

    private int currentScore = 0;
    private int currentLives;
    private bool isGameOver = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGame();
        if (collectibleUI != null)
        {
            collectibleUI.SetTotal(totalCollectibles);
            collectibleUI.SetCurrent(0);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pauseMenu) pauseMenu.SetActive(false);
        if (victoryMenu) victoryMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
    }

    public void InitializeGame()
    {
        currentScore = 0;
        currentLives = startingLives;
        isGameOver = false;
        UpdateScoreUI();
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void AddScore(int points)
    {
        if (!isGameOver)
        {
            currentScore += points;
            UpdateScoreUI();
        }
    }

    public void LoseLife()
    {
        if (!isGameOver)
        {
            currentLives--;
            if (currentLives <= 0)
            {
                GameOver();
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void Victory()
    {
        isGameOver = true;
        if (victoryMenu) victoryMenu.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InitializeGame();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // 假设主菜单是场景索引0
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void SaveGameState()
    {
        PlayerPrefs.SetInt("PlayerScore", currentScore);
        PlayerPrefs.SetInt("PlayerLives", currentLives);
        PlayerPrefs.Save();
    }

    public void LoadGameState()
    {
        currentScore = PlayerPrefs.GetInt("PlayerScore", 0);
        currentLives = PlayerPrefs.GetInt("PlayerLives", startingLives);
        UpdateScoreUI();
    }

    public void OnPlayerDeath()
    {
        LoseLife();
        if (!isGameOver)
        {
            Checkpoint.LoadLastCheckpoint();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseMenu) pauseMenu.SetActive(isPaused);
    }
} 