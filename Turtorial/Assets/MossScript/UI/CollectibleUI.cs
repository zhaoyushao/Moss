using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectibleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text countText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject collectiblePanel;

    private int totalCount;
    private int currentCount;

    private void Start()
    {
        // 初始化UI
        UpdateUI();
    }

    public void SetTotal(int total)
    {
        totalCount = total;
        UpdateUI();
    }

    public void SetCurrent(int current)
    {
        currentCount = current;
        UpdateUI();
    }

    public void AddOne()
    {
        currentCount++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = $"{currentCount}/{totalCount}";
        }
    }

    public void ShowUI()
    {
        if (collectiblePanel != null)
        {
            collectiblePanel.SetActive(true);
        }
    }

    public void HideUI()
    {
        if (collectiblePanel != null)
        {
            collectiblePanel.SetActive(false);
        }
    }
} 