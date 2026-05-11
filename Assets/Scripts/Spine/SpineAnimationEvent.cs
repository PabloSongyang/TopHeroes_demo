using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpineAnimationEvent : MonoBehaviour
{
    public Animator Animator => this.m_Animator;

    [SerializeField]
    private Animator m_Animator;

    public UnityAction<string> PlayerAttackReadyEvent;
    public UnityAction<string> PlayerAttackEvent;
    public UnityAction<string> PlayerAttackChargingUpEvent;
    public UnityAction<int> PlayerAttackEndEvent;

    public UnityAction<int> EnemyAttackEvent;
    public UnityAction<int> EnemyAttackEndEvent;
    public UnityAction EnemyDeadEndEvent;

    public void PlayerAttackReady(string key)
    {
        PlayerAttackReadyEvent?.Invoke(key);
    }

    public void PlayerAttack(string key)
    {
        PlayerAttackEvent?.Invoke(key);
    }

    public void PlayerAttackChargingUp(string key)
    {
        PlayerAttackChargingUpEvent?.Invoke(key);
    }

    public void PlayerAttackEnd(int key)
    {
        PlayerAttackEndEvent?.Invoke(key);
    }

    public void EnemyAttack(int attackIndex)
    {
        EnemyAttackEvent?.Invoke(attackIndex);
    }


    public void EnemyAttackEnd(int attackIndex)
    {
        EnemyAttackEndEvent?.Invoke(attackIndex);
    }

    public void EnemyDeadEnd(string key)
    {
        EnemyDeadEndEvent?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this.m_Animator == null) this.m_Animator = this.GetComponent<Animator>();
    }
#endif
}