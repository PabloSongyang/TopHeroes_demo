using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamage
{
    public float health = 100f;

    [SerializeField]
    private Transform m_BaseNode;

    [SerializeField]
    private StatusBar m_HealthBar;

    [SerializeField]
    private Player m_Player;

    [SerializeField]
    private GameObject m_QuanIcon;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private Image flashImage;
    [SerializeField] private float maxAlpha = 0.4f;    // 红屏最深时的透明度
    [SerializeField] private float flashDuration = 0.2f; // 红一下变亮的时间
    [SerializeField] private float fadeDuration = 0.5f;  // 变暗消失的时间

    private Coroutine flashRoutine;

    private bool m_IsInitMaterial;

    public int Damage => 0;

    public IEntity SelfEntity => this.m_Player;

    public void Init()
    {
        health = this.m_Player.PlayerSo.HP;
        this.flashImage.color = new Color(this.flashImage.color.r, flashImage.color.g, flashImage.color.b, 0);
        foreach (var item in this.m_Player.SpineAnimationEvents)
        {
            item.Animator.SetBool("IsDead", false);
        }
        this.m_QuanIcon.SetActive(true);
        this.m_IsInitMaterial = true;
    }


    public void TakeDamage(IEntity target, int damage, Vector2 hitPosition, Vector2 hitDirection, float beatBackDis, SoundInfo hitSoundInfo, string hitEffectLabel = null)
    {
        if (this.m_Player.IsStartChargingUp)
        {
            return;
        }

        Debug.Log("TakeDamage ==>> amount :: " + damage);
        health -= damage;

        if (health > this.m_Player.PlayerSo.HP)
        {
            health = this.m_Player.PlayerSo.HP;
        }

        if (health < 0)
        {
            health = 0;
            this.PlayDeadAnimation();
            SWGameManager.Instance.IsInit = false;
            SWGameManager.Instance.OnPlayerDeadEvent.Send();
            AudioManager.Instance.StopBGM();
            return;
        }


        Debug.Log("health  " + health);
        // 通知 UI 脚本更新
        if (m_HealthBar != null)
        {
            m_HealthBar.UpdateHealth(health, this.m_Player.PlayerSo.HP);
        }

        if (damage >= 0)
        {
            //this.m_Player.SpineAnimationEvent.Animator.SetTrigger("Hit");
            this.PlayerHitEffect(hitPosition, hitSoundInfo, hitEffectLabel);
            this.TriggerFlash();
        }
    }

    private void PlayDeadAnimation()
    {
        this.flashImage.color = new Color(this.flashImage.color.r, flashImage.color.g, flashImage.color.b, 0);
        if (m_HealthBar != null) m_HealthBar.HideImmediately();
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetTrigger("Dead");
        this.m_Player.CurrentSpineAnimationEvent.Animator.SetBool("IsDead", true);
        this.m_QuanIcon.SetActive(false);
    }

    /// <summary>
    /// 供外部（如 PlayerHealth）调用的接口
    /// </summary>
    private void TriggerFlash()
    {
        if (flashImage == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlash(.3f));
    }


    private void PlayerHitEffect(Vector2 hitPosition, SoundInfo hitSound, string hitEffectLabel)
    {
        AudioManager.Instance.PlaySound(hitSound);
        if (!string.IsNullOrEmpty(hitEffectLabel))
        {
            GameObject hitEffectGo = PoolManager.Instance.EffectPool.Get(hitEffectLabel, hitPosition, Quaternion.identity);
            if (hitEffectGo != null && hitEffectGo.TryGetComponent<Effect>(out Effect effect))
            {
                if (effect.WeaponEffect != null)
                {
                    effect.WeaponEffect.Animator.Rebind();
                    effect.WeaponEffect.Animator.Play("Init", 0, 0f);
                }
                else if (effect.ParticleSystemEvent != null)
                {
                    effect.ParticleSystemEvent.EffectLabel = hitEffectLabel;
                }
            }
        }
    }

    private IEnumerator HitFlash(float duration)
    {
        this.SetOutlineReferenceTexWidth(1024);

        yield return new WaitForSeconds(duration);
        this.SetOutlineReferenceTexWidth(0);
    }

    private void SetOutlineReferenceTexWidth(int value)
    {
        if (this.meshRenderer == null) return;

        Material mat = meshRenderer.sharedMaterials[0];
        string propName = "_OutlineReferenceTexWidth";
        mat.SetInt(propName, value);
    }

    public void OnUpdate()
    {
        this.m_HealthBar.transform.localScale = new Vector3(this.m_BaseNode.localScale.x, 1, 1);

        if (this.meshRenderer != null && meshRenderer.sharedMaterials.Length > 0)
        {
            if (this.m_IsInitMaterial)
            {
                this.m_IsInitMaterial = false;
                this.SetOutlineReferenceTexWidth(0);
            }
        }
    }
}
