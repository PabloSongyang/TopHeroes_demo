using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRange : MonoBehaviour
{
    public EnemyAI CurrentBoss => this.m_CurrentBoss;

    [SerializeField]
    private EnemyAI m_CurrentBoss;

    [SerializeField]
    private GameObject m_wallObj;

    private void Start()
    {
        SWGameManager.Instance.OnGameRetryEvent.AddListener(this.OnGameRetryEvent);
    }

    private void OnGameRetryEvent(Transform initPoint)
    {
        this.SetBoss(null);
        this.ShowWall();
    }

    public void SetBoss(EnemyAI boss)
    {
        this.m_CurrentBoss = boss;
    }

    public void HideWall()
    {
        this.m_wallObj.SetActive(false);
    }

    private void ShowWall()
    {
        this.m_wallObj.SetActive(true);
    }
}
