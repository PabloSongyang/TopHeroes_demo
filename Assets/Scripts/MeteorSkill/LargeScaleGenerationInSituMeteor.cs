using UnityEngine;

/// <summary>
/// 原地大范围生成式
/// </summary>
public class LargeScaleGenerationInSituMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.原地大范围生成式;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity == null) return;

        Vector3 releasePosition = attackEntity.RenderObject.transform.position;

        if (attackEntity.EntityType == IEntity.Type.Enemy)
        {
            if (attackEntity.RenderObject.TryGetComponent<EnemyAI>(out EnemyAI servant) && servant.AiType == AIType.Servant)
            {
                if (servant.EnemySo.MeteorSkillSo.IsUltimateReleaseTargetPosition)
                {
                    if (servant.CurrentTarget != null)
                    {
                        releasePosition = servant.CurrentTarget.transform.position;
                    }
                }
                else
                {
                    releasePosition = servant.transform.position;
                }

                this.m_UltimateCompleted = false;
                // 1. 找到半径 3 米内的所有碰撞体
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(releasePosition, servant.EnemySo.MeteorSkillSo.UltimateRadius, servant.EnemySo.TargetLayer);

                foreach (var hit in hitColliders)
                {
                    // 2. 计算距离
                    float distance = Vector3.Distance(releasePosition, hit.transform.position);

                    // 3. 计算伤害衰减比例 (0 到 1 之间)
                    // 距离越近，damagePercent 越接近 1；距离为 3 时，为 0
                    float damagePercent = 1f - (distance / servant.EnemySo.MeteorSkillSo.UltimateRadius);
                    damagePercent = Mathf.Clamp01(damagePercent); // 确保不会出现负数

                    // 4. 计算最终伤害值
                    float finalDamage = servant.EnemySo.MeteorSkillSo.Damage * damagePercent;

                    IDamage iDamage = hit.GetComponentInChildren<IDamage>();
                    iDamage.TakeDamage(iDamage.SelfEntity, Mathf.RoundToInt(finalDamage), hit.transform.position, Vector2.zero, 0, servant.EnemySo.MeteorSkillSo.HitSoundInfo);

                    Debug.Log($"命中敌人: {hit.name}, 距离: {distance:F2}m, 造成伤害: {finalDamage:F1}");
                }

                PoolManager.Instance.EffectPool.Get(servant.EnemySo.MeteorSkillSo.Label, servant.transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound(servant.EnemySo.MeteorSkillSo.HitSoundInfo);
                this.m_UltimateCompleted = true;
            }
        }
    }
}