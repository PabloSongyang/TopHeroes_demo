using UnityEngine;

/// <summary>
/// 多目标原地生成式
/// </summary>
public class MultiTargetIn_SituGenerationFormulaMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.多目标原地生成式;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity != null)
            this.SpawnEffectOnTargets(monoBehaviour, attackEntity);
    }

    public void SpawnEffectOnTargets(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity.EntityType == IEntity.Type.Enemy)
        {
            if (attackEntity.RenderObject.TryGetComponent<EnemyAI>(out EnemyAI enemyAI))
            {
                if (enemyAI.AiType == AIType.Servant)
                {
                    this.m_UltimateCompleted = false;
                    EnemyAI servant = enemyAI;
                    // 1. 使用 2D 圆形重叠检测
                    // 这会返回半径 7 米内所有带有 Collider2D 的物体
                    Collider2D[] hitTargets = Physics2D.OverlapCircleAll(monoBehaviour.transform.position, servant.EnemySo.MeteorSkillSo.AOERadius, servant.EnemySo.MeteorSkillSo.HitLayer);

                    foreach (var target in hitTargets)
                    {
                        if (target != null)
                        {
                            EnemyAI targetAI = target.GetComponentInParent<EnemyAI>();
                            if (targetAI.CompareTag("Enemy"))
                            {
                                if (target.TryGetComponent<IDamage>(out IDamage damage))
                                {
                                    GameObject MeteorSkillObj = PoolManager.Instance.EffectPool.Get(servant.EnemySo.MeteorSkillSo.Label, target.transform.position, Quaternion.identity);
                                    AudioManager.Instance.PlaySound(servant.EnemySo.MeteorSkillSo.HitSoundInfo);
                                    damage.TakeDamage(damage.SelfEntity, Mathf.RoundToInt(servant.EnemySo.MeteorSkillSo.Damage), target.transform.position, Vector2.zero, 0, null);
                                }
                            }
                        }
                        Debug.Log("2D检测到目标: " + target.name);
                    }
                    SWGameManager.Instance.CinemachineImpulseSource.GenerateImpulse();
                    this.m_UltimateCompleted = true;
                }
            }
        }
    }
}