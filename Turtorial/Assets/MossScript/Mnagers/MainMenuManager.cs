using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button musicMuteButton;
    [SerializeField] private Button sfxMuteButton;
    [SerializeField] private Button backButton;
        
    [Header("Quit")]
    [SerializeField] private Button confirmQuitButton;
    [SerializeField] private Button cancelQuitButton;

    [Header("Animation")]
    [SerializeField] private float buttonAnimationDelay = 0.1f;
    [SerializeField] private float panelTransitionTime = 0.5f;
    [SerializeField] private Ease panelTransitionEase = Ease.OutBack;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip panelSwitchSound;
    [SerializeField] private AudioSource uiAudioSource;

    private bool isTransitioning = false;

    private void Start()
    {
        // 初始化按钮监听
        InitializeButtons();
        
        // 初始化设置
        InitializeSettings();
        
        // 显示主面板
        ShowMainPanel();
        
        // 播放按钮动画
        StartCoroutine(AnimateButtons());

        // 初始化音频源
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Added AudioSource component to MainMenuManager");
        }

        // 检查音效文件
        if (buttonClickSound == null)
        {
            Debug.LogWarning("Button click sound is not set! Please assign a sound clip in the inspector.");
        }
        else
        {
            Debug.Log($"Button click sound loaded: {buttonClickSound.name}");
        }
    }

    private void InitializeButtons()
    {
        // 主面板按钮
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => OnButtonClick(OnStartClick));
            AddButtonHoverEffect(startButton);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => OnButtonClick(OnSettingsClick));
            AddButtonHoverEffect(settingsButton);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => OnButtonClick(OnQuitClick));
            AddButtonHoverEffect(quitButton);
        }

        // 返回按钮
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => OnButtonClick(OnBackClick));
            AddButtonHoverEffect(backButton);
        }

        // 退出确认按钮
        if (confirmQuitButton != null)
        {
            confirmQuitButton.onClick.AddListener(() => OnButtonClick(QuitGame));
            AddButtonHoverEffect(confirmQuitButton);
        }
        if (cancelQuitButton != null)
        {
            cancelQuitButton.onClick.AddListener(() => OnButtonClick(OnBackClick));
            AddButtonHoverEffect(cancelQuitButton);
        }

        // 音量控制按钮
        if (musicMuteButton != null)
        {
            musicMuteButton.onClick.AddListener(() => OnButtonClick(ToggleMusicMute));
            AddButtonHoverEffect(musicMuteButton);
        }
        if (sfxMuteButton != null)
        {
            sfxMuteButton.onClick.AddListener(() => OnButtonClick(ToggleSFXMute));
            AddButtonHoverEffect(sfxMuteButton);
        }
    }

    private void AddButtonHoverEffect(Button button)
    {
        // 添加悬停效果
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // 添加鼠标进入事件
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            button.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        });
        trigger.triggers.Add(enterEntry);

        // 添加鼠标退出事件
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            button.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        });
        trigger.triggers.Add(exitEntry);
    }

    private void OnButtonClick(System.Action action)
    {
        if (isTransitioning) return;

        // 播放按钮音效
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
        else
        {
            if (uiAudioSource == null)
                Debug.LogWarning("AudioSource is missing!");
            if (buttonClickSound == null)
                Debug.LogWarning("Button click sound is missing!");
        }

        // 执行按钮动作
        action?.Invoke();
    }

    private void InitializeSettings()
    {
        // 加载保存的设置
        if (musicVolumeSlider != null)
        {
            // 设置初始值
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            
            // 清除现有监听器
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            
            // 添加新的监听器
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
            
            Debug.Log("Music volume slider connected successfully");
        }
        else
        {
            Debug.LogWarning("Music volume slider is not assigned!");
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
        }
    }

    private IEnumerator AnimateButtons()
    {
        // 获取所有按钮
        Button[] buttons = mainPanel.GetComponentsInChildren<Button>();
        
        // 初始状态：按钮透明且位置偏移
        foreach (Button button in buttons)
        {
            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0;
            button.transform.localPosition += Vector3.up * 50f;
        }
        
        // 逐个显示按钮
        foreach (Button button in buttons)
        {
            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            
            // 使用DOTween创建动画
            Sequence sequence = DOTween.Sequence();
            sequence.Append(button.transform.DOLocalMoveY(button.transform.localPosition.y - 50f, panelTransitionTime).SetEase(panelTransitionEase));
            sequence.Join(canvasGroup.DOFade(1f, panelTransitionTime));
            
            yield return new WaitForSeconds(buttonAnimationDelay);
        }
    }

    private void ShowPanel(GameObject panel)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        // 播放面板切换音效
        if (uiAudioSource != null && panelSwitchSound != null)
        {
            uiAudioSource.PlayOneShot(panelSwitchSound);
        }

        // 使用DOTween创建面板切换动画
        Sequence sequence = DOTween.Sequence();

        // 隐藏所有面板
        GameObject[] allPanels = { mainPanel, settingsPanel, quitConfirmationPanel };
        foreach (GameObject p in allPanels)
        {
            if (p != null && p != panel)
            {
                sequence.Join(p.transform.DOScale(0.8f, panelTransitionTime).SetEase(Ease.InBack));
                sequence.Join(p.GetComponent<CanvasGroup>().DOFade(0f, panelTransitionTime));
            }
        }

        // 显示目标面板
        if (panel != null)
        {
            panel.SetActive(true);
            panel.transform.localScale = Vector3.one * 0.8f;
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            sequence.Append(panel.transform.DOScale(1f, panelTransitionTime).SetEase(panelTransitionEase));
            sequence.Join(canvasGroup.DOFade(1f, panelTransitionTime));
        }

        sequence.OnComplete(() => {
            // 隐藏其他面板
            foreach (GameObject p in allPanels)
            {
                if (p != null && p != panel)
                {
                    p.SetActive(false);
                }
            }
            isTransitioning = false;
        });
    }

    private void ShowMainPanel()
    {
        ShowPanel(mainPanel);
    }

    #region Button Click Handlers
    private void OnStartClick()
    {
        SceneLoader.Instance.LoadScene("LoadingScene");
        // 使用LevelManager的StartGame方法
        LevelManager.Instance?.StartGame();
        
    }

    private void OnSettingsClick()
    {
        ShowPanel(settingsPanel);
    }

    private void OnQuitClick()
    {
        ShowPanel(quitConfirmationPanel);
    }

    private void OnBackClick()
    {
        ShowMainPanel();
    }
    #endregion

    #region Settings Handlers
    private void OnMusicVolumeChanged(float value)
    {
        // 使用LevelManager来控制音乐音量
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetMusicVolume(value);
        }
        else
        {
            Debug.LogWarning("LevelManager.Instance is null!");
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // 更新音效音量
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = value;
        }
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void ToggleMusicMute()
    {
        if (musicVolumeSlider != null)
        {
            float currentVolume = musicVolumeSlider.value;
            musicVolumeSlider.value = currentVolume > 0 ? 0 : 1;
        }
    }

    private void ToggleSFXMute()
    {
        if (sfxVolumeSlider != null)
        {
            float currentVolume = sfxVolumeSlider.value;
            sfxVolumeSlider.value = currentVolume > 0 ? 0 : 1;
        }
    }

    // 添加公共方法供 Inspector 使用
    public void OnMusicVolumeSliderChanged(float value)
    {
        OnMusicVolumeChanged(value);
    }

    public void OnSFXVolumeSliderChanged(float value)
    {
        OnSFXVolumeChanged(value);
    }
    #endregion

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 