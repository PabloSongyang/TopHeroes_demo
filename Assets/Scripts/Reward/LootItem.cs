using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UltimateDH;
using UnityEngine;


public class LootItem : MonoBehaviour, IPoolElement
{
    private Transform player;

    private LootItemSo m_CurrentLootItemSo;

    public Message onGetEvemt = new Message();

    private Vector3 m_CreatePosition;

    [SerializeField]
    private bool m_EatVaild = true;

    [SerializeField]
    private GameObject m_EffectGo;


    public bool EatVaild => this.m_EatVaild;
    public GameObject RenderObject => this.gameObject;

    public void Initialize(Vector3 createPosition, LootItemSo lootItemSo, LootItem lootItem, Transform playerTarget)
    {
        this.m_CreatePosition = createPosition;
        this.m_CurrentLootItemSo = lootItemSo;
        player = playerTarget;



        this.GetRewards(lootItem);
    }

    private void GetRewards(LootItem lootItem)
    {
        if (this.m_CurrentLootItemSo.LootType == LootItemSo.Type.Servant)
        {
            this.EatRewards(this.m_CurrentLootItemSo, lootItem);
        }
        else
        {
            this.PlayLootAnimation(lootItem);
        }
    }

    private void PlayLootAnimation(LootItem lootItem)
    {
        // 1. 抛物线跳跃阶段
        // 随机跳跃方向和距离
        float jumpDist = Random.Range(1f, 2.5f) * (Random.value > 0.5f ? 1 : -1);


        Vector3 jumpTarget = transform.position + new Vector3(jumpDist, 0, 0);

        // DOJump 参数：目标点、跳跃高度、跳跃次数、持续时间
        transform.DOJump(jumpTarget, 3f, 1, 0.6f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (this.m_EffectGo != null)
            {
                this.m_EffectGo.SetActive(true);
            }
            // 2. 跳跃完成后，稍微停顿 0.2 秒再吸向玩家
            DOVirtual.DelayedCall(0.3f, () => StartHoming(lootItem));
        });
    }

    private void StartHoming(LootItem lootItem)
    {
        if (player == null) return;

        // 3. 自动直线移动到玩家（吸附阶段）
        // 使用 Update 模式跟随，防止玩家移动导致位置偏离

        Vector3 endPos = player.position;

        if (this.m_CurrentLootItemSo.LootType == LootItemSo.Type.Coin)
        {
            endPos = player.position + Vector3.down * 10;
        }

        transform.DOMove(endPos, 0.5f)
        .SetEase(Ease.InBack) // InBack 会产生一种“先蓄力后加速”的吸入感
        .OnUpdate(() =>
        {
            if (this.m_CurrentLootItemSo.LootType != LootItemSo.Type.Coin)
            {
                player = SWGameManager.Instance.CurrentPlayer.transform;
                endPos = player.position;
            }


            // 如果玩家在移动，实时更新目标点（可选，DOMove本身不支持动态目标，若玩家移动快可用此方案）
            // 简单起见，这里直接使用快速移动
        })
        .OnComplete(() =>
        {
            // 4. 消失效果：缩小并销毁
            transform.DOScale(Vector3.zero, 0.15f).OnComplete(() =>
            {
                //onGetEvemt.Send();
                this.EatRewards(this.m_CurrentLootItemSo, lootItem);
                //Destroy(gameObject);

            });
        });

        // 持续追踪动态目标的优化版（如果玩家跑得很快）：
        /*
        DOTween.To(() => transform.position, x => transform.position = x, () => player.position, 0.5f)
               .SetTarget(transform)
               .OnComplete(()=> Destroy(gameObject));
        */
    }

    private void EatRewards(LootItemSo lootItemSo, LootItem lootItem)
    {
        if (player != null && lootItem.EatVaild)
            player.GetComponent<Player>().GetRewards(lootItemSo, this.m_CreatePosition);

        LootLevelInfo lootLevelInfo = null;
        if (lootItemSo.LootType == LootItemSo.Type.Equip)
        {
            lootLevelInfo = lootItemSo.GetLootLevelInfoByNextLevel(SWGameManager.Instance.CurrentPlayer.CurrentLevel - 1);
            foreach (var label in lootLevelInfo.PrefabLabelsList)
            {
                PoolManager.Instance.RewardPool.Release(label, this.gameObject);
            }

        }
        else
        {
            lootLevelInfo = lootItemSo.GetLootLevelInfoByNextLevel();
            PoolManager.Instance.RewardPool.Release(lootLevelInfo.GetPrefabLabel(), this.gameObject);
        }

        if (this.m_EffectGo != null)
        {
            this.m_EffectGo.SetActive(false);
        }
    }
}
