using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private bool isPaused = false;

    private void Start()
    {
        // 初始化按钮监听
        if (resumeButton) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton) settingsButton.onClick.AddListener(ShowSettings);
        if (restartButton) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(LoadMainMenu);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);

        // 确保游戏开始时菜单是隐藏的
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // 检测ESC键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (pauseMenuPanel)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // 如果暂停，隐藏设置面板
        if (settingsPanel && isPaused)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void ResumeGame()
    {
        TogglePause();
    }

    private void ShowSettings()
    {
        if (settingsPanel)
        {
            settingsPanel.SetActive(true);
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // 假设主菜单是场景索引0
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 