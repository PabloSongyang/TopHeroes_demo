using UnityEngine;

public class Effect : MonoBehaviour, IPoolElement
{
    public GameObject RenderObject => this.gameObject;

    public WeaponEffect WeaponEffect => this.m_WeaponEffect;
    public ParticleSystemEvent ParticleSystemEvent => this.m_ParticleSystemEvent;

    [SerializeField]
    private WeaponEffect m_WeaponEffect;

    [SerializeField]
    private ParticleSystemEvent m_ParticleSystemEvent;
}