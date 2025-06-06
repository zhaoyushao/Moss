using UnityEngine;
using System.Collections.Generic;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;  // 收集物预制体
    [SerializeField] private int totalCollectibles = 5;     // 要生成的收集物总数
    [SerializeField] private float minHeight = 1f;          // 最小生成高度
    [SerializeField] private float maxHeight = 5f;          // 最大生成高度
    [SerializeField] private float spawnRadius = 10f;       // 生成半径
    [SerializeField] private LayerMask groundLayer;         // 地面层
    [SerializeField] private float raycastDistance = 10f;   // 射线检测距离
    [SerializeField] private float minDistanceBetweenCollectibles = 2f; // 收集物之间的最小距离

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;   // 是否显示调试图形
    [SerializeField] private Color gizmoColor = Color.yellow;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private CollectibleUI collectibleUI;

    private void Start()
    {
        collectibleUI = FindObjectOfType<CollectibleUI>();
        SpawnCollectibles();
    }

    private void SpawnCollectibles()
    {
        // 清空之前的生成点
        spawnPoints.Clear();

        // 尝试生成指定数量的收集物
        int attempts = 0;
        int maxAttempts = totalCollectibles * 3; // 最大尝试次数
        int spawnedCount = 0;

        while (spawnedCount < totalCollectibles && attempts < maxAttempts)
        {
            // 生成随机位置
            Vector3 randomPosition = GetRandomPosition();
            
            // 检查位置是否合适
            if (IsValidPosition(randomPosition))
            {
                // 生成收集物
                GameObject collectible = Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);
                collectible.transform.parent = transform; // 设置为子物体
                spawnPoints.Add(randomPosition);
                spawnedCount++;
            }

            attempts++;
        }

        // 更新UI显示总数
        if (collectibleUI != null)
        {
            collectibleUI.SetTotal(spawnedCount);
            collectibleUI.SetCurrent(0);
        }

        // 更新GameManager中的总数
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // 通过反射设置totalCollectibles
            var field = gameManager.GetType().GetField("totalCollectibles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(gameManager, spawnedCount);
            }
        }

        Debug.Log($"成功生成 {spawnedCount} 个收集物");
    }

    private Vector3 GetRandomPosition()
    {
        // 在圆形区域内随机生成位置
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // 从上方发射射线检测地面
        RaycastHit hit;
        if (Physics.Raycast(randomPosition + Vector3.up * raycastDistance, Vector3.down, out hit, raycastDistance * 2, groundLayer))
        {
            // 在地面上方随机高度生成
            float randomHeight = Random.Range(minHeight, maxHeight);
            return hit.point + Vector3.up * randomHeight;
        }

        return randomPosition;
    }

    private bool IsValidPosition(Vector3 position)
    {
        // 检查是否与其他收集物太近
        foreach (Vector3 existingPoint in spawnPoints)
        {
            if (Vector3.Distance(position, existingPoint) < minDistanceBetweenCollectibles)
            {
                return false;
            }
        }

        // 检查是否在地面层上方
        RaycastHit hit;
        if (!Physics.Raycast(position + Vector3.up * raycastDistance, Vector3.down, out hit, raycastDistance * 2, groundLayer))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // 绘制生成范围
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // 绘制已生成的收集物位置
        Gizmos.color = Color.green;
        foreach (Vector3 point in spawnPoints)
        {
            Gizmos.DrawSphere(point, 0.3f);
        }
    }
} 