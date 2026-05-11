using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "(Bullet) Bullet So", menuName = "ScriptableObject/Bullet/Bullet So")]
public class BulletSo : ScriptableObject
{
    public int ID => this.m_ID;
    public int Damage => this.m_Damage;
    public string Label => this.m_Label;

    public float Speed => this.m_Speed;

    public float LeftTime => this.m_LeftTime;
    public bool IsPenetrate => this.m_IsPenetrate;
    public float BeatBackDistance => this.m_BeatBackDistance;
    public float AOERadius => this.m_AOERadius;
    public bool IsHitRewardBox => this.m_IsHitRewardBox;
    public LayerMask HitLayer => this.m_HitLayer;
    public string HitEffectLabel => this.m_HitEffectLabel;


    public float ChargeUpDamage => this.m_ChargeUpDamage;
    public SoundInfo HitSoundInfo => this.m_HitSoundInfo;

    [SerializeField]
    private int m_ID;

    [SerializeField]
    private string m_Label;

    [SerializeField, Range(0, 1000)]
    private int m_Damage;

    [SerializeField]
    protected float m_Speed;

    [SerializeField]
    protected float m_LeftTime;

    [SerializeField, Tooltip("是否为穿透射击？")]
    protected bool m_IsPenetrate;

    [SerializeField, Tooltip("击退距离？")]
    protected float m_BeatBackDistance = 0.2f;

    [SerializeField, Tooltip("AOE范围伤害")]
    protected float m_AOERadius;

    [SerializeField, Tooltip("是否可以击碎宝箱或者木桶？")]
    protected bool m_IsHitRewardBox;

    [SerializeField, Tooltip("打击目标所在的层级")]
    protected LayerMask m_HitLayer;

    [SerializeField, Tooltip("受击特效名称")]
    protected string m_HitEffectLabel;

    [SerializeField, Tooltip("打击音效")]
    protected SoundInfo m_HitSoundInfo;

    [SerializeField, Range(0, 3000)]
    private float m_ChargeUpDamage;
}
