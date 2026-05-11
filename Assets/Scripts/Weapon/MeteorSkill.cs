using System.Collections;
using UnityEngine;

public class MeteorSkill : MonoBehaviour, IPoolElement
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private MeteorSkillSo m_MeteorSkillSo;

    public int AddDamage;
    public int Level;

    [SerializeField]
    private BoxCollider2D m_BoxCollider2D;

    private Transform attacker;

    private string m_ImpactEffectLabel;


    private bool m_IsRecycle;

    private Coroutine m_RecycleCoroutine;

    public GameObject RenderObject => this.gameObject;

    public bool IsRecycle => this.m_IsRecycle;


    public void Setup(Transform attacker, Vector2 direction, bool isPointToTarget = false)
    {
        this.m_IsRecycle = false;
        this.attacker = attacker;
        this.m_ImpactEffectLabel = this.m_MeteorSkillSo.HitEffectLabel;

        // 给刚体一个瞬时速度
        rb.velocity = direction * this.m_MeteorSkillSo.Speed;

        if (isPointToTarget)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

    }

    private void OnEnable()
    {
        this.m_RecycleCoroutine =  base.StartCoroutine(this.Recycle_delay());
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.CompareTag("Ground"))
        //{
        //    Recycle();
        //    this.AOEDamage(other.transform.position);
        //}
    }

    private void AOEDamage(Vector3 hitPosition)
    {
        // 1. 获取爆炸中心点
        Vector2 explosionPos = hitPosition;
        float radius = this.m_MeteorSkillSo.AOERadius;

        // 2. 核心：检测半径内的所有碰撞体
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(explosionPos, radius, this.m_MeteorSkillSo.HitLayer);

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
                int finalDamage = Mathf.RoundToInt((m_MeteorSkillSo.Damage + AddDamage) * damageMultiplier);

                Vector2 dir = (col.transform.position - transform.position).normalized;
                iDamage.TakeDamage(iDamage.SelfEntity, finalDamage, explosionPos, dir, this.m_MeteorSkillSo.BeatBackDistance, this.m_MeteorSkillSo.HitSoundInfo, this.m_MeteorSkillSo.HitEffectLabel);
            }
        }
    }

    //private void LateUpdate()
    //{
    //    if (this.attacker != null && this.attacker.CompareTag("Player"))
    //    {
    //        this.m_BoxCollider2D.enabled = Vector2.Distance(this.attacker.transform.position, this.transform.position) <= 5;
    //    }
    //}

    private IEnumerator Recycle_delay()
    {
        yield return new WaitForSeconds(this.m_MeteorSkillSo.LeftTime);
        if (this.gameObject.activeSelf)
        {
            this.Recycle();
        }
    }

    void Recycle()
    {
        if (this.m_RecycleCoroutine != null)
            StopCoroutine(this.m_RecycleCoroutine);

        SWGameManager.Instance.CinemachineImpulseSource.GenerateImpulse();
        this.AOEDamage(transform.position);
        PoolManager.Instance.EffectPool.Get(this.m_ImpactEffectLabel, transform.position, Quaternion.identity);
        PoolManager.Instance.EffectPool.Release(this.m_MeteorSkillSo.Label, gameObject);

        this.m_IsRecycle = true;
    }

    private void OnDisable()
    {
        base.StopCoroutine(this.m_RecycleCoroutine);
        //StopAllCoroutines();
    }

    //private void OnBecameInvisible()
    //{
    //    // 只有在物体处于激活状态时才回收，防止重复触发
    //    if (gameObject.activeSelf)
    //    {
    //        Recycle();
    //    }
    //}
}
