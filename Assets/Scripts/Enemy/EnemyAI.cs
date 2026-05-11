using DG.Tweening;
using Pathfinding;
using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using static Pathfinding.PathUtilities;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;



public class EnemyAI : MonoBehaviour, IPoolElement, IEntity
{
    public EnemyHealth EnemyHealth => this.m_EnemyHealth;
    public EnemySo EnemySo => this.m_EnemySo;

    public Transform CurrentTarget => this.currentTarget;

    public AIType AiType => this.m_AIType;

    public int EnemyID => this.m_EnemyID;
    public int EntityID => this.m_EntityID;

    public GameObject RenderObject => this.gameObject;

    public IEntity.Type EntityType => IEntity.Type.Enemy;

    public EnemyAttack EnemyAttack => this.m_EnemyAttack;

    public Dic<string, List<SpineSlotAttachment>> SpineSlotAttachmentDic => this.m_SpineSlotAttachmentDic;
    public SpineComponent SpineComponent => this.m_SpineComponent;

    [SerializeField]
    private int m_EnemyID;

    [SerializeField]
    private int m_EntityID;

    [SerializeField]
    private AIPath aiPath;

    [SerializeField]
    private Transform currentTarget;

    private Transform m_FollowObject;

    [SerializeField]
    private EnemySo m_EnemySo;

    [SerializeField]
    private Animator m_Animator;

    private float nextAttackTime = 0f;

    [SerializeField]
    private SpineAnimationEvent m_SpineAnimationEvent;

    [SerializeField]
    private EnemyHealth m_EnemyHealth;

    [SerializeField]
    private EnemyAttack m_EnemyAttack;

    [SerializeField]
    private SpineComponent m_SpineComponent;


    private AIType m_AIType;


    private IAstarAI ai;
    public float repathRate = 0.5f;   // 路径更新频率 (秒)
    public float nextWaypointDistance = 3f; // 切换到下一个路点的距离阈值
    private float lastRepathTime = -9999f;
    private float scanInterval = 0.2f;  // 每0.2秒搜寻一次目标，节省CPU

    [SerializeField, Tooltip("大招冷却")]
    private float m_UltimateSkillCD;

    [SerializeField, Tooltip("大招CD计时器")]
    private float m_UltimateSkillTimer;

    /// <summary>
    /// 当前正在释放大招
    /// </summary>
    private bool m_IsReleasingUltimateSkill;

    /// <summary>
    /// 是否可以释放大招？
    /// </summary>
    private bool m_IsCanReleaseUltimateSkill;

    private float nextScanTime;

    private readonly Dic<string, List<SpineSlotAttachment>> m_SpineSlotAttachmentDic = new Dic<string, List<SpineSlotAttachment>>();

    public Dic<IMeteorSkill.MeteorSkillType, IMeteorSkill> IMeteorSkillDic { get; private set; }

    private void OnEnable()
    {
        ai = GetComponent<IAstarAI>();
        ai.simulateMovement = true;
        foreach (var item in this.m_EnemySo.SpineSlotAttachmentInfosList)
        {
            this.m_SpineSlotAttachmentDic.Add(item.Label, item.SpineSlotAttachmentList);
        }
    }

    private void Start()
    {
        this.IMeteorSkillDic = new Dic<IMeteorSkill.MeteorSkillType, IMeteorSkill>();
        this.m_SpineAnimationEvent.EnemyAttackEndEvent += this.OnEnemyAttackEndEvent;
        SWGameManager.Instance.OnPlayerWinEvent.AddListener(this.OnGameOverEvent);
        SWGameManager.Instance.OnPlayerDeadEvent.AddListener(this.OnGameOverEvent);
        this.m_EnemyHealth.BindingEvent();
        this.m_EnemyAttack.BindingEvent();


        foreach (var item in this.m_EnemySo.MeteorSkillTypes)
        {
            switch (item)
            {
                case IMeteorSkill.MeteorSkillType.流星空降式:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.流星空降式, new MeteorAirborneMeteor());
                    break;
                case IMeteorSkill.MeteorSkillType.多目标原地生成式:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.多目标原地生成式, new MultiTargetIn_SituGenerationFormulaMeteor());
                    break;
                case IMeteorSkill.MeteorSkillType.原地大范围生成式:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.原地大范围生成式, new LargeScaleGenerationInSituMeteor());
                    break;
                case IMeteorSkill.MeteorSkillType.自身状态式:
                    this.IMeteorSkillDic.Add(IMeteorSkill.MeteorSkillType.自身状态式, new SelfStateMeteor());
                    break;
            }
        }
    }

    public void Create(int entityID)
    {
        this.m_EntityID = entityID;
        this.m_FollowObject = null;
        currentTarget = null;
        this.m_IsCanReleaseUltimateSkill = true;
        this.m_AIType = this.m_EnemySo.AIType;
        this.m_EnemyID = this.m_EnemySo.ID;
        // 从对象池取出时，动态赋值玩家目标
        GameObject player = SWGameManager.Instance.CurrentPlayer.gameObject;
        if (this.m_AIType == AIType.Enemy)
        {
            currentTarget = player.transform;
        }
        else if (this.m_AIType == AIType.Servant)
        {
            this.m_FollowObject = player.transform;
            Sequence seq = DOTween.Sequence();
            Vector3 targetPos = player.transform.position - player.transform.right * 2f;
            seq.Append(this.transform.DOMoveY(3, 0.5f).SetRelative().SetEase(Ease.OutQuad));
            seq.Append(this.transform.DOMove(targetPos, 0.25f).SetEase(Ease.OutSine));

            seq.OnComplete(() =>
            {
                if (currentTarget != null)
                    StartCoroutine(ReleaseUltimateSkill_Deley());
            });
        }

        this.m_EnemyHealth.Init(ai, this.m_EntityID);
        this.m_SpineAnimationEvent.Animator.SetLayerWeight(1, 1);
    }


    private IEnumerator ReleaseUltimateSkill_Deley()
    {
        yield return new WaitForSeconds(3);
        this.ReleaseUltimateSkill();
        this.m_IsCanReleaseUltimateSkill = false;
    }

    private void OnGameOverEvent()
    {
        if (this == null) return;

        ai.destination = this.transform.position;
        ai.simulateMovement = false;

        this.m_SpineAnimationEvent.Animator.ResetTrigger("Attack");
        this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", 0);
        this.m_SpineAnimationEvent.Animator.SetFloat("MoveX", 0);
        this.m_SpineAnimationEvent.Animator.SetFloat("MoveY", 0);

        this.m_SpineAnimationEvent.Animator.SetLayerWeight(1, 0);
    }

    public void EnemyUpdate()
    {
        if (ai == null) return;


        if (this.m_EnemyHealth.Dead) return;
        this.m_EnemyHealth.OnUpdate();

        this.Attacking();
        this.SetSpriteXY();
        this.MotionAnimation();

        //UpdateTargetPosition();
    }

    private void Attacking()
    {
        // 性能关键：通过计时器控制寻路频率，而非每帧 SearchPath
        if (Time.time > lastRepathTime + repathRate && !ai.pathPending)
        {
            lastRepathTime = Time.time;
            UpdatePath();
        }

        if (this.m_AIType == AIType.Enemy)
        {
            if (this.m_EnemySo.IsBoss)
            {
                if (!this.m_IsCanReleaseUltimateSkill)
                {
                    this.m_UltimateSkillTimer += Time.deltaTime;
                    if (this.m_UltimateSkillTimer > this.m_UltimateSkillCD)
                    {
                        this.m_UltimateSkillTimer = 0;
                        this.m_IsCanReleaseUltimateSkill = true;
                    }
                }

                //this.m_IsReleasingUltimateSkill = this.m_SpineAnimationEvent.Animator.GetInteger("AttackIndex") == 2;

                // 1. 性能优化：定时搜寻最近目标，而不是每帧都搜
                if (Time.time >= nextScanTime)
                {
                    currentTarget = SWGameManager.Instance.CurrentPlayer.transform;
                    nextScanTime = Time.time + scanInterval;
                }

                // 2. 检查是否有有效目标且攻击冷却完毕
                if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
                {
                    // 如果目标跑出了范围，丢弃目标
                    if (Vector2.Distance(transform.position, currentTarget.position) > this.m_EnemySo.AttackRange)
                    {
                        //currentTarget = null;
                        return;
                    }

                    if (!this.m_IsReleasingUltimateSkill)
                    {
                        if (this.m_IsCanReleaseUltimateSkill)
                        {
                            this.ReleaseUltimateSkill();
                        }
                        else if (Time.time >= nextAttackTime)
                        {
                            this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", this.m_EnemySo.GetRandomNormalAttackIndex());
                            this.m_SpineAnimationEvent.Animator.SetTrigger("Attack");
                            nextAttackTime = Time.time + this.m_EnemySo.AttackRate;
                            Debug.Log("仆从启动射击。。。。。");
                        }
                    }
                }
                else
                {
                    this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", 0);
                    Debug.Log("仆从停止射击。。。。。");
                }
            }
            else
            {
                // 计算与玩家的距离
                float distance = Vector2.Distance(transform.position, this.currentTarget.position);

                // 如果距离够近 且 CD 结束
                if (distance <= this.m_EnemySo.AttackRange && Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + this.m_EnemySo.AttackRate;
                }
                else
                {
                    this.m_Animator.SetInteger("AttackIndex", 0);
                    this.m_Animator.ResetTrigger("Attack");
                }
            }

        }
        else if (this.m_AIType == AIType.Servant)
        {
            if (this.m_FollowObject == null) return;

            float distance = Vector2.Distance(this.transform.position, this.m_FollowObject.position);

            if (distance > 10)
            {
                this.transform.position = this.m_FollowObject.position;
            }

            if (!this.m_IsCanReleaseUltimateSkill)
            {
                this.m_UltimateSkillTimer += Time.deltaTime;
                if (this.m_UltimateSkillTimer > this.m_UltimateSkillCD)
                {
                    this.m_UltimateSkillTimer = 0;
                    this.m_IsCanReleaseUltimateSkill = true;
                }
            }

            //this.m_IsReleasingUltimateSkill = this.m_SpineAnimationEvent.Animator.GetInteger("AttackIndex") == 2;

            // 1. 性能优化：定时搜寻最近目标，而不是每帧都搜
            if (Time.time >= nextScanTime)
            {
                currentTarget = FindNearestEnemy(this.m_EnemySo.AttackRange);
                nextScanTime = Time.time + scanInterval;
            }

            // 2. 检查是否有有效目标且攻击冷却完毕
            if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            {
                // 如果目标跑出了范围，丢弃目标
                if (Vector2.Distance(transform.position, currentTarget.position) > this.m_EnemySo.AttackRange)
                {
                    currentTarget = null;
                    return;
                }

                if (!this.m_IsReleasingUltimateSkill)
                {
                    if (this.m_IsCanReleaseUltimateSkill)
                    {
                        this.ReleaseUltimateSkill();
                    }
                    else if (Time.time >= nextAttackTime)
                    {
                        this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", 1);
                        this.m_SpineAnimationEvent.Animator.SetTrigger("Attack");
                        nextAttackTime = Time.time + this.m_EnemySo.AttackRate;
                        Debug.Log("仆从启动射击。。。。。");
                    }
                }
            }
            else
            {
                this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", 0);
                Debug.Log("仆从停止射击。。。。。");
            }
        }
    }

    /// <summary>
    /// 释放大招
    /// </summary>
    private void ReleaseUltimateSkill()
    {
        // 1. 立即重置 CD 标志位，防止下一帧逻辑再次进入这里
        this.m_IsCanReleaseUltimateSkill = false;
        this.m_UltimateSkillTimer = 0;


        this.m_SpineAnimationEvent.Animator.SetInteger("AttackIndex", 2);
        this.m_SpineAnimationEvent.Animator.SetTrigger("Attack");

        this.m_IsReleasingUltimateSkill = true;
        Debug.Log("仆从释放大招！");
    }

    /// <summary>
    /// 根据移动速度处理镜像翻转
    /// </summary>
    private void SetSpriteXY()
    {
        if (this.IsAttacking())
        {
            if (this.currentTarget == null) return;
            this.transform.localScale = new Vector3(this.transform.position.x - this.currentTarget.position.x < 0 ? 1 : -1, 1, 1);
        }

        if (aiPath.desiredVelocity.x > .1f || aiPath.desiredVelocity.x < -.1)
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("X", aiPath.desiredVelocity.x > 0 ? 1 : -1);

            if (!this.IsAttacking())
            {
                this.transform.localScale = new Vector3(aiPath.desiredVelocity.x > 0 ? 1 : -1, 1, 1);
            }
        }
        else
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("X", 0);
        }

        if (aiPath.desiredVelocity.y > .1f || aiPath.desiredVelocity.y < -.1f)
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("Y", aiPath.desiredVelocity.y > 0 ? 1 : -1);
        }
        else
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("Y", 0);
        }
    }

    private void MotionAnimation()
    {
        if (this.IsArrived() || !this.IsMoving())
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("Speed", 0);
        }

        if (this.IsMoving())
        {
            this.m_SpineAnimationEvent.Animator.SetFloat("Speed", ai.velocity.sqrMagnitude);
        }
    }

    /// <summary>
    /// 是否到达寻路点
    /// </summary>
    /// <returns></returns>
    private bool IsArrived()
    {
        if (ai.reachedEndOfPath && !ai.pathPending)
        {
            Debug.Log("AI 已到达终点并停止");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否在移动？
    /// </summary>
    /// <returns></returns>
    private bool IsMoving()
    {
        return ai.velocity.sqrMagnitude > 0.01f;
    }

    private bool IsAttacking()
    {
        return this.m_Animator.GetInteger("AttackIndex") > 0;
    }

    private Transform FindNearestEnemy(float radius)
    {
        // 物理层检测（记得给怪物设置 Enemy 层）
        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(transform.position, radius, this.m_EnemySo.TargetLayer);
        Transform nearestEnemy = null;
        float minEnemyDist = Mathf.Infinity;


        foreach (var col in targetColliders)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);

            // 优先逻辑：如果是敌人
            if (col.CompareTag("Enemy"))
            {
                if (dist < minEnemyDist)
                {
                    minEnemyDist = dist;
                    nearestEnemy = col.transform;
                }
            }
        }

        // 最后的判断：只要有敌人，就回传最近的敌人；否则回传最近的奖励
        return nearestEnemy;
    }

    void UpdatePath()
    {
        // 仅在目标移动超过一定阈值时才真正设置 destination
        // 这能减少内部路径计算开销
        if (this.m_AIType == AIType.Enemy)
        {
            if (this.m_EnemySo.IsBoss)
            {
                if (SWGameManager.Instance.CurrentPlayer.IsActiveBoss && this.currentTarget != null)
                    ai.destination = this.currentTarget.position;
                else
                {
                    ai.destination = this.transform.position;
                }
            }
            else
            {
                if (this.currentTarget != null)
                    ai.destination = this.currentTarget.position;
                else
                {
                    ai.destination = this.transform.position;
                }
            }
        }
        else if (this.m_AIType == AIType.Servant)
        {
            if (this.m_FollowObject != null)
            {
                if (this.m_IsReleasingUltimateSkill)
                {
                    ai.destination = this.transform.position;
                    ai.simulateMovement = false;
                    this.m_SpineAnimationEvent.Animator.SetFloat("MoveX", 0);
                    this.m_SpineAnimationEvent.Animator.SetFloat("MoveY", 0);
                }
                else
                {
                    ai.simulateMovement = true;
                    ai.destination = this.m_FollowObject.position;
                }
            }
        }
        ai.SearchPath();
    }

    private void OnEnemyAttackEndEvent(int attackIndex)
    {
        if (attackIndex == 2)
        {
            if (this.m_AIType == AIType.Servant)
            {
                this.m_IsReleasingUltimateSkill = false;
            }
            else if (this.m_AIType == AIType.Enemy)
            {
                if (this.m_EnemySo.IsBoss)
                {
                    this.m_IsReleasingUltimateSkill = false;
                }
            }
        }

        //if (this.m_EnemySo.SpineSlotAttachmentInfosList.Count > 0)
        //{
        //    List<SpineSlotAttachment> spineSlotAttachmentList = this.m_SpineSlotAttachmentDic.Get("Default");
        //    SpineManager.Instance.SetSpecificAttachmentVisibility(this.m_SpineComponent, spineSlotAttachmentList);
        //}
    }

    // 针对大量单位的性能优化：当物体不可见时停止寻路
    void OnBecameVisible()
    {
        if (ai != null) ai.isStopped = false;
    }
    void OnBecameInvisible()
    {
        if (ai != null) ai.isStopped = true;
    }

    private void Attack()
    {
        int maxAttackIndex = this.m_EnemySo.IsBoss ? 3 : 2;

        // 1. 触发 Spine 攻击动画
        this.m_Animator.SetTrigger("Attack");
        this.m_Animator.SetInteger("AttackIndex", Random.Range(1, maxAttackIndex + 1));
    }

    private void OnDestroy()
    {
        this.m_SpineAnimationEvent.EnemyAttackEndEvent -= this.OnEnemyAttackEndEvent;
        SWGameManager.Instance.OnPlayerDeadEvent.RemoveListener(this.OnGameOverEvent);
    }

    public void UpdateTargetPosition()
    {
        if (this.currentTarget == null) return;

        var ais = UnityCompatibility.FindObjectsByTypeSorted<MonoBehaviour>().OfType<IAstarAI>().ToList();
        var destinations = PathUtilities.FormationDestinations(ais, this.currentTarget.position, FormationMode.SinglePoint, 0.5f);
        for (int i = 0; i < ais.Count; i++)
        {
#if MODULE_ENTITIES
            var isFollowerEntity = ais[i] is FollowerEntity;
#else
            var isFollowerEntity = false;
#endif
            if (ais[i] != null)
            {
                ais[i].destination = destinations[i];

                // Make the agents recalculate their path immediately for slighly increased responsiveness.
                // The FollowerEntity is better at doing this automatically.
                if (!isFollowerEntity) ais[i].SearchPath();
            }
        }

        StartCoroutine(OptimizeFormationDestinations(ais, destinations));
    }

    /// <summary>
    /// Swap the destinations of pairs of agents if it reduces the total distance they need to travel.
    ///
    /// This is a simple optimization algorithm to make group movement smoother and more efficient.
    /// It is not perfect and may not always find the optimal solution, but it is very fast and works well in practice.
    /// It will not work great for large groups of agents, as the optimization becomes too hard for this simple algorithm.
    ///
    /// See: https://en.wikipedia.org/wiki/Assignment_problem
    /// </summary>
    IEnumerator OptimizeFormationDestinations(List<IAstarAI> ais, List<Vector3> destinations)
    {
        // Prevent swapping the same agents multiple times.
        // This is because the distance measurement is only an approximation, and agents
        // may temporarily have to move away from their destination before they can move towards it.
        // Allowing multiple swaps could make the agents move back and forth indefinitely as the targets shift around.
        var alreadySwapped = new HashSet<(IAstarAI, IAstarAI)>();

        const int IterationsPerFrame = 4;

        while (true)
        {
            for (int i = 0; i < IterationsPerFrame; i++)
            {
                var a = Random.Range(0, ais.Count);
                var b = Random.Range(0, ais.Count);
                if (a == b) continue;
                if (b < a) Memory.Swap(ref a, ref b);
                var aiA = ais[a];
                var aiB = ais[b];

                if ((MonoBehaviour)aiA == null) continue;
                if ((MonoBehaviour)aiB == null) continue;

                if (alreadySwapped.Contains((aiA, aiB))) continue;

                var pA = aiA.position;
                var pB = aiB.position;
                var distA = (pA - destinations[a]).sqrMagnitude;
                var distB = (pB - destinations[b]).sqrMagnitude;

                var newDistA = (pA - destinations[b]).sqrMagnitude;
                var newDistB = (pB - destinations[a]).sqrMagnitude;
                var cost1 = distA + distB;
                var cost2 = newDistA + newDistB;
                if (cost2 < cost1 * 0.98f)
                {
                    // Swap the destinations
                    var tmp = destinations[a];
                    destinations[a] = destinations[b];
                    destinations[b] = tmp;

                    aiA.destination = destinations[a];
                    aiB.destination = destinations[b];

                    alreadySwapped.Add((aiA, aiB));
                }
            }
            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (this.aiPath == null) this.aiPath = this.GetComponent<AIPath>();
    }
#endif
}