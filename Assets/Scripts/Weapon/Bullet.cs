using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolElement
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private BulletSo m_BulletSo;

    public int AddDamage;
    public int Level;

    [SerializeField]
    private BoxCollider2D m_BoxCollider2D;

    private HashSet<int> m_HitEnemyIds = new HashSet<int>();

    private IEntity attackerEntity;

    private bool m_IsUltimate;

    private Coroutine m_RecycleCoroutine;

    public GameObject RenderObject => this.gameObject;

    public void Setup(IEntity entity, Vector2 direction, bool isUltimate)
    {
        this.attackerEntity = entity;
        this.m_IsUltimate = isUltimate;
        m_HitEnemyIds.Clear();

        // 给刚体一个瞬时速度
        rb.velocity = direction * this.m_BulletSo.Speed;

        // 子弹飞行时通常需要旋转图片指向目标（可选）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

    }

    private void OnEnable()
    {
        this.m_RecycleCoroutine = base.StartCoroutine(this.Recycle_delay());
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        var entityType = this.attackerEntity.EntityType;

        Player player = null;
        EnemyAI enemyAI = null;
        switch (entityType)
        {
            case IEntity.Type.Player:
                player = this.attackerEntity as Player;
                break;
            case IEntity.Type.Enemy:
                enemyAI = this.attackerEntity as EnemyAI;
                break;
        }


        if (this.m_BulletSo.IsHitRewardBox)
        {
            if (other.CompareTag("Reward") && entityType == IEntity.Type.Player)
            {

                RewardObj rewardObj = other.GetComponentInParent<RewardObj>();
                if (rewardObj != null)
                {
                    rewardObj.DoHit();
                }

                Recycle();
            }
        }


        if (other.CompareTag("Enemy") && (enemyAI != null && !enemyAI.EnemySo.IsBoss || player != null))
        {
            this.DoDamage(other);
        }
        else if (other.CompareTag("Player") && enemyAI != null)
        {
            if (enemyAI.EnemySo.IsBoss)
            {
                this.DoDamage(other);
            }
        }
    }

    private void DoDamage(Collider2D other)
    {
        if (this.m_BulletSo.AOERadius > 0)
        {
            this.AOEDamage(other.transform.position);
        }
        else
        {
            this.SingleDamage(other);
        }
    }


    private void SingleDamage(Collider2D other)
    {
        int enemyId = other.gameObject.GetInstanceID();

        // 如果这个敌人还没被这颗子弹打过
        if (!m_HitEnemyIds.Contains(enemyId))
        {
            m_HitEnemyIds.Add(enemyId); // 记录该敌人

            Vector2 dir = transform.right;

            // 执行伤害
            IDamage iDamage = other.GetComponentInChildren<IDamage>();
            if (iDamage != null)
            {
                iDamage.TakeDamage(iDamage.SelfEntity, this.m_BulletSo.Damage + AddDamage, other.transform.position, dir, this.m_BulletSo.BeatBackDistance, this.m_BulletSo.HitSoundInfo, this.m_BulletSo.HitEffectLabel);
            }

            if (!this.m_BulletSo.IsPenetrate)
            {
                this.Recycle();
            }
            // 【关键改动】这里不再调用 Recycle()，子弹会继续飞行
        }
    }

    private void AOEDamage(Vector3 hitPosition)
    {
        // 1. 获取爆炸中心点
        Vector2 explosionPos = hitPosition;
        float radius = this.m_BulletSo.AOERadius;

        // 2. 核心：检测半径内的所有碰撞体
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(explosionPos, radius, this.m_BulletSo.HitLayer);

        foreach (var col in hitEnemies)
        {
            IDamage iDamage = col.GetComponentInChildren<IDamage>();
            if (iDamage != null)
            {
                // 3. 计算伤害衰减
                float distance = Vector2.Distance(explosionPos, col.transform.position);
                // 衰减比例：离中心越近伤害越高（1.0），边缘伤害最低（例如0.2）
                float damageMultiplier = Mathf.Clamp01(1 - (distance / radius));

                // 实际伤害 = 基础伤害 * 衰减系数
                int finalDamage = Mathf.RoundToInt((m_BulletSo.Damage + AddDamage) * damageMultiplier);

                Vector2 dir = (col.transform.position - transform.position).normalized;
                iDamage.TakeDamage(iDamage.SelfEntity, finalDamage, explosionPos, dir, m_BulletSo.BeatBackDistance, this.m_BulletSo.HitSoundInfo, this.m_BulletSo.HitEffectLabel);
            }
        }
        if (!this.m_BulletSo.IsPenetrate)
        {
            this.Recycle();
        }
    }

    private void LateUpdate()
    {
        if (this.attackerEntity != null && this.attackerEntity.EntityType == IEntity.Type.Player && !this.m_IsUltimate)
        {
            this.m_BoxCollider2D.enabled = Vector2.Distance(this.attackerEntity.RenderObject.transform.position, this.transform.position) <= 7;
        }
    }


    private IEnumerator Recycle_delay()
    {
        yield return new WaitForSeconds(this.m_BulletSo.LeftTime);

        if (this.gameObject.activeSelf)
        {
            Recycle();
        }

    }

    private void Recycle()
    {
        if (this.m_RecycleCoroutine != null)
            StopCoroutine(this.m_RecycleCoroutine);

        PoolManager.Instance.BulletPool.Release(this.m_BulletSo.Label, gameObject);
    }

    private void OnDisable()
    {
        base.StopCoroutine(this.m_RecycleCoroutine);
    }

    private void OnBecameInvisible()
    {
        // 只有在物体处于激活状态时才回收，防止重复触发
        if (gameObject.activeSelf)
        {
            Recycle();
        }
    }
}