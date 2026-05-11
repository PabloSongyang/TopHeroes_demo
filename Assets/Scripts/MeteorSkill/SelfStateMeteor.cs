using UnityEngine;

/// <summary>
/// 自身状态式
/// </summary>
public class SelfStateMeteor : IMeteorSkill
{
    public IMeteorSkill.MeteorSkillType CurrentMeteorSkillType => IMeteorSkill.MeteorSkillType.自身状态式;

    public bool UltimateCompleted => this.m_UltimateCompleted;

    private bool m_UltimateCompleted;

    public void Ultimate(MonoBehaviour monoBehaviour, IEntity attackEntity)
    {
        this.m_UltimateCompleted = false;
        EnemyAI attackerAI = attackEntity as EnemyAI;
        ParticleSystem particleSystem = attackEntity.RenderObject.GetComponentInChildren<ParticleSystem>(true);
        particleSystem.gameObject.SetActive(true);
        particleSystem.GetComponent<AudioSource>().Play();
        particleSystem.Play();
        attackerAI.EnemyAttack.AddDamage += 20;
        this.m_UltimateCompleted = true;
    }
}
