using DG.Tweening;
using UltimateDH;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject m_Input;

    [SerializeField]
    private Button m_Download_L_Btn, m_Download_P_Btn;

    [SerializeField]
    private GameOverPanel m_GameOverPanel;

    public Message<bool> ScreenLOrPEvent = new Message<bool>();

    private bool m_IsScreenLandspace;

    [SerializeField]
    private GameObject m_Logo_L, m_Logo_R, m_Co_L, m_Co_R;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        this.m_IsScreenLandspace = (Screen.width > Screen.height);
        this.ScreenDirChangedEvent(this.m_IsScreenLandspace);

        this.ScreenLOrPEvent.AddListener(this.ScreenDirChangedEvent);

        SWGameManager.Instance.OnPlayerWinEvent.AddListener(this.OnGameWinEvent);
        SWGameManager.Instance.OnPlayerDeadEvent.AddListener(this.OnPlayerDeadEvent);
        SWGameManager.Instance.OnGameRetryEvent.AddListener(this.OnGameRetryEvent);

        this.m_Download_L_Btn.transform.DOScale(0.95f, 0.08f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        this.m_Download_P_Btn.transform.DOScale(0.95f, 0.08f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        this.m_Download_L_Btn.onClick.AddListener(SWGameManager.Instance.GotoURL);
        this.m_Download_P_Btn.onClick.AddListener(SWGameManager.Instance.GotoURL);
    }

    private void OnGameWinEvent()
    {
        this.m_Input.gameObject.SetActive(false);
        this.m_GameOverPanel.Show(true);
    }

    private void OnPlayerDeadEvent()
    {
        this.m_Input.gameObject.SetActive(false);
        this.m_GameOverPanel.Show(false);
    }

    private void OnGameRetryEvent(Transform initTrans)
    {
        this.m_Input.SetActive(true);
    }

    private void ScreenDirChangedEvent(bool landspace)
    {
        this.m_Logo_L.SetActive(landspace);
        this.m_Logo_R.SetActive(!landspace);
        this.m_Co_L.SetActive(landspace);
        this.m_Co_R.SetActive(!landspace);

        this.m_GameOverPanel.ScreenDirChangedEvent(landspace);
    }

    public void UIUpdate()
    {
        if (this.m_IsScreenLandspace != (Screen.width > Screen.height))
        {
            this.m_IsScreenLandspace = (Screen.width > Screen.height);
            this.ScreenLOrPEvent.Send(this.m_IsScreenLandspace);
        }

        this.m_GameOverPanel.DoUpdate();
    }
}