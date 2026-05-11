using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private GameOverPanel panel;

    [SerializeField]
    private Button m_RetryBtn, m_DownloadBtn, m_WinDownloadBtn;

    [SerializeField]
    private GameObject m_FaiedGo;

    [SerializeField]
    private GameObject m_WinGo;

    [SerializeField]
    private RectTransform m_LogoRect; // 图中 "My Heroes" 的 Logo 部分

    [SerializeField]
    private Transform m_WinBg;

    [SerializeField]
    private CanvasGroup m_CanvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        this.m_RetryBtn.onClick.AddListener(() =>
        {
            this.Hide();
            SWGameManager.Instance.OnGameRetryEvent.Send(SWGameManager.Instance.InitPoint);
        });

        this.m_DownloadBtn.onClick.AddListener(SWGameManager.Instance.GotoURL);
        this.m_WinDownloadBtn.onClick.AddListener(SWGameManager.Instance.GotoURL);
    }

    public void Show(bool win)
    {
        this.m_WinGo.SetActive(win);
        this.m_FaiedGo.SetActive(!win);
        this.gameObject.SetActive(true);

        if (win)
        {
            this.m_WinDownloadBtn.transform.DOScale(0.95f, 0.08f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        this.PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        // 先重置状态
        this.m_CanvasGroup.alpha = 0;
        this.m_LogoRect.localScale = Vector3.zero;
        this.m_RetryBtn.transform.localScale = Vector3.zero;
        this.m_DownloadBtn.transform.localScale = Vector3.zero;

        // 创建动画序列
        Sequence seq = DOTween.Sequence();

        // 整体淡入
        seq.Join(this.m_CanvasGroup.DOFade(1f, 0.3f));

        // Logo 弹出 (带回弹效果)
        seq.Join(this.m_LogoRect.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        // 按钮依次弹出
        seq.Append(this.m_RetryBtn.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        seq.Append(this.m_DownloadBtn.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

        // 设置为不受 TimeScale 影响 (防止游戏暂停时动画不动)
        seq.SetUpdate(true);
    }

    public void Hide()
    {
        // 隐藏动画：缩小并淡出，完成后关闭 GameObject
        this.m_CanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        this.transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
            this.panel.gameObject.SetActive(false);
            this.transform.localScale = Vector3.one; // 恢复缩放供下次使用
        });
    }

    public void DoUpdate()
    {
        this.m_WinBg.transform.Rotate(Vector3.back, Time.deltaTime * 30f);
    }
}