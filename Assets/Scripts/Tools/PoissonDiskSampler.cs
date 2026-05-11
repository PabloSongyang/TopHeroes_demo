#region 泊松盘采样
/*
 * 泊松盘采样 (Poisson Disk Sampling) 是一种让点分布看起来“随机但均匀”的算法。它能保证任意两个点之间的距离都不小于一个设定的半径 R。
 * 相比于物理弹开，它的优势是计算一次即可获得完美布局，不会有物理碰撞的抖动和额外的 CPU 开销
 * 算法使用一个 背景网格 (Grid) 来加速查找。网格的单元格大小设为 (R/根号2)（在 2D 中），这样每个格子内最多只能存在一个点。
 * 第一步：随机选一个起始点。
 * 第二步：以该点为基础，在半径 [R, 2R] 的环状区域内随机尝试生成新点。
 * 第三步：检查新点周围的网格。如果新点与已有点的距离都 R，则接受该点。
 * 第四步：重复，直到没有新点可以生成。
 */
#endregion

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泊松盘采样器
/// </summary>
public static class PoissonDiskSampler
{
    public static List<Vector2> GeneratePoints(float radius, Rect bounds, int maxCandidates = 30)
    {
        // 单元格大小确保每个格子里最多只有一个点
        float cellSize = radius / Mathf.Sqrt(2);

        // 初始化网格
        int gridWidth = Mathf.CeilToInt(bounds.width / cellSize);
        int gridHeight = Mathf.CeilToInt(bounds.height / cellSize);
        int[,] grid = new int[gridWidth, gridHeight];
        // 填充 -1 表示该格为空
        for (int i = 0; i < gridWidth; i++)
            for (int j = 0; j < gridHeight; j++) grid[i, j] = -1;

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        // 选取初始点
        Vector2 firstPoint = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax));
        spawnPoints.Add(firstPoint);
        points.Add(firstPoint);
        grid[Mathf.FloorToInt((firstPoint.x - bounds.xMin) / cellSize), Mathf.FloorToInt((firstPoint.y - bounds.yMin) / cellSize)] = points.Count - 1;

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < maxCandidates; i++)
            {
                // 在 [R, 2R] 范围内生成候选点
                float angle = Random.value * Mathf.PI * 2;
                float r = Random.Range(radius, 2 * radius);
                Vector2 candidate = spawnCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

                if (IsValid(candidate, bounds, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[Mathf.FloorToInt((candidate.x - bounds.xMin) / cellSize), Mathf.FloorToInt((candidate.y - bounds.yMin) / cellSize)] = points.Count - 1;
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted) spawnPoints.RemoveAt(spawnIndex);
        }

        return points;
    }

    private static bool IsValid(Vector2 p, Rect bounds, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (p.x < bounds.xMin || p.x >= bounds.xMax || p.y < bounds.yMin || p.y >= bounds.yMax) return false;

        int xIndex = Mathf.FloorToInt((p.x - bounds.xMin) / cellSize);
        int yIndex = Mathf.FloorToInt((p.y - bounds.yMin) / cellSize);

        // 检查周围 5x5 的格子 (通常 3x3 即可，但 5x5 更稳健)
        int searchStartX = Mathf.Max(0, xIndex - 2);
        int searchEndX = Mathf.Min(grid.GetLength(0) - 1, xIndex + 2);
        int searchStartY = Mathf.Max(0, yIndex - 2);
        int searchEndY = Mathf.Min(grid.GetLength(1) - 1, yIndex + 2);

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                int pointIndex = grid[x, y];
                if (pointIndex != -1)
                {
                    float sqrDst = (p - points[pointIndex]).sqrMagnitude;
                    if (sqrDst < radius * radius) return false;
                }
            }
        }
        return true;
    }
}