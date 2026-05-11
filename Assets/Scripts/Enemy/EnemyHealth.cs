using DG.Tweening;
using NUnit.Framework.Interfaces;
using PabloFramework;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamage
{
    public int health = 100;
    private int m_Damage;

    [SerializeField]
    private Transform m_BaseNode;

    private MaterialPropertyBlock block;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private EnemyAI m_EnemyAI;

    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private SpineAnimationEvent m_SpineAnimationEvent;

    [SerializeField]
    private StatusBar m_HealthBar;

    [SerializeField]
    private int m_HitMaterialIndex;

    [SerializeField]
    private bool m_Dead;

    private AIPath m_AIPath;

    private int m_EntityID;

    private bool m_IsInitMaterial;

    public int Damage => this.m_Damage;

    public bool Dead => this.m_Dead;

    public IEntity SelfEntity => this.m_EnemyAI;

    private Coroutine m_hitFlashCoroutine;

    private BindableProperty<int> m_HealthBP = new BindableProperty<int>() { Value = 0 };

    void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    public void Init(IAstarAI astarAI, int entityID)
    {
        this.m_EntityID = entityID;
        this.m_AIPath = (AIPath)astarAI;
        this.m_AIPath.enabled = true;
        this.m_Dead = false;
        health = this.m_EnemyAI.EnemySo.HP;
        this.m_Damage = this.m_EnemyAI.EnemySo.Damage;
        this.m_Animator.SetBool("IsDead", false);
        this.m_Animator.ResetTrigger("Dead");

        this.m_IsInitMaterial = true;

    }


    public void BindingEvent()
    {
        this.m_HealthBP.OnValueChanged += this.OnHealthValueChanged;
        this.m_SpineAnimationEvent.EnemyDeadEndEvent += this.EnemyDeadEndEvent;
    }



    public void TakeDamage(IEntity target, int damage, Vector2 hitPosition, Vector2 hitDirection, float beatBackDis, SoundInfo hitSoundInfo, string hitEffectLabel = null)
    {
        if (this.m_EnemyAI.EnemySo.IsBoss && !SWGameManager.Instance.CurrentPlayer.IsActiveBoss)
        {
            return;
        }

        health -= damage;

        if (health < 0)
        {
            health = 0;
            this.m_Dead = true;
            this.PlayDeadAnimation();
            return;
        }



        // 通知 UI 脚本更新
        if (m_HealthBar != null)
        {
            m_HealthBar.UpdateHealth(health, this.m_EnemyAI.EnemySo.HP);
        }

        this.DoHitAction(hitPosition, hitDirection, beatBackDis, hitSoundInfo, hitEffectLabel);
    }

    private void OnHealthValueChanged(int health)
    {
        //if (health <= 0)
        //{
        //    this.EnemyDeadEndEvent();
        //}
    }

    /// <summary>
    /// 受击逻辑
    /// </summary>
    private void DoHitAction(Vector2 hitPosition, Vector2 hitDirection, float beatBackDis, SoundInfo hitSoundInfo, string hitEffectLabel)
    {
        // 1. 播放受击动画 (如果你的Animator里有"Hurt"状态)
        if (this.m_EnemyAI.EnemySo.HitFeedback)
            this.m_Animator.SetTrigger("Hit");

        // 2. 视觉反馈：闪红
        if (this.m_hitFlashCoroutine != null)
        {
            base.StopCoroutine(this.m_hitFlashCoroutine);
        }
        this.m_hitFlashCoroutine = StartCoroutine(HitFlash(.3f));

        this.ShowHitEffect(hitPosition, hitSoundInfo, hitEffectLabel);
        if (this.m_EnemyAI.EnemySo.HitFeedback)
        {
            this.HitBack(hitDirection, beatBackDis);
        }
        // 3. 物理反馈：微小后退 (Knockback)
        //Vector3 targetPosition = this.m_BaseNode.position + (Vector3)(hitDirection * beatBackDis);

        //// 使用 DOMove 移动到目标位置
        //// 参数：目标点，持续时间（如 0.1s），是否对齐到整数像素（false）
        //this.m_BaseNode.DOMove(targetPosition, 0.15f)
        //    .SetEase(Ease.OutQuad) // 设置缓动：先快后慢，模拟撞击感
        //    .SetTarget(this.m_BaseNode); // 设置引用，方便在对象销毁时自动杀掉动画
    }

    private void ShowHitEffect(Vector2 hitPosition, SoundInfo hitSound, string hitEffectLabel)
    {
        AudioManager.Instance.PlaySound(hitSound);
        if (!string.IsNullOrEmpty(hitEffectLabel))
        {
            GameObject effectGo = PoolManager.Instance.EffectPool.Get(hitEffectLabel, hitPosition, Quaternion.identity);

            if (effectGo != null && effectGo.TryGetComponent<WeaponEffect>(out WeaponEffect weaponEffect))
            {
                weaponEffect.Animator.Rebind();
                weaponEffect.Animator.Play("Init", 0, 0f);
            }
        }
    }

    private IEnumerator HitFlash(float duration)
    {
        this.SetOutlineReferenceTexWidth(1024);

        yield return new WaitForSeconds(duration);
        this.SetOutlineReferenceTexWidth(0);
        //block.SetColor("_Color", new Color(1, 1, 1, 0)); // 恢复透明/原色
        //meshRenderer.SetPropertyBlock(block);


    }

    private void SetOutlineReferenceTexWidth(int value)
    {
        if (meshRenderer == null) return;

        Material mat = null;
        if (meshRenderer.sharedMaterials.Length > this.m_HitMaterialIndex)
        {
            mat = meshRenderer.sharedMaterials[this.m_HitMaterialIndex];
        }
        else
        {
            if (meshRenderer.sharedMaterials.Length > 0)
            {
                mat = meshRenderer.sharedMaterials[0];
            }
        }

        string propName = "_OutlineReferenceTexWidth";
        mat?.SetInt(propName, value);
    }

    private void HitBack(Vector2 hitDirection, float beatBackDis)
    {
        Vector2 origin = this.m_BaseNode.position;
        float checkDistance = beatBackDis;
        // 假设墙体的 Layer 是 "Wall"
        int layerMask = LayerMask.GetMask("Obstacle");

        // 发射射线探测
        RaycastHit2D hit = Physics2D.Raycast(origin, hitDirection, checkDistance, layerMask);

        Vector3 finalTarget;
        if (hit.collider != null)
        {
            // 如果撞到墙了，目标点设为撞击点（稍微缩进一点点防止卡进墙里）
            finalTarget = hit.point - (hitDirection * 0.1f);
        }
        else
        {
            // 没撞到墙，正常后退
            finalTarget = origin + (hitDirection * beatBackDis);
        }

        this.m_BaseNode.DOMove(finalTarget, 0.15f).SetEase(Ease.OutQuad);
    }

    void PlayDeadAnimation()
    {
        if (m_HealthBar != null) m_HealthBar.HideImmediately();
        this.m_AIPath.enabled = false;
        this.m_Animator.SetTrigger("Dead");
        this.m_Animator.SetBool("IsDead", true);
    }

    private void EnemyDeadEndEvent()
    {
        PoolManager.Instance.EnemyPool.Release(this.m_EnemyAI.EnemySo.ObjectPoolLabel, this.m_BaseNode.gameObject);
        SWGameManager.Instance.EnemiesAIDic.Remove(this.m_EntityID);

        if (this.m_EnemyAI.EnemySo.IsBoss)
        {
            SWGameManager.Instance.OnPlayerWinEvent.Send();
            SWGameManager.Instance.IsWin = true;
            AudioManager.Instance.StopBGM();
        }
    }

    public void OnUpdate()
    {
        this.m_HealthBP.Value = health;

        this.m_HealthBar.transform.localScale = new Vector3(this.m_BaseNode.localScale.x, 1, 1);

        if (meshRenderer != null && meshRenderer.sharedMaterials.Length > 0)
        {
            if (this.m_IsInitMaterial)
            {
                this.m_IsInitMaterial = false;
                this.SetOutlineReferenceTexWidth(0);
            }
        }
    }

    private void OnDisable()
    {
        this.m_HealthBP.OnValueChanged -= this.OnHealthValueChanged;
    }
}
