using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FireRotate : MonoBehaviour
{
    [SerializeField, Range(0, 5f)]
    private float duration = 1.0f;

    [SerializeField, Range(0, 1f)]
    private float fireRate = 0.1f;    // 射击间隔

    public void Shoot(EnemyAI boss, UnityAction FiringAction, UnityAction CompletedEvent = null)
    {
        base.StartCoroutine(this.FiringSequence(boss, FiringAction, CompletedEvent));
    }

    private IEnumerator FiringSequence(EnemyAI boss, UnityAction FiringAction, UnityAction CompletedEvent = null)
    {
        // --- 阶段1：旋转扫射 ---
        duration = 1.0f; // 旋转一圈需要的时间
        float elapsed = 0f;


        while (elapsed < duration)
        {
            // 计算当前旋转角度 (0 到 360)
            float angle = Mathf.Lerp(0, 360, elapsed / duration);
            transform.rotation = Quaternion.Euler(0, 0, angle);

            elapsed += Time.deltaTime;

            yield return new WaitForSeconds(0.1f);
            FiringAction?.Invoke();
        }

        // --- 阶段2：锁定 Boss 射击 ---
        while (boss != null && boss.EnemyHealth.health > 0)
        {
            // 计算指向 Boss 的方向
            Vector2 direction = boss.EnemyAttack.ShootPos.transform.position - transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 锁定角度并射击
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            FiringAction?.Invoke();
            yield return new WaitForSeconds(fireRate);
        }


        CompletedEvent?.Invoke();

        base.StopAllCoroutines();
    }
}