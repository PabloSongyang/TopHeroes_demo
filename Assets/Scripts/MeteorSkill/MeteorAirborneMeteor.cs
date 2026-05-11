using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 流星空降式
/// </summary>
public class MeteorAirborneMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.流星空降式;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    private Coroutine m_GenerateMeteorSkillCoroutine;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity != null)
        {
            this.SpawnFireballSequence(monoBehaviour, attackEntity);
        }
    }

    /// <summary>
    /// 生成火流星序列
    /// </summary>
    /// <param name="monoBehaviour"></param>
    /// <param name="attackEntity"></param>
    private void SpawnFireballSequence(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity.EntityType == IEntity.Type.Enemy)
        {
            if (attackEntity.RenderObject.TryGetComponent<EnemyAI>(out EnemyAI enemyAI))
            {
                if (enemyAI.CurrentTarget != null && enemyAI.EnemySo.MeteorSkillSo != null)
                {
                    this.GenerateMeteorSkill(monoBehaviour, enemyAI.EnemySo.MeteorSkillSo, enemyAI.CurrentTarget.position, enemyAI.transform, Random.Range(.08f, .12f));
                }
            }
        }
        else if (attackEntity.EntityType == IEntity.Type.Player)
        {
            if (attackEntity.RenderObject.TryGetComponent<Player>(out Player player))
            {
                PlayerLevelInfo playerLevelInfo = player.PlayerSo.GetPlayerLevelInfoByLevel(player.CurrentLevel);
                RewardObj rewardObj = SWGameManager.Instance.HittedRewardObj_weapon.LastOrDefault();

                Debug.Log("释放火流星大招。。。");

                if (playerLevelInfo.MeteorSkillSo != null)
                {
                    this.GenerateMeteorSkill(monoBehaviour, playerLevelInfo.MeteorSkillSo, rewardObj.PolygonRange.transform.position, player.transform, Random.Range(.05f, .07f));
                }
            }
        }
    }

    /// <summary>
    /// 生成技能协程
    /// </summary>
    /// <param name="monoBehaviour"></param>
    /// <param name="meteorSkillSo"></param>
    /// <param name="targetPosition"></param>
    /// <param name="attacker"></param>
    /// <param name="itemInterval"></param>
    private void GenerateMeteorSkill(MonoBehaviour monoBehaviour, MeteorSkillSo meteorSkillSo, Vector3 targetPosition, Transform attacker, float itemInterval)
    {
        if (this.m_GenerateMeteorSkillCoroutine != null)
        {
            monoBehaviour.StopCoroutine(this.m_GenerateMeteorSkillCoroutine);
        }
        this.m_GenerateMeteorSkillCoroutine = monoBehaviour.StartCoroutine(this.GenerateMeteorSkill_Coroutine(meteorSkillSo, targetPosition, attacker, itemInterval));
    }

    private IEnumerator GenerateMeteorSkill_Coroutine(MeteorSkillSo meteorSkillSo, Vector3 targetPosition, Transform attacker, float itemInterval)
    {
        this.m_UltimateCompleted = false;
        List<MeteorSkill> cache = new List<MeteorSkill>();
        for (int i = 0; i < meteorSkillSo.EffectCount; i++)
        {
            Vector3 targetPos = targetPosition + Vector3.up * meteorSkillSo.HeightOffset + new Vector3(Random.Range(-meteorSkillSo.SpawnAreaSize.x / 2, meteorSkillSo.SpawnAreaSize.x / 2), Random.Range(-meteorSkillSo.SpawnAreaSize.y / 2, meteorSkillSo.SpawnAreaSize.y / 2), 0);
            GameObject MeteorSkillObj = PoolManager.Instance.EffectPool.Get(meteorSkillSo.Label, targetPos, Quaternion.identity);

            if (MeteorSkillObj.TryGetComponent<MeteorSkill>(out MeteorSkill meteorSkill))
            {
                meteorSkill.Setup(attacker, Vector2.down);
            }
            cache.Add(meteorSkill);
            yield return new WaitForSeconds(itemInterval);
        }

        yield return new WaitUntil(() =>
        {
            bool b = true;
            foreach (var item in cache)
            {
                b &= item.IsRecycle;
            }
            return b;
        });
        this.m_UltimateCompleted = true;
    }
}