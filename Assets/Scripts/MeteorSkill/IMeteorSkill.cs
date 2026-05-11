using UnityEngine;

public interface IMeteorSkill
{
    /// <summary>
    /// 大招类型
    /// </summary>
    public enum MeteorSkillType
    {
        流星空降式,
        多目标原地生成式,
        原地大范围生成式,
        自身状态式,
        蓄力直线射击,
        攻击者旋转射击
    }
    MeteorSkillType CurrentMeteorSkillType { get; }

    bool UltimateCompleted { get; }

    void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity);
}
