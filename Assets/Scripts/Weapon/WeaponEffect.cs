using UnityEngine;

public class WeaponEffect : MonoBehaviour
{
    public Animator Animator => this.m_Animator;

    [SerializeField]
    private GameObject m_BaseGo;

    [SerializeField]
    private Animator m_Animator;

    public void AnimationOver(BulletSo bulletSo)
    {
        if (bulletSo != null)
        {
            PoolManager.Instance.EffectPool.Release(bulletSo.HitEffectLabel, this.m_BaseGo);
        }
    }

    public void AnimationOver2(MeteorSkillSo meteorSkillSo)
    {
        if (meteorSkillSo != null)
        {
            PoolManager.Instance.EffectPool.Release(meteorSkillSo.HitEffectLabel, this.m_BaseGo);
        }
    }

    public void AnimationOver3(PlayerSo playerSo)
    {
        if (playerSo != null)
        {
            PlayerLevelInfo playerLevelInfo = playerSo.GetPlayerLevelInfoByLevel(SWGameManager.Instance.CurrentPlayer.CurrentLevel);
            PoolManager.Instance.EffectPool.Release(playerLevelInfo.UpgradeEffectName, this.m_BaseGo);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this.m_Animator == null) this.m_Animator = this.GetComponent<Animator>();
    }
#endif
}