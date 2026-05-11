using UltimateDH;
using Unity.Cinemachine;
using UnityEngine;

public class SWGameManager : MonoBehaviour
{
    public static SWGameManager Instance;

    public Player CurrentPlayer => this.player;

    public Dic<int, EnemyAI> EnemiesAIDic => this.m_EnemiesAIDic;

    public Transform InitPoint => this.m_InitPoint;

    public CinemachineImpulseSource CinemachineImpulseSource => this.m_CinemachineImpulseSource;
    public CinemachineCamera CinemachineCamera => this.m_CinemachineCamera;

    public bool IsInit;

    public bool IsWin;

    [SerializeField]
    private Player player;

    [SerializeField]
    private string m_WebURL = "https://www.baidu.com/";

    private Dic<int, EnemyAI> m_EnemiesAIDic = new Dic<int, EnemyAI>();

    [SerializeField]
    private Transform m_InitPoint;

    [SerializeField]
    private CinemachineCamera m_CinemachineCamera;

    [SerializeField]
    private CinemachineImpulseSource m_CinemachineImpulseSource;

    [SerializeField]
    private BossRange m_BossRange;

    [SerializeField]
    private RewardObjGroup m_RewardObjGroup;

    /// <summary>
    /// 游戏开始
    /// </summary>
    public Message OnGameStartEvent = new Message();

    /// <summary>
    /// 游戏重开
    /// </summary>
    public Message<Transform> OnGameRetryEvent = new Message<Transform>();

    /// <summary>
    /// 玩家死亡事件
    /// </summary>
    public Message OnPlayerDeadEvent = new Message();

    /// <summary>
    /// 玩家赢了（打死boss）的事件
    /// </summary>
    public Message OnPlayerWinEvent = new Message();

    /// <summary>
    /// 玩家武器升级时事件
    /// </summary>
    public Message<int> OnPlayerWeaponUpgradeEvent = new Message<int>();

    /// <summary>
    /// 玩家每升级一次蓄力射击完毕的事件
    /// </summary>
    public Message OnPlayerChargedUpCompleteEvent = new Message();

    public Dic<string, PolygonRange> EnemyCreatePolygonRangeDic = new Dic<string, PolygonRange>();

    public BossRange BossRange => this.m_BossRange;

    private void Awake()
    {
        Instance = this;
    }

    public void GotoURL()
    {
        Application.OpenURL(this.m_WebURL);
    }

    private void Start()
    {
        this.OnPlayerWinEvent.AddListener(this.GameOver);
        this.OnPlayerDeadEvent.AddListener(this.GameOver);
    }

    private void GameOver()
    {
        base.StopAllCoroutines();
    }

    private void UpdatePlayer()
    {
        this.CurrentPlayer.PlayerUpdate(this.m_RewardObjGroup.RewardObject_WeaponList);
    }

    private void UpdateEnemey()
    {
        foreach (var enemy in m_EnemiesAIDic.Body)
        {
            enemy.Value.EnemyUpdate();
        }
    }

    private void UpdateUI()
    {
        UIManager.Instance.UIUpdate();
    }


    private void Update()
    {
        this.UpdateUI();

        if (!this.IsInit) return;
        if (this.CurrentPlayer.IsDead) return;
        if (this.IsWin) return;

        this.UpdatePlayer();
        this.UpdateEnemey();
    }

    private void FixedUpdate()
    {
        if (!this.IsInit) return;
        if (this.IsWin) return;

        this.CurrentPlayer.PlayerFixedUpdate();
    }
}