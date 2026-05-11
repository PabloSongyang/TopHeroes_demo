using Spine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public float attackRange = 8f;      // 搜索范围
    public float fireRate = 0.5f;       // 攻击间隔
    public LayerMask targetLayer;        // 敌人所在的图层

    private float nextFireTime;
    private Transform currentTarget;
    private float scanInterval = 0.2f;  // 每0.2秒搜寻一次目标，节省CPU
    private float nextScanTime;

    [SerializeField]
    private Transform m_HandPos;

    [SerializeField]
    private FireRotate m_FireRotate;

    [SerializeField]
    private Player m_Player;

    public Transform CurrentTarget => this.currentTarget;
    public Transform HandPos => this.m_HandPos;
    public FireRotate FireRotate => this.m_FireRotate;

    public int AddDamage;

    private float m_ChargeUpTimer;

    private int m_ChargeUpType;
    private bool m_IsChargeUp;

    private bool m_IsAttackEnd;


    /// <summary>
    /// 当前释放的大招
    /// </summary>
    private IMeteorSkill m_CurrentIMeteorSkill;
    public void Init()
    {
        foreach (var item in this.m_Player.SpineAnimationEvents)
        {
            item.PlayerAttackReadyEvent += this.AttackReadyEvent;
            item.PlayerAttackEvent += this.AttackEvent;
            item.PlayerAttackChargingUpEvent += this.AttackChargingUpEvent;
            item.PlayerAttackEndEvent += this.AttackEndEvent;
        }


        this.m_IsAttackEnd = true;
    }

    public void Attacking()
    {
        // 1. 性能优化：定时搜寻最近目标，而不是每帧都搜
        if (Time.time >= nextScanTime)
        {
            currentTarget = FindNearestEnemy(this.attackRange);
            nextScanTime = Time.time + scanInterval;
        }

        // 2. 检查是否有有效目标且攻击冷却完毕
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            // 如果目标跑出了范围，丢弃目标
            if (Vector2.Distance(transform.position, currentTarget.position) > attackRange)
            {
                currentTarget = null;
                return;
            }

            // 3. 执行射击
            if (Time.time >= nextFireTime)
            {
                if (!this.m_Player.IsStartChargingUp && (this.m_CurrentIMeteorSkill == null || (this.m_CurrentIMeteorSkill != null && this.m_CurrentIMeteorSkill.UltimateCompleted)))
                {
                    this.PlayAttackAnimation();
                }
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("AttackIndex", 0);
        }


        this.AttackingExtend();
    }

    /// <summary>
    /// 播放普攻动画
    /// </summary>
    public void PlayAttackAnimation()
    {
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetBool("IsChargeUp", false);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("AttackIndex", this.m_Player.CurrentLevel == 4 ? 2 : 1);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetTrigger("Attack");
    }

    /// <summary>
    /// 播放蓄力大招动画
    /// </summary>
    public void PlayAttackChargingUpAnimation()
    {
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("ChargeUpType", 1);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetBool("IsChargeUp", true);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("AttackIndex", this.m_Player.CurrentLevel == 4 ? 2 : 1);
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetTrigger("Attack");
    }

    private void AttackingExtend()
    {
        if (this.m_Player.IsStartChargingUp)
        {
            if (this.m_Player.PlayerSo.IsOpenChargeUp)
            {
                this.m_ChargeUpType = this.m_Player.CurrentSpineAnimationEvent.Animator.GetInteger("ChargeUpType");
                this.m_IsChargeUp = this.m_Player.CurrentSpineAnimationEvent.Animator.GetBool("IsChargeUp");

                if (this.m_IsChargeUp)
                {
                    switch (this.m_ChargeUpType)
                    {
                        case 0:

                            break;

                        case 1:
                            this.m_Player.CameraOrthographicSizeChangingLerp(this.m_Player.PlayerSo.ChargeUpCameraOrthographicSize);
                            this.m_ChargeUpTimer += Time.deltaTime;
                            if (this.m_ChargeUpTimer > this.m_Player.PlayerSo.ChargeUpTime)
                            {
                                this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("ChargeUpType", 2);
                                this.m_ChargeUpTimer = 0;

                            }
                            break;
                        case 2:
                            if (this.m_IsAttackEnd)
                            {
                                if (this.m_CurrentIMeteorSkill != null)
                                {
                                    if (this.m_CurrentIMeteorSkill.CurrentMeteorSkillType == IMeteorSkill.MeteorSkillType.攻击者旋转射击)
                                    {
                                        this.m_Player.IsCanMove = false;
                                    }
                                    else
                                    {
                                        this.m_Player.IsCanMove = true;
                                    }

                                    if (this.m_CurrentIMeteorSkill.UltimateCompleted)
                                    {
                                        Debug.Log("大招释放完毕。。。。。。。。");
                                        this.m_Player.CameraOrthographicSizeChangingLerp(this.m_Player.PlayerSo.DefaultCameraOrthographicSize, () =>
                                        {
                                            SWGameManager.Instance.OnPlayerChargedUpCompleteEvent.Send();
                                            this.m_Player.IsStartChargingUp = false;

                                            this.m_Player.CurrentSpineAnimationEvent.Animator.SetInteger("ChargeUpType", 0);
                                            this.m_Player.CurrentSpineAnimationEvent.Animator.SetBool("IsChargeUp", false);
                                        });
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
    }


    private void AttackReadyEvent(string soundName)
    {
        this.m_IsAttackEnd = false;
        AudioManager.Instance.PlaySound(this.m_Player.PlayerSo.GetSound(soundName));
    }

    private void AttackEvent(string soundName)
    {
        SWGameManager.Instance.CinemachineImpulseSource.GenerateImpulse();
        this.Shoot(this.currentTarget);
        AudioManager.Instance.PlaySound(this.m_Player.PlayerSo.GetSound(soundName));
    }

    private void AttackChargingUpEvent(string soundName)
    {
        SWGameManager.Instance.CinemachineImpulseSource.GenerateImpulse();
        this.ShootUltimate();
        AudioManager.Instance.PlaySound(this.m_Player.PlayerSo.GetSound(soundName));
    }

    private void AttackEndEvent(int key)
    {
        this.m_IsAttackEnd = true;
        this.m_Player.ChargeUpEffect.SetActive(false);
    }

    private Transform FindNearestEnemy(float radius)
    {
        // 物理层检测（记得给怪物设置 Enemy 层）
        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        Transform nearestEnemy = null;
        float minEnemyDist = Mathf.Infinity;

        Transform nearestReward = null;
        float minRewardDist = Mathf.Infinity;

        foreach (var col in targetColliders)
        {
            EnemyAI target = col.GetComponentInParent<EnemyAI>();
            if (target != null && target.EnemySo.IsBoss && !SWGameManager.Instance.CurrentPlayer.IsActiveBoss)
            {
                continue;
            }

            float dist = Vector2.Distance(transform.position, col.transform.position);

            // 优先逻辑：如果是敌人
            //if (col.CompareTag("Enemy"))
            {
                if (dist < minEnemyDist)
                {
                    minEnemyDist = dist;
                    nearestEnemy = col.transform;
                }
            }
            // 次要逻辑：如果是奖励宝箱（仅在没确定有敌人时有意义，但这里先记录下来）
            //else if (col.CompareTag("Reward"))
            //{
            //    if (dist < minRewardDist)
            //    {
            //        minRewardDist = dist;
            //        nearestReward = col.transform;
            //    }
            //}
        }

        // 最后的判断：只要有敌人，就回传最近的敌人；否则回传最近的奖励
        return nearestEnemy != null ? nearestEnemy : nearestReward;
    }

    private void Shoot(Transform target)
    {
        // 1. 安全检查：确保对象池管理器已经存在
        if (PoolManager.Instance == null || PoolManager.Instance.BulletPool == null || target == null) return;

        // 2. 从对象池取出子弹
        // 参数1：发射起点（主角位置）
        // 参数2：初始旋转（2D游戏通常用 Quaternion.identity，方向由速度决定）

        int bulletCount = 1;
        float angleStep = 0f;


        PlayerLevelInfo playerLevelInfo = this.m_Player.PlayerSo.GetPlayerLevelInfoByLevel(this.m_Player.CurrentLevel);

        Vector2 base_fireDirection = (target.position - transform.position).normalized;

        string bulletEffectLabel = playerLevelInfo.BulletEffectLabel;

        switch (this.m_Player.CurrentLevel)
        {
            case 1:
                GameObject bulletObj = PoolManager.Instance.BulletPool.Get(bulletEffectLabel, transform.position, Quaternion.identity);

                bulletObj.transform.position = this.m_HandPos.position;
                // 3. 计算发射方向：(目标点坐标 - 起点坐标) 的归一化向量

                // 4. 获取子弹身上的脚本并初始化
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    // 调用子弹的 Setup 方法，把方向传过去
                    bulletScript.AddDamage = AddDamage;
                    bulletScript.Level = this.m_Player.CurrentLevel;
                    bulletScript.Setup(this.m_Player, base_fireDirection, false);
                }
                break;
            case 2:

                bulletCount = 3;
                angleStep = 40f;
                for (int i = 0; i < bulletCount; i++)
                {
                    float startAngle = -(i - 1) * angleStep / 2f;

                    GameObject bulletObjItem = PoolManager.Instance.BulletPool.Get(bulletEffectLabel, transform.position, Quaternion.identity);

                    bulletObjItem.transform.position = this.m_HandPos.position;

                    Vector3 sss = Quaternion.Euler(new Vector3(0, 0, startAngle)) * base_fireDirection;

                    // 4. 获取子弹身上的脚本并初始化
                    Bullet bulletScriptItem = bulletObjItem.GetComponent<Bullet>();
                    if (bulletScriptItem != null)
                    {
                        // 调用子弹的 Setup 方法，把方向传过去
                        bulletScriptItem.AddDamage = AddDamage;
                        bulletScriptItem.Level = this.m_Player.CurrentLevel;
                        bulletScriptItem.Setup(this.m_Player, sss, false);
                    }
                }
                break;
            case 3:
                bulletCount = 5;
                angleStep = 20f;
                for (int i = 0; i < bulletCount; i++)
                {
                    float startAngle = -(i - 1) * angleStep / 2f;
                    GameObject bulletObjItem = PoolManager.Instance.BulletPool.Get(bulletEffectLabel, transform.position, Quaternion.identity);

                    bulletObjItem.transform.position = this.m_HandPos.position;

                    Vector3 sss = Quaternion.Euler(new Vector3(0, 0, startAngle)) * base_fireDirection;

                    // 4. 获取子弹身上的脚本并初始化
                    Bullet bulletScriptItem = bulletObjItem.GetComponent<Bullet>();
                    if (bulletScriptItem != null)
                    {
                        // 调用子弹的 Setup 方法，把方向传过去
                        bulletScriptItem.AddDamage = AddDamage;
                        bulletScriptItem.Level = this.m_Player.CurrentLevel;
                        bulletScriptItem.Setup(this.m_Player, sss, false);
                    }
                }
                break;
            case 4:
                bulletCount = 7;
                angleStep = 20f;
                for (int i = 0; i < bulletCount; i++)
                {
                    float startAngle = -(i - 1) * angleStep / 2f;
                    GameObject bulletObjItem = PoolManager.Instance.BulletPool.Get(bulletEffectLabel, transform.position, Quaternion.identity);

                    bulletObjItem.transform.position = this.m_HandPos.position;

                    Vector3 sss = Quaternion.Euler(new Vector3(0, 0, startAngle)) * base_fireDirection;

                    // 4. 获取子弹身上的脚本并初始化
                    Bullet bulletScriptItem = bulletObjItem.GetComponent<Bullet>();
                    if (bulletScriptItem != null)
                    {
                        // 调用子弹的 Setup 方法，把方向传过去
                        bulletScriptItem.AddDamage = AddDamage;
                        bulletScriptItem.Level = this.m_Player.CurrentLevel;
                        bulletScriptItem.Setup(this.m_Player, sss, false);
                    }
                }
                break;
        }
    }

    private void ShootUltimate()
    {
        if (PoolManager.Instance == null || PoolManager.Instance.BulletPool == null || this.m_Player.IsDead) return;

        if (this.m_Player.CurrentLevel > 1)
        {
            PlayerLevelInfo playerLevelInfo = this.m_Player.PlayerSo.GetPlayerLevelInfoByLevel(this.m_Player.CurrentLevel);

            this.m_CurrentIMeteorSkill = this.m_Player.IMeteorSkillDic.Get(playerLevelInfo.CurrentMeteorSkillType);
            if (this.m_CurrentIMeteorSkill != null)
            {
                Debug.Log("玩家释放大招");
                this.m_CurrentIMeteorSkill.Ultimate(this, this.m_Player);
            }
        }

    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器里画一个圈，方便观察攻击范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
