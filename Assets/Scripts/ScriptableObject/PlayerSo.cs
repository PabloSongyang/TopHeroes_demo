using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerLevelInfo
{
    public int Level;
    public string UpgradeEffectName;
    public string BulletEffectLabel;

    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType;
    public string UltimateBulletEffectLabel;
    public MeteorSkillSo MeteorSkillSo;

    public string EnemyCreatePolygonRangeName;
}

[CreateAssetMenu(fileName = "(Player) Player So", menuName = "ScriptableObject/Player/Player So")]
public class PlayerSo : ScriptableObject
{
    public int ID => this.m_ID;
    public int HP => this.m_HP;
    public float ChargeUpTime => this.m_ChargeUpTime;
    public List<PlayerLevelInfo> PlayerLevelInfosList => this.m_PlayerLevelInfosList;

    public float DefaultCameraOrthographicSize => this.m_DefaultCameraOrthographicSize;

    public float ChargeUpCameraOrthographicSize => this.m_ChargeUpCameraOrthographicSize;
    public float SizeChangeSpeed => this.m_SizeChangeSpeed;
    public bool IsOpenChargeUp => this.m_IsOpenChargeUp;

    [SerializeField]
    private int m_ID;

    [SerializeField, Range(0, 2000)]
    private int m_HP;

    [SerializeField, Tooltip("是否开启蓄力？")]
    private bool m_IsOpenChargeUp;

    [SerializeField, Range(0, 3f), Tooltip("装备升级时蓄力时间")]
    private float m_ChargeUpTime;

    [SerializeField, Tooltip("默认时相机视口尺寸"), Min(0)]
    private float m_DefaultCameraOrthographicSize = 6.5f;

    [SerializeField, Tooltip("蓄力时相机视口尺寸"), Min(0)]
    private float m_ChargeUpCameraOrthographicSize = 10.5f;

    [SerializeField, Range(0, 10f)]
    private float m_SizeChangeSpeed;

    [SerializeField]
    private List<PlayerLevelInfo> m_PlayerLevelInfosList;

    [SerializeField]
    private List<SoundInfo> m_SoundInfoList;

    public PlayerLevelInfo GetPlayerLevelInfoByLevel(int currentPlayerLevel)
    {
        return this.m_PlayerLevelInfosList.Find(x => x.Level == currentPlayerLevel);
    }

    public SoundInfo GetSound(string label)
    {
        SoundInfo soundInfo = this.m_SoundInfoList.Find(x => x.Label.Equals(label));

        return soundInfo;
    } 
}