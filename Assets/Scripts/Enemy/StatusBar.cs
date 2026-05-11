using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private float fadeSpeed = 2f;      // 显隐速度
    [SerializeField] private float smoothTime = 0.2f;   // 血条变化平滑时间

    private Coroutine fadeCoroutine;
    private Coroutine fillCoroutine;
    private float hideDelay = 2f; // 停止受击后多久隐藏

    [SerializeField]
    private Color m_FullColor;

    [SerializeField]
    private Color m_LowColor = Color.white;

    [SerializeField]
    private bool m_AlwaysShow;

    void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 1f;
            healthFillImage.color = this.m_FullColor;
        }
    }

    // 被 EnemyHealth 调用
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        float targetPercent = Mathf.Clamp01(currentHealth / maxHealth);

        if (this.m_AlwaysShow)
        {
            canvasGroup.alpha = 1;
        }
        else
        {
            // 1. 显示血条
            ToggleVisibility(true);
        }


        // 2. 更新进度与颜色
        if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        fillCoroutine = StartCoroutine(AnimateHealth(targetPercent));
    }

    private void ToggleVisibility(bool visible)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(Fade(visible ? 1f : 0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            yield return null;
        }

        // 如果变为了显示状态，开启自动隐藏倒计时
        if (targetAlpha >= 1f)
        {
            yield return new WaitForSeconds(hideDelay);
            ToggleVisibility(false);
        }
    }

    private IEnumerator AnimateHealth(float targetPercent)
    {
        float startPercent = healthFillImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < smoothTime)
        {
            elapsed += Time.deltaTime;
            float current = Mathf.Lerp(startPercent, targetPercent, elapsed / smoothTime);

            healthFillImage.fillAmount = current;
            // 颜色从白(0)到红(1)
            healthFillImage.color = Color.Lerp(this.m_LowColor, this.m_FullColor, current);

            yield return null;
        }
        healthFillImage.fillAmount = targetPercent;
    }

    public void HideImmediately()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0;
    }
}
