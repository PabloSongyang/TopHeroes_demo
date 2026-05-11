using UnityEngine;

/// <summary>
/// 攻击者旋转射击
/// </summary>
public class AttackerRotateShootingMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.攻击者旋转射击;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        if (attackEntity.EntityType == IEntity.Type.Player)
        {
            if (attackEntity.RenderObject.TryGetComponent<Player>(out Player player))
            {
                this.m_UltimateCompleted = false;

                PlayerLevelInfo playerLevelInfo = player.PlayerSo.GetPlayerLevelInfoByLevel(player.CurrentLevel);
                PolygonRange polygonRange = SWGameManager.Instance.EnemyCreatePolygonRangeDic.Get(playerLevelInfo.EnemyCreatePolygonRangeName);

                Vector2 base_fireDirection = (polygonRange.transform.position - player.transform.position).normalized;

                string bulletEffectLabel = playerLevelInfo.BulletEffectLabel;

                player.PlayerAutoAttack.FireRotate.Shoot(SWGameManager.Instance.BossRange.CurrentBoss, () => this.Shoot(player, playerLevelInfo), () => this.m_UltimateCompleted = true);
            }
        }
    }

    private void Shoot(Player player, PlayerLevelInfo playerLevelInfo)
    {
        player.IsCanMove = false;
        int bulletCount = 5;
        float angleStep = 40f;
        for (int i = 0; i < bulletCount; i++)
        {
            float startAngle = -(i - 1) * angleStep / 2f;
            GameObject bulletObjItem = PoolManager.Instance.BulletPool.Get(playerLevelInfo.BulletEffectLabel, player.transform.position, Quaternion.identity);

            bulletObjItem.transform.position = player.PlayerAutoAttack.HandPos.position;

            Vector3 sss = Quaternion.Euler(new Vector3(0, 0, startAngle)) * player.PlayerAutoAttack.FireRotate.transform.right;

            // 4. 获取子弹身上的脚本并初始化
            Bullet bulletScriptItem = bulletObjItem.GetComponent<Bullet>();
            if (bulletScriptItem != null)
            {
                // 调用子弹的 Setup 方法，把方向传过去
                bulletScriptItem.AddDamage = playerLevelInfo.MeteorSkillSo.Damage;
                bulletScriptItem.Level = player.CurrentLevel;
                bulletScriptItem.Setup(player, sss, false);
            }
        }

        AudioManager.Instance.PlaySound(playerLevelInfo.MeteorSkillSo.HitSoundInfo.AudioClip);
    }
}