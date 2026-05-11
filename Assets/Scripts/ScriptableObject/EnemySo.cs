using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIType
{
    [InspectorName("(Enemy) 敌人")] Enemy,
    [InspectorName("(Servant) 仆从")] Servant
}

[CreateAssetMenu(fileName = "(Enemy) Enemy So", menuName = "ScriptableObject/Enemy/Enemy So")]
public class EnemySo : ScriptableObject
{
    public int ID => this.m_ID;
    public int HP => this.m_HP;
    public int Damage => this.m_Damage;
    public bool IsBoss => this.m_IsBoss;

    public float AttackRange => this.m_AttackRange;
    public float AttackRate => this.m_AttackRate;
    public LayerMask TargetLayer => this.m_TargetLayer;
    public string ObjectPoolLabel => this.m_ObjectPoolLabel;

    public AIType AIType => this.m_AIType;
    public string BulletLabel => this.m_BulletLabel;

    public MeteorSkillSo MeteorSkillSo => this.m_MeteorSkillSo;
    public IMeteorSkill.MeteorSkillType[] MeteorSkillTypes => this.m_MeteorSkillTypes;
    public bool HitFeedback => this.m_HitFeedback;
    public List<SpineSlotAttachmentInfo> SpineSlotAttachmentInfosList => this.m_SpineSlotAttachmentInfosList;
    public SoundInfo AASoundInfo => this.m_AASoundInfo;
    public bool UseAnimationMixed => this.m_UseAnimationMixed;

    [SerializeField]
    private int m_ID;

    [SerializeField]
    private AIType m_AIType;

    [SerializeField]
    private bool m_IsBoss;

    [SerializeField]
    private string m_ObjectPoolLabel;

    [SerializeField, Range(0, 50000)]
    private int m_HP;

    [SerializeField, Range(0, 500)]
    private int m_Damage;

    [SerializeField, Range(0, 15f), Tooltip("攻击触发的范围")]
    private float m_AttackRange = 1.5f;

    [SerializeField, Range(0, 5f), Tooltip("攻击频率（秒/次）")]
    private float m_AttackRate = 2.0f;

    [SerializeField]
    private int[] m_NormalAttackIndexList;

    [SerializeField, Tooltip("是否开启受击反馈？")]
    private bool m_HitFeedback = true;

    [SerializeField, Tooltip("打击目标所在的层级")]
    private LayerMask m_TargetLayer;

    [SerializeField, Tooltip("使用动画融合")]
    private bool m_UseAnimationMixed;

    [SerializeField]
    private string m_BulletLabel;

    [SerializeField, Tooltip("普攻音效")]
    private SoundInfo m_AASoundInfo;

    [SerializeField, Tooltip("大招技能")]
    protected MeteorSkillSo m_MeteorSkillSo;

    [SerializeField, Tooltip("技能类型")]
    protected IMeteorSkill.MeteorSkillType[] m_MeteorSkillTypes;

    [SerializeField]
    private List<SpineSlotAttachmentInfo> m_SpineSlotAttachmentInfosList;

    public int GetRandomNormalAttackIndex()
    {
        return this.m_NormalAttackIndexList.Length > 1 ? this.m_NormalAttackIndexList[Random.Range(0, this.m_NormalAttackIndexList.Length)] : this.m_NormalAttackIndexList[0];
    }
}
