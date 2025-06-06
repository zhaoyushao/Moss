using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float progressBarAnimationSpeed = 0.5f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private Color progressBarColor = Color.white;
    [SerializeField] private Color progressBarFillColor = Color.green;

    private float currentProgress = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("LoadingUI initialized");
            InitializeComponents();
        }
        else
        {
            Debug.Log("Duplicate LoadingUI found, destroying");
            Destroy(gameObject);
        }
    }

    private void InitializeComponents()
    {
        Debug.Log("Initializing LoadingUI components");
        
        // 确保所有组件都已赋值
        if (progressBar == null)
        {
            progressBar = GetComponentInChildren<Slider>();
            Debug.Log($"Progress Bar found: {progressBar != null}");
        }
        if (progressText == null)
        {
            progressText = GetComponentInChildren<Text>();
            Debug.Log($"Progress Text found: {progressText != null}");
        }
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Debug.Log($"Canvas Group found: {canvasGroup != null}");
        }

        // 初始化进度条
        if (progressBar != null)
        {
            progressBar.value = 0f;
            Debug.Log("Progress bar initialized to 0");
            progressBar.fillRect.GetComponent<Image>().color = progressBarFillColor;
        }
        else
        {
            Debug.LogError("Progress Bar reference is missing!");
        }

        // 初始化文本
        if (progressText != null)
        {
            progressText.text = "0%";
            Debug.Log("Progress text initialized to 0%");
        }
        else
        {
            Debug.LogError("Progress Text reference is missing!");
        }

        // 初始化CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeInTime);
        }
    }

    public void UpdateProgress(float progress)
    {
        Debug.Log($"Updating progress to: {progress}");

        currentProgress = progress;

        // 更新进度条
        if (progressBar != null)
        {
            progressBar.value = progress;
            Debug.Log($"Progress bar updated to: {progress}");
        }
        else
        {
            Debug.LogError("Progress Bar reference is missing!");
        }

        // 更新进度文本
        if (progressText != null)
        {
            progressText.text = $"{Mathf.Round(progress * 100)}%";
            Debug.Log($"Progress text updated to: {Mathf.Round(progress * 100)}%");
        }
        else
        {
            Debug.LogError("Progress Text reference is missing!");
        }
    }

    private void OnValidate()
    {
        // 验证设置
        if (progressBarAnimationSpeed < 0.1f)
        {
            progressBarAnimationSpeed = 0.1f;
            Debug.LogWarning("Progress Bar Animation Speed should be at least 0.1 seconds!");
        }
    }
} 