using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Basic Info")]
    public string levelName;
    public string sceneName;
    public Sprite levelPreview;
    [TextArea(3, 5)]
    public string levelDescription;

    [Header("Level Settings")]
    public Transform playerSpawnPoint;
    public List<GameObject> levelObjects;
    public float timeLimit = 0f; // 0表示无时间限制
    public int requiredCollectibles = 0;
    public bool isTutorialLevel = false;

    [Header("Level Progression")]
    public bool isUnlocked = false;
    public int starsRequiredToUnlock = 0;
    public int maxStars = 3;
    public List<LevelData> requiredLevels; // 需要先完成的关卡

    [Header("Level Rewards")]
    public int coinsReward = 0;
    public List<PlayerUpgrade> availableUpgrades;
    public List<GameObject> unlockableItems; // 解锁的物品（如新角色、新皮肤等）

    [Header("Level Music")]
    public AudioClip backgroundMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
    public AudioClip[] ambientSounds; // 环境音效

    [Header("Level Effects")]
    public GameObject levelParticles;
    public Color ambientColor = Color.white;
    public float fogDensity = 0f;
    public bool useParallax = true; // 是否使用视差效果
    public float parallaxStrength = 0.5f; // 视差强度

    [Header("Level Challenges")]
    public bool hasTimeChallenge = false;
    public float timeChallengeGoal = 0f;
    public bool hasCollectibleChallenge = false;
    public int collectibleChallengeGoal = 0;
    public bool hasHiddenArea = false;
    public List<Transform> hiddenAreaLocations;

    [Header("Level Tips")]
    [TextArea(2, 4)]
    public string[] levelTips; // 关卡提示

    public void UnlockLevel()
    {
        isUnlocked = true;
    }

    public bool CanBeUnlocked(int playerStars)
    {
        if (requiredLevels == null || requiredLevels.Count == 0)
        {
            return playerStars >= starsRequiredToUnlock;
        }

        // 检查是否完成了所有前置关卡
        foreach (var level in requiredLevels)
        {
            if (!level.isUnlocked)
            {
                return false;
            }
        }

        return playerStars >= starsRequiredToUnlock;
    }

    public string GetRandomTip()
    {
        if (levelTips == null || levelTips.Length == 0)
        {
            return "";
        }
        return levelTips[Random.Range(0, levelTips.Length)];
    }
} 