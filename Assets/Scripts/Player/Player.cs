using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IEntity
{
    public bool IsDead => this.m_IsDead;
    public int CurrentLevel => this.m_CurrentLevel;

    public PlayerSo PlayerSo => this.m_PlayerSo;

    public GameObject RenderObject => this.gameObject;

    public IEntity.Type EntityType => IEntity.Type.Player;

    public List<SpineAnimationEvent> SpineAnimationEvents => this.m_SpineAnimationEvents;
    public SpineAnimationEvent CurrentSpineAnimationEvent => this.m_CurrentSpineAnimationEvent;

    public PlayerAutoAttack PlayerAutoAttack => this.m_PlayerAutoAttack;
    public PlayerHealth PlayerHealth => this.m_PlayerHealth;
    public GameObject ChargeUpEffect => this.m_ChargeUpEffect;

    [SerializeField]
    private PlayerAutoAttack m_PlayerAutoAttack;

    [SerializeField]
    private PlayerHealth m_PlayerHealth;

    [SerializeField]
    private PlayerMovement m_PlayerMovement;

    [SerializeField]
    private SpineSkinSwitcher m_SpineSkinSwitcher;

    [SerializeField]
    private PlayerSo m_PlayerSo;

    [SerializeField]
    private Transform m_ArrowTrans;

    private bool m_IsDead;

    [SerializeField]
    private ParticleSystem m_AddHPEffect, m_WeaponUpgradeEffect, m_WeaponUpgradeEffect2;

    [SerializeField]
    private GameObject m_ChargeUpEffect;

    [SerializeField]
    private int m_CurrentLevel;

    [SerializeField]
    private List<SpineAnimationEvent> m_SpineAnimationEvents;

    [SerializeField]
    private SpineAnimationEvent m_CurrentSpineAnimationEvent;

    private Transform m_UpgradeEffectTransform;


    public bool IsStartChargingUp;

    public bool IsCanMove;

    public bool IsActiveBoss;

    public Dic<IMeteorSkill.MeteorSkillType, IMeteorSkill> IMeteorSkillDic { get; private set; }

    private void Start()
    {
        this.IMeteorSkillDic = new Dic<IMeteorSkill.MeteorSkillType, IMeteorSkill>();
        this.Init();

        SWGameManager.Instance.OnGameRetryEvent.AddListener(this.OnGameRetryEvent);
        SWGameManager.Instance.OnPlayerDeadEvent.AddListener(() => this.m_IsDead = true);

        foreach (var item in this.m_PlayerSo.PlayerLevelInfosList)
        {
            switch (item.CurrentMeteorSkillType)
            {
                case IMeteorSkill.MeteorSkillType.流星空降式:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.流星空降式, new MeteorAirborneMeteor());
                    break;
                case IMeteorSkill.MeteorSkillType.蓄力直线射击:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.蓄力直线射击, new ChargingStraightLineShootingMeteor());
                    break;
                case IMeteorSkill.MeteorSkillType.攻击者旋转射击:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.攻击者旋转射击, new AttackerRotateShootingMeteor());
                    break;
            }
        }
    }

    private void Init()
    {
        this.m_CurrentSpineAnimationEvent = this.m_SpineSkinSwitcher.ChangeSkinObject("skin_A");

        this.m_CurrentLevel = 1;
        this.m_PlayerMovement.Init();
        this.m_PlayerHealth.Init();
        this.m_PlayerAutoAttack.Init();
        this.m_IsDead = false;
        this.m_AddHPEffect.gameObject.SetActive(false);
        this.m_WeaponUpgradeEffect.gameObject.SetActive(false);
        this.m_WeaponUpgradeEffect2.gameObject.SetActive(false);
        LensSettings lensSettings = SWGameManager.Instance.CinemachineCamera.Lens;
        lensSettings.OrthographicSize = this.m_PlayerSo.DefaultCameraOrthographicSize;
        SWGameManager.Instance.CinemachineCamera.Lens = lensSettings;
        this.m_CurrentSpineAnimationEvent.Animator.SetInteger("ChargeUpType", 0);
        this.m_CurrentSpineAnimationEvent.Animator.SetBool("IsChargeUp", false);
        this.IsCanMove = true;
        this.IsStartChargingUp = false;
        SWGameManager.Instance.CurrentPlayer.IsActiveBoss = false;



        this.m_ArrowTrans.gameObject.SetActive(false);
    }

    public void SetLevel(int level)
    {
        this.m_CurrentLevel = level;
    }

    public void GetRewards(LootItemSo lootItemSo, Vector3 createPosition)
    {
        if (lootItemSo == null) return;

        Debug.Log("吃到了道具.....");
        if (lootItemSo.LootType == LootItemSo.Type.HP)
        {
            LootLevelInfo current = lootItemSo.GetLootLevelInfoByNextLevel();
            this.m_PlayerHealth.TakeDamage(this, -current.AddHP, this.transform.position, Vector2.zero, 0, null);
            this.m_AddHPEffect.gameObject.SetActive(true);
            this.m_AddHPEffect.Play();
        }
        else if (lootItemSo.LootType == LootItemSo.Type.Equip)
        {
            LootLevelInfo current = lootItemSo.GetLootLevelInfoByNextLevel(this.m_CurrentLevel);

            PlayerLevelInfo playerLevelInfo = this.m_PlayerSo.GetPlayerLevelInfoByLevel(this.m_CurrentLevel);

            this.m_PlayerAutoAttack.AddDamage = current.AddDamage;
            this.m_CurrentSpineAnimationEvent = this.m_SpineSkinSwitcher.ChangeSkinObject(current.SpineSkinName);
            //GameObject effect = PoolManager.Instance.EffectPool.Get(playerLevelInfo.UpgradeEffectName, this.transform.position, Quaternion.identity);
            //this.m_UpgradeEffectTransform = effect.transform;
            this.m_WeaponUpgradeEffect.gameObject.SetActive(true);
            this.m_WeaponUpgradeEffect.Play();
            this.m_WeaponUpgradeEffect2.gameObject.SetActive(true);
            this.m_WeaponUpgradeEffect2.Play();
            this.m_CurrentLevel++;
            SWGameManager.Instance.OnPlayerWeaponUpgradeEvent.Send(this.m_CurrentLevel);
            this.m_PlayerHealth.TakeDamage(this, -current.AddHP, this.transform.position, Vector2.zero, 0, null);
            if (this.m_PlayerSo.IsOpenChargeUp)
            {
                this.m_ChargeUpEffect.SetActive(true);
                this.IsStartChargingUp = true;
                this.IsCanMove = false;

                this.m_PlayerAutoAttack.PlayAttackChargingUpAnimation();
            }
        }
        else if (lootItemSo.LootType == LootItemSo.Type.Servant)
        {
            LootLevelInfo current = lootItemSo.GetLootLevelInfoByNextLevel();
            GameObject go = PoolManager.Instance.ServantPool.Get(current.GetPrefabLabel(), createPosition, Quaternion.identity);

            if (go != null && go.TryGetComponent<EnemyAI>(out EnemyAI servantAI))
            {
                servantAI.Create(servantAI.EnemySo.ID);
                SWGameManager.Instance.EnemiesAIDic.Add(-servantAI.EnemySo.ID, servantAI);
            }
        }

        if (!string.IsNullOrEmpty(lootItemSo.EatSound.Label))
        {
            AudioManager.Instance.PlaySound(lootItemSo.EatSound);
        }
    }


    public void PlayerUpdate(List<RewardObj> rewardObj_weaponList)
    {
        this.m_PlayerHealth.OnUpdate();
        this.m_PlayerAutoAttack.Attacking();
        this.UpdateEffectDir();
        this.UpdateArrawIconDir(rewardObj_weaponList);
    }

    public void PlayerFixedUpdate()
    {
        if (this.m_IsDead) return;

        if (this.IsCanMove)
        {
            this.m_PlayerMovement.OnMove();
        }

        if (this.IsStartChargingUp)
        {
            PlayerLevelInfo playerLevelInfo = this.m_PlayerSo.GetPlayerLevelInfoByLevel(this.m_CurrentLevel);
            PolygonRange polygonRange = SWGameManager.Instance.EnemyCreatePolygonRangeDic.Get(playerLevelInfo.EnemyCreatePolygonRangeName);
            transform.localScale = new Vector3(polygonRange.transform.position.x > this.transform.position.x ? -1 : 1, 1, 1);
        }
    }



    public void CameraOrthographicSizeChangingLerp(float to, UnityAction complete = null)
    {
        LensSettings lensSettings = SWGameManager.Instance.CinemachineCamera.Lens;

        if (Mathf.Abs(lensSettings.OrthographicSize - to) >= 0.1f)
        {
            lensSettings.OrthographicSize = Mathf.Lerp(lensSettings.OrthographicSize, to, Time.deltaTime * this.m_PlayerSo.SizeChangeSpeed);
        }
        else
        {
            lensSettings.OrthographicSize = to;
            complete?.Invoke();
        }

        SWGameManager.Instance.CinemachineCamera.Lens = lensSettings;
    }

    private void OnGameRetryEvent(Transform initTrans)
    {
        this.transform.position = initTrans.position;
        this.transform.rotation = initTrans.rotation;
        this.transform.localScale = Vector3.one;

        this.Init();
    }

    /// <summary>
    /// 更新特效方向
    /// </summary>
    private void UpdateEffectDir()
    {
        this.m_AddHPEffect.transform.localScale = new Vector3(this.transform.localScale.x * .625f, .625f, .625f);
        this.m_WeaponUpgradeEffect.transform.localScale = new Vector3(this.transform.localScale.x * .625f, .625f, .625f);

        if (this.m_UpgradeEffectTransform != null && this.m_UpgradeEffectTransform.gameObject.activeSelf)
        {
            this.m_UpgradeEffectTransform.position = this.transform.position;
        }
    }

    /// <summary>
    /// 更新箭头指向
    /// </summary>
    /// <param name="rewardObj_weaponList"></param>
    private void UpdateArrawIconDir(List<RewardObj> rewardObj_weaponList)
    {
        this.m_ArrowTrans.transform.localScale = new Vector3(this.transform.localScale.x * 1.25f, 1.25f, 1.25f);

        if (rewardObj_weaponList.Count > 0)
        {
            List<RewardObj> all = rewardObj_weaponList.FindAll(x => !x.IsHitted);

            if (all.Count > 0)
            {
                RewardObj nearest = all.OrderBy(x => (x.transform.position - this.transform.position).sqrMagnitude).FirstOrDefault();
                if (nearest != null)
                {
                    this.m_ArrowTrans.gameObject.SetActive(true);

                    Vector3 direction = nearest.transform.position - transform.position;

                    // 使用 Atan2 计算角度（弧度转角度）
                    // Atan2(y, x) 得到的是从 X 轴正方向开始逆时针旋转的角度
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // 应用旋转
                    this.m_ArrowTrans.rotation = Quaternion.Euler(0, 0, angle);
                }
                else
                {
                    this.m_ArrowTrans.gameObject.SetActive(false);
                }
            }
        }
    }
}