using UnityEngine;



[CreateAssetMenu(fileName = "(Meteor Skill) Meteor Skill So", menuName = "ScriptableObject/Meteor Skill/Meteor Skill So")]
public class MeteorSkillSo : ScriptableObject
{
    public int ID => this.m_ID;
    public int Damage => this.m_IsSplitDamage ? Mathf.RoundToInt((float)this.m_Damage / (float)this.m_EffectCount) : this.m_Damage;
    public string Label => this.m_Label;

    public float Speed => this.m_Speed;

    public float LeftTime => this.m_LeftTime;
    public float AOERadius => this.m_AOERadius;
    public LayerMask HitLayer => this.m_HitLayer;
    public string HitEffectLabel => this.m_HitEffectLabel;
    public int EffectCount => this.m_EffectCount;
    public Vector2 SpawnAreaSize => this.m_SpawnAreaSize;
    public SoundInfo HitSoundInfo => this.m_HitSoundInfo;
    public float UltimateRadius => this.m_UltimateRadius;
    public bool IsUltimateReleaseTargetPosition => this.m_IsUltimateReleaseTargetPosition;

    public float HeightOffset => this.m_HeightOffset;

    public float BeatBackDistance => this.m_BeatBackDistance;

    [SerializeField]
    private int m_ID;

    [SerializeField]
    private string m_Label;

    [SerializeField, Range(0, 1000)]
    private int m_Damage;

    [SerializeField, Tooltip("大招检测范围")]
    private float m_UltimateRadius = 3f;

    [SerializeField]
    protected float m_Speed;

    [SerializeField]
    protected float m_LeftTime;

    [SerializeField, Tooltip("击退距离？")]
    protected float m_BeatBackDistance = 0f;

    [SerializeField, Tooltip("特效数量")]
    protected int m_EffectCount = 6;

    [SerializeField, Tooltip("大招是否分摊伤害（就是Damage / m_EffectCount）否则，每个特效都是Damage伤害")]
    protected bool m_IsSplitDamage;

    [SerializeField, Tooltip("技能覆盖范围")]
    protected Vector2 m_SpawnAreaSize = new Vector2(10f, 5f);

    [SerializeField, Range(0, 30f), Tooltip("高度偏移量")]
    protected float m_HeightOffset = 10;

    [SerializeField, Tooltip("AOE范围伤害")]
    protected float m_AOERadius;

    [SerializeField, Tooltip("大招释放的位置是否是目标点为中心点，如果为False，则为释放者自己为中心点")]
    private bool m_IsUltimateReleaseTargetPosition;

    [SerializeField, Tooltip("打击目标所在的层级")]
    protected LayerMask m_HitLayer;

    [SerializeField, Tooltip("受击特效名称")]
    protected string m_HitEffectLabel;

    [SerializeField, Tooltip("受击音效")]
    protected SoundInfo m_HitSoundInfo;
}
