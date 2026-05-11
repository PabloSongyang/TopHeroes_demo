using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonRange : MonoBehaviour
{
    [SerializeField]
    private LineRenderer m_LineRenderer;

    [SerializeField]
    private PolygonCollider2D m_PolygonCollider2D;

    public void Init()
    {
        int count = this.m_LineRenderer.positionCount;
        Vector3[] worldPositions = new Vector3[count];
        this.m_LineRenderer.GetPositions(worldPositions);

        Vector2[] localPoints = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            // 核心修正：将世界坐标点 转换成 相对于当前物体的本地坐标点
            Vector3 localPos = this.transform.InverseTransformPoint(worldPositions[i]);
            localPoints[i] = new Vector2(localPos.x, localPos.y);
        }

        this.m_PolygonCollider2D.points = localPoints;
        this.m_LineRenderer.enabled = false;
    }

    public bool IsInRange(Vector3 pos)
    {
        return this.m_PolygonCollider2D.OverlapPoint(pos);
    }

    public Vector3 GetRandomPoint()
    {
        if (this.m_PolygonCollider2D == null) return transform.position;

        // 获取多边形的包围盒边界
        Bounds bounds = this.m_PolygonCollider2D.bounds;

        // 安全计数，防止多边形面积过小导致死循环
        int attempts = 0;
        while (attempts < 100)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(x, y);

            if (this.IsInRange(randomPoint))
            {
                return new Vector3(randomPoint.x, randomPoint.y, 0);
            }
            attempts++;
        }

        // 如果实在特么点儿背没随机到，就回退到中心点
        return bounds.center;
    }

    /// <summary>
    /// 使用泊松盘算法生成一批点
    /// </summary>
    /// <param name="simpleRadius"></param>
    /// <param name="totalCount"></param>
    /// <returns></returns>
    public List<Vector3> GetRandomPointByPoissonDisk(float simpleRadius, int totalCount)
    {
        Bounds b = this.m_PolygonCollider2D.bounds;
        Rect rect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);

        // 2. 预采样点位（性能极高，几百个点通常在 1-2ms 内完成）
        List<Vector2> potentialPoints = PoissonDiskSampler.GeneratePoints(simpleRadius, rect);

        // 3. 过滤掉不在多边形内的点
        List<Vector3> validPoints = new List<Vector3>();
        foreach (var p in potentialPoints)
        {
            Vector3 p3 = new Vector3(p.x, p.y, 0);
            if (this.IsInRange(p3)) validPoints.Add(p3);
            if (validPoints.Count >= totalCount) break; // 够了就停
        }
        return validPoints;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this.m_LineRenderer == null) this.m_LineRenderer = this.GetComponent<LineRenderer>();
        if (this.m_PolygonCollider2D == null) this.m_PolygonCollider2D = this.GetComponent<PolygonCollider2D>();
    }
#endif
}