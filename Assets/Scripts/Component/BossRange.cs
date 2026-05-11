using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRange : MonoBehaviour
{
    public EnemyAI CurrentBoss => this.m_CurrentBoss;

    [SerializeField]
    private EnemyAI m_CurrentBoss;
    public void SetBoss(EnemyAI boss)
    {
        this.m_CurrentBoss = boss;
    }
}
