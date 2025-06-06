using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider energyBar;
    [SerializeField] private Text energyText;
    [SerializeField] private GameObject energyDepletedEffect;
    
    [Header("Slider Settings")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color lowEnergyColor = Color.red;
    [SerializeField] private float lowEnergyThreshold = 0.3f; // 30%以下显示红色
    [SerializeField] private Image fillImage; // Slider的Fill Image组件

    private EnergySystem energySystem;

    private void Start()
    {
        // 查找玩家身上的EnergySystem组件
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            energySystem = player.GetComponent<EnergySystem>();
            if (energySystem != null)
            {
                // 订阅能量变化事件
                energySystem.onEnergyChanged.AddListener(UpdateEnergyUI);
                energySystem.onEnergyDepleted.AddListener(ShowEnergyDepletedEffect);
                
                // 初始化UI
                UpdateEnergyUI(energySystem.GetCurrentEnergy());
            }
            else
            {
                Debug.LogError("EnergySystem not found on player!");
            }
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        // 确保Slider设置正确
        if (energyBar != null)
        {
            energyBar.minValue = 0;
            energyBar.maxValue = 1;
            energyBar.wholeNumbers = false;
        }
    }

    private void UpdateEnergyUI(float currentEnergy)
    {
        if (energyBar != null)
        {
            float energyPercentage = energySystem.GetEnergyPercentage();
            energyBar.value = energyPercentage;

            // 更新能量条颜色
            if (fillImage != null)
            {
                fillImage.color = energyPercentage <= lowEnergyThreshold ? lowEnergyColor : normalColor;
            }
        }

        if (energyText != null)
        {
            energyText.text = $"{Mathf.RoundToInt(currentEnergy)}/{energySystem.GetMaxEnergy()}";
        }
    }

    private void ShowEnergyDepletedEffect()
    {
        if (energyDepletedEffect != null)
        {
            energyDepletedEffect.SetActive(true);
            Invoke(nameof(HideEnergyDepletedEffect), 1f);
        }
    }

    private void HideEnergyDepletedEffect()
    {
        if (energyDepletedEffect != null)
        {
            energyDepletedEffect.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (energySystem != null)
        {
            energySystem.onEnergyChanged.RemoveListener(UpdateEnergyUI);
            energySystem.onEnergyDepleted.RemoveListener(ShowEnergyDepletedEffect);
        }
    }
} 