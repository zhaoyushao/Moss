using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;

    private int currentScore = 0;
    private int currentLives;
    private bool isGameOver = false;

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

    private void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InitializeGame();
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
} 