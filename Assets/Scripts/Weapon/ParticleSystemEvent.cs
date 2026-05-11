using UnityEngine;

public class ParticleSystemEvent : MonoBehaviour
{
    [SerializeField]
    private Effect m_Effect;

    public string EffectLabel;

    private void OnParticleSystemStopped()
    {
        PoolManager.Instance.EffectPool.Release(this.EffectLabel, this.m_Effect.gameObject);
    }
}