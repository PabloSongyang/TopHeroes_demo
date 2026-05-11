using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 蓄力直线射击
/// </summary>
public class ChargingStraightLineShootingMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.蓄力直线射击;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    private Coroutine m_ShootCoroutine;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity.EntityType == IEntity.Type.Player)
        {
            if (attackEntity.RenderObject.TryGetComponent<Player>(out Player player))
            {
                PlayerLevelInfo playerLevelInfo = player.PlayerSo.GetPlayerLevelInfoByLevel(player.CurrentLevel);
                RewardObj rewardObj = SWGameManager.Instance.HittedRewardObj_weapon.LastOrDefault();

                Vector2 base_fireDirection = (rewardObj.PolygonRange.transform.position - player.transform.position).normalized;

                string bulletEffectLabel = playerLevelInfo.UltimateBulletEffectLabel;


    
                this.UltimateShoot(monoBehaviour, attackEntity, bulletEffectLabel, player, base_fireDirection);


                AudioManager.Instance.PlaySound(player.PlayerSo.GetSound("ChargingShooting"));
            }
        }
    }

    private void UltimateShoot(MonoBehaviour monoBehaviour, IEntity attackEntity, string bulletEffectLabel, Player player, Vector3 shootDirection)
    {
        if (this.m_ShootCoroutine != null)
        {
            monoBehaviour.StopCoroutine(this.m_ShootCoroutine);
        }
        this.m_ShootCoroutine = monoBehaviour.StartCoroutine(this.UltimateShootCoroutine(attackEntity, bulletEffectLabel, player, shootDirection));
    }

    private IEnumerator UltimateShootCoroutine(IEntity attackEntity, string bulletEffectLabel, Player player, Vector3 shootDirection)
    {
        this.m_UltimateCompleted = false;
        int bulletCount = 3;
        float angleStep = 40f;

        List<GameObject> bullets = new List<GameObject>();

        for (int i = 0; i < bulletCount; i++)
        {
            float startAngle = -(i - 1) * angleStep / 2f;

            GameObject bulletObjItem = PoolManager.Instance.BulletPool.Get(bulletEffectLabel, player.transform.position, Quaternion.identity);

            bulletObjItem.transform.position = player.PlayerAutoAttack.HandPos.position;

            Vector3 sss = Quaternion.Euler(new Vector3(0, 0, startAngle)) * shootDirection;

            // 4. 获取子弹身上的脚本并初始化
            if (bulletObjItem.TryGetComponent<Bullet>(out var bulletScriptItem))
            {
                // 调用子弹的 Setup 方法，把方向传过去
                bulletScriptItem.AddDamage = player.PlayerAutoAttack.AddDamage;
                bulletScriptItem.Level = player.CurrentLevel;
                bulletScriptItem.Setup(player, sss, true);

                bullets.Add(bulletObjItem);
            }
        }

        yield return new WaitUntil(() =>
        {
            bool b = true;

            foreach (var item in bullets)
            {
                b = Vector3.Distance(item.transform.position, player.transform.position) > 15;
            }
            return b;
        });
        this.m_UltimateCompleted = true;
    }
}