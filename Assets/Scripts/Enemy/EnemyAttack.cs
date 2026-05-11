using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform ShootPos => this.m_ShootPos;

    [SerializeField]
    private SpineAnimationEvent m_SpineAnimationEvent;

    [SerializeField]
    private Transform m_ShootPos;

    [SerializeField]
    private EnemyAI m_EnemyAI;

    public int AddDamage;
    public void BindingEvent()
    {
        this.m_SpineAnimationEvent.EnemyAttackEvent += this.EnemyAttackEvent;
    }

    private void EnemyAttackEvent(int attackIndex)
    {
        switch (this.m_EnemyAI.AiType)
        {
            case AIType.Enemy:
                if (this.m_EnemyAI.EnemySo.IsBoss)
                {
                    switch (attackIndex)
                    {
                        case 1:
                            this.AA();
                            break;
                        case 2:
                            IMeteorSkill meteorSkill = this.m_EnemyAI.IMeteorSkillDic.Get(this.m_EnemyAI.EnemySo.MeteorSkillTypes[0]);
                            meteorSkill?.Ultimate(this, this.m_EnemyAI);
                            break;
                        case 3:
                            meteorSkill = this.m_EnemyAI.IMeteorSkillDic.Get(this.m_EnemyAI.EnemySo.MeteorSkillTypes[1]);
                            meteorSkill?.Ultimate(this, this.m_EnemyAI);
                            break;
                    }
                }
                else
                {
                    this.AA();
                }
                break;

            case AIType.Servant:
                switch (attackIndex)
                {
                    case 1:
                        this.AA();
                        break;
                    case 2:
                        IMeteorSkill meteorSkill = this.m_EnemyAI.IMeteorSkillDic.Get(this.m_EnemyAI.EnemySo.MeteorSkillTypes[0]);
                        meteorSkill?.Ultimate(this, this.m_EnemyAI);
                        break;
                }
                break;
        }

        //if (this.m_EnemyAI.SpineSlotAttachmentDic.Count() > 0)
        //{
        //    List<SpineSlotAttachment> spineSlotAttachmentList = this.m_EnemyAI.SpineSlotAttachmentDic.Get("AttackIndex_" + attackIndex);
        //    SpineManager.Instance.SetSpecificAttachmentVisibility(this.m_EnemyAI.SpineComponent, spineSlotAttachmentList);
        //}
    }

    /// <summary>
    /// 环形检测打击目标
    /// </summary>
    /// <returns></returns>
    private bool HasRaycastTarget(out Collider2D hitPlayer)
    {
        hitPlayer = Physics2D.OverlapCircle(this.m_EnemyAI.transform.position, this.m_EnemyAI.EnemySo.AttackRange, this.m_EnemyAI.EnemySo.TargetLayer);
        return hitPlayer != null;
    }

    /// <summary>
    /// 平A
    /// </summary>
    private void AA()
    {
        if (this.m_ShootPos == null)
        {
            if (this.HasRaycastTarget(out Collider2D hit))
            {
                Debug.Log("打中目标了！");

                if (this.m_EnemyAI.CurrentTarget != null)
                {
                    IDamage iDamage = this.m_EnemyAI.CurrentTarget.GetComponentInChildren<IDamage>();
                    iDamage.TakeDamage(iDamage.SelfEntity, this.m_EnemyAI.EnemyHealth.Damage, hit.transform.position, Vector2.zero, 0, this.m_EnemyAI.EnemySo.AASoundInfo);
                }
            }
        }
        else
        {
            this.Shoot(this.m_EnemyAI.CurrentTarget);
        }
    }



    private void Shoot(Transform target)
    {
        // 1. 安全检查：确保对象池管理器已经存在
        if (PoolManager.Instance == null || PoolManager.Instance.BulletPool == null || target == null) return;

        // 2. 从对象池取出子弹
        // 参数1：发射起点（主角位置）
        // 参数2：初始旋转（2D游戏通常用 Quaternion.identity，方向由速度决定）


        Vector2 base_fireDirection = (target.position - this.m_ShootPos.position).normalized;

        GameObject bulletObj = PoolManager.Instance.BulletPool.Get(this.m_EnemyAI.EnemySo.BulletLabel, this.m_EnemyAI.transform.position, Quaternion.identity);

        bulletObj.transform.position = this.m_ShootPos.position;
        // 3. 计算发射方向：(目标点坐标 - 起点坐标) 的归一化向量

        // 4. 获取子弹身上的脚本并初始化
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // 调用子弹的 Setup 方法，把方向传过去
            bulletScript.AddDamage = AddDamage;
            bulletScript.Level = 1;
            bulletScript.Setup(this.m_EnemyAI, base_fireDirection, false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (this.m_EnemyAI.EnemySo.MeteorSkillSo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, this.m_EnemyAI.EnemySo.MeteorSkillSo.UltimateRadius);
        }
    }
#endif
}