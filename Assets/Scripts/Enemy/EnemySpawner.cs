using Cysharp.Threading.Tasks;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnRate = 1.5f;
    private Transform player;

    private List<GraphNode> walkableNodes;

    [SerializeField]
    private PolygonRange m_PolygonRange;

    private int m_EnemyEntityID;

    [SerializeField, Tooltip("Boss刷新位置")]
    private BossRange m_BossCreateRange;

    [SerializeField, Tooltip("玩家武器升2级的时候蓄力射击时的刷怪点")]
    private PolygonRange m_Player_Bullet_Arrow_Level2_Ultimate_EnemyCreatePolygonRange;

    [SerializeField, Tooltip("玩家武器升3级的时候蓄力射击时的刷怪点")]
    private PolygonRange m_Player_Bullet_Arrow_Level3_Ultimate_EnemyCreatePolygonRange;

    [SerializeField, Tooltip("玩家武器升满级的时候蓄力射击时的刷怪点")]
    private PolygonRange m_Player_Bullet_Max_EnemyCreatePolygonRange;

    private Coroutine m_SpawnInBatchCoroutine;

    private void Awake()
    {
        this.m_PolygonRange.Init();
        this.m_Player_Bullet_Arrow_Level2_Ultimate_EnemyCreatePolygonRange.Init();
        this.m_Player_Bullet_Arrow_Level3_Ultimate_EnemyCreatePolygonRange.Init();
        this.m_Player_Bullet_Max_EnemyCreatePolygonRange.Init();

        SWGameManager.Instance.EnemyCreatePolygonRangeDic.Clear();
        SWGameManager.Instance.EnemyCreatePolygonRangeDic.Add(this.m_Player_Bullet_Arrow_Level2_Ultimate_EnemyCreatePolygonRange.gameObject.name, this.m_Player_Bullet_Arrow_Level2_Ultimate_EnemyCreatePolygonRange);
        SWGameManager.Instance.EnemyCreatePolygonRangeDic.Add(this.m_Player_Bullet_Arrow_Level3_Ultimate_EnemyCreatePolygonRange.gameObject.name, this.m_Player_Bullet_Arrow_Level3_Ultimate_EnemyCreatePolygonRange);
        SWGameManager.Instance.EnemyCreatePolygonRangeDic.Add(this.m_Player_Bullet_Max_EnemyCreatePolygonRange.gameObject.name, this.m_Player_Bullet_Max_EnemyCreatePolygonRange);
    }

    private void Start()
    {
        //// 1. 获取当前的 GridGraph
        //GridGraph gridGraph = AstarPath.active.data.gridGraph;

        //// 2.存储所有可通行的节点
        //walkableNodes = new List<GraphNode>();
        //gridGraph.GetNodes((node) =>
        //{
        //    if (node.Walkable)
        //    {
        //        walkableNodes.Add(node);
        //    }
        //});



        player = SWGameManager.Instance.CurrentPlayer.transform;

        SWGameManager.Instance.OnGameStartEvent.AddListener(this.OnGameStartEvent);
        SWGameManager.Instance.OnPlayerWeaponUpgradeEvent.AddListener(this.OnPlayerWeaponUpgradeEvent);
        SWGameManager.Instance.OnPlayerChargedUpCompleteEvent.AddListener(this.OnPlayerChargedUpCompleteEvent);
        SWGameManager.Instance.OnPlayerWinEvent.AddListener(this.OnGameOverEvent);
        SWGameManager.Instance.OnPlayerDeadEvent.AddListener(this.OnGameOverEvent);
    }

    public void GetRandomWalkablePos()
    {
        if (!SWGameManager.Instance.IsInit) return;
        if (player == null) return;
        if (SWGameManager.Instance.CurrentPlayer.IsDead) return;
        if (SWGameManager.Instance.IsWin) return;

        Vector3 playerPos = SWGameManager.Instance.CurrentPlayer.transform.position;
        Vector3 randomPos = Vector3.zero;
        bool found = false;
        int maxAttempts = 20; // 防止死循环，给它个最大重试的机会

        for (int i = 0; i < maxAttempts; i++)
        {
            // 生成玩家周围
            Vector3 potentialPos = playerPos + new Vector3(
                Random.Range(-7f, 7f),
                Random.Range(-5f, 5f),
                0
            );

            // 获取距离该随机点最近的节点
            NNInfo info = AstarPath.active.GetNearest(potentialPos, NearestNodeConstraint.Walkable);

            // 检查该点是否可寻路？且距离随机点足够近（防止偏移太远）
            if (info.node != null && info.node.Walkable)
            {
                // 如果希望位置严谨地落在节点中心，可以使用 (Vector3)info.node.position
                // 如果希望在节点范围内有微小随机偏移，直接用 potentialPos
                randomPos = (Vector3)info.node.position;
                // 判断生成的点是否在生成器画圈范围内，如果不在，重来
                if (this.m_PolygonRange.IsInRange(randomPos))
                {
                    found = true;
                    break;
                }
            }
        }

        var resultPoint = found ? randomPos : playerPos; // 如果没找到，返回玩家位置作为保底
        this.CreateEnemy(resultPoint);
    }

    private void OnGameStartEvent()
    {
        this.m_EnemyEntityID = 1;
        this.CreateEnemy(this.m_BossCreateRange.transform.position, true);
        InvokeRepeating(nameof(GetRandomWalkablePos), 0, spawnRate);
    }

    private void OnPlayerWeaponUpgradeEvent(int level)
    {
        CancelInvoke(nameof(GetRandomWalkablePos));
        switch (level)
        {
            case 2:
                this.SpawnInBatchStart(100, SWGameManager.Instance.CurrentPlayer.PlayerSo.SizeChangeSpeed, this.m_Player_Bullet_Arrow_Level2_Ultimate_EnemyCreatePolygonRange, .75f);
                break;
            case 3:
                this.SpawnInBatchStart(100, SWGameManager.Instance.CurrentPlayer.PlayerSo.SizeChangeSpeed, this.m_Player_Bullet_Arrow_Level3_Ultimate_EnemyCreatePolygonRange, .75f);
                break;
            case 4:
                this.SpawnInBatchStart(100, SWGameManager.Instance.CurrentPlayer.PlayerSo.SizeChangeSpeed, this.m_Player_Bullet_Max_EnemyCreatePolygonRange, .75f);
                break;
        }
    }

    private void OnPlayerChargedUpCompleteEvent()
    {
        if (this.m_SpawnInBatchCoroutine != null)
        {
            base.StopCoroutine(this.m_SpawnInBatchCoroutine);
        }
        InvokeRepeating(nameof(GetRandomWalkablePos), 0, spawnRate);
    }

    private void OnGameOverEvent()
    {
        CancelInvoke(nameof(GetRandomWalkablePos));
    }

    private void SpawnInBatchStart(int totalCount, float duration, PolygonRange createRange, float createRadius)
    {
        if (this.m_SpawnInBatchCoroutine != null)
        {
            base.StopCoroutine(this.m_SpawnInBatchCoroutine);
        }
        this.m_SpawnInBatchCoroutine = base.StartCoroutine(this.SpawnInBatchCoroutine(totalCount, duration, createRange, createRadius));
    }


    private IEnumerator SpawnInBatchCoroutine(int totalCount, float duration, PolygonRange createRange, float createRadius)
    {
        // 获取随机点位（假设这个方法是同步的）
        List<Vector3> validPoints = createRange.GetRandomPointByPoissonDisk(createRadius, totalCount);

        int spawned = 0;
        float startTime = Time.time;
        int actualToSpawn = Mathf.Min(totalCount, validPoints.Count);

        while (spawned < actualToSpawn)
        {
            // 协程不需要手动检查 Token，外部可以通过 StopCoroutine 停止它

            float elapsed = Time.time - startTime;
            // 计算当前时间点应该生成的总数
            int targetCount = (duration <= 0) ? actualToSpawn : Mathf.FloorToInt((elapsed / duration) * actualToSpawn);
            targetCount = Mathf.Min(targetCount, actualToSpawn);

            while (spawned < targetCount)
            {
                CreateEnemy(validPoints[spawned]);
                spawned++;
            }

            // 等待下一帧
            yield return null;
        }
    }

    //private void TrySpawn()
    //{
    //    if (!SWGameManager.Instance.IsInit) return;
    //    if (player == null) return;
    //    if (SWGameManager.Instance.CurrentPlayer.IsDead) return;

    //    List<GridNodeBase> gnbList = this.GetNeighborsInArea(player.position, 1);

    //    List<GridNodeBase> finalList = new List<GridNodeBase>();
    //    for (int i = 0; i < gnbList.Count; i++)
    //    {
    //        GridNodeBase gridNodeBase = gnbList[i];
    //        bool b = IsLineOfSightClear(player.position, (Vector3)gridNodeBase.position);
    //        if (!b)
    //        {
    //            finalList.Add(gridNodeBase);
    //        }
    //    }

    //    if (finalList.Count > 0)
    //    {
    //        GridNodeBase gridNodeBase = finalList[Random.Range(0, finalList.Count)];
    //        this.CreateEnemy((Vector3)gridNodeBase.position);
    //    }
    //}

    private void CreateEnemy(Vector3 createPosition, bool isBoss = false)
    {
        if (isBoss)
        {
            EnemyAI boss = _("Boss");
            this.m_BossCreateRange.SetBoss(boss);
        }
        else
        {
            string objName = PoolManager.Instance.EnemyPool.GetRandomObjName("Boss");
            _(objName);
        }

        EnemyAI _(string enemyLabel)
        {
            GameObject go = PoolManager.Instance.EnemyPool.Get(enemyLabel, createPosition, Quaternion.identity);
            if (go != null && go.TryGetComponent<EnemyAI>(out EnemyAI enemyAI))
            {
                enemyAI.Create(this.m_EnemyEntityID);
                SWGameManager.Instance.EnemiesAIDic.Add(this.m_EnemyEntityID, enemyAI);
                this.m_EnemyEntityID++;
                return enemyAI;
            }
            return null;
        }
    }


    /// <summary>
    /// 获取玩家中心n×n范围内的可寻路的格子
    /// </summary>
    /// <param name="position">中心坐标（玩家）</param>
    /// <param name="radius">1对应3×3，2对应4×4，3对应5×5，以此类推……</param>
    /// <returns></returns>
    public List<GridNodeBase> GetNeighborsInArea(Vector3 position, int radius)
    {
        List<GridNodeBase> neighbors = new List<GridNodeBase>();
        var gg = AstarPath.active.data.gridGraph;

        // 1. 获取中心节点的网格坐标
        GridNodeBase centerNode = gg.GetNearest(position).node as GridNodeBase;
        if (centerNode == null) return neighbors;

        int centerX = centerNode.XCoordinateInGrid;
        int centerZ = centerNode.ZCoordinateInGrid;

        // 2. 遍历从 -radius 到 +radius 的区域
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dz = -radius; dz <= radius; dz++)
            {
                // 如果不需要包含玩家中心点，取消下面这一行的注释
                // if (dx == 0 && dz == 0) continue; 

                int targetX = centerX + dx;
                int targetZ = centerZ + dz;

                // 3. 安全检查：确保坐标在网格范围内，防止索引越界
                if (targetX >= 0 && targetX < gg.width && targetZ >= 0 && targetZ < gg.depth)
                {
                    var neighbor = gg.GetNode(targetX, targetZ);
                    if (neighbor != null)
                    {
                        if (walkableNodes.Contains(neighbor))
                        {
                            neighbors.Add(neighbor);
                        }
                    }
                }
            }
        }
        return neighbors;
    }

    /// <summary>
    /// 判断起点到终点之间是否有不能寻路的点？
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private bool IsLineOfSightClear(Vector3 start, Vector3 end)
    {
        // 使用 A* 自带的射线检测（检测节点是否 Walkable）
        if (AstarPath.active.data.gridGraph.Linecast(start, end, out GraphHitInfo hit))
        {
            // 如果 Linecast 返回 true，说明路径上有不可通行的节点（墙）
            return false;
        }
        return true;
    }
}