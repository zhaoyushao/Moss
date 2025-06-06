using UnityEngine;
using UnityEngine.Events;
using System;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy;
    [SerializeField] private float energyRegenRate = 5f; // 每秒恢复的能量
    [SerializeField] private float energyRegenDelay = 2f; // 使用能量后多久开始恢复

    [Header("Energy Costs")]
    [SerializeField] private float dashCost = 20f;
    [SerializeField] private float doubleJumpCost = 15f;
    [SerializeField] private float wallJumpCost = 10f;

    [Header("Events")]
    public UnityEvent<float> onEnergyChanged;
    public UnityEvent onEnergyDepleted;

    private float lastEnergyUseTime;
    private bool isRegenerating = true;

    private void Start()
    {
        currentEnergy = maxEnergy;
        onEnergyChanged?.Invoke(currentEnergy);
    }

    private void Update()
    {
        if (isRegenerating && Time.time >= lastEnergyUseTime + energyRegenDelay)
        {
            RegenerateEnergy();
        }
    }

    public bool UseEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            lastEnergyUseTime = Time.time;
            isRegenerating = false;
            onEnergyChanged?.Invoke(currentEnergy);

            if (currentEnergy <= 0)
            {
                onEnergyDepleted?.Invoke();
            }

            return true;
        }
        return false;
    }

    private void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(currentEnergy + energyRegenRate * Time.deltaTime, maxEnergy);
            onEnergyChanged?.Invoke(currentEnergy);
        }
        else
        {
            isRegenerating = true;
        }
    }

    public float GetEnergyPercentage()
    {
        return currentEnergy / maxEnergy;
    }

    public bool HasEnoughEnergy(float amount)
    {
        return currentEnergy >= amount;
    }

    // 获取各种技能的能量消耗
    public float GetDashCost() => dashCost;
    public float GetDoubleJumpCost() => doubleJumpCost;
    public float GetWallJumpCost() => wallJumpCost;

    // 用于UI显示
    public float GetCurrentEnergy() => currentEnergy;
    public float GetMaxEnergy() => maxEnergy;
} 