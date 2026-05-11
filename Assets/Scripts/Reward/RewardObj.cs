using DG.Tweening;
using HighlightPlus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class RewardObj : MonoBehaviour
{
    public bool IsHitted => this.IsHit;

    [SerializeField]
    private SpriteGlow[] spriteGlows;

    [SerializeField]
    private List<LootItemSo> m_LootItemSoList;

    [SerializeField] private float minAlpha = 0.2f;    // 最暗时的透明度
    [SerializeField] private float maxAlpha = 1.0f;    // 最亮时的透明度
    [SerializeField] private float speed = 2.0f;       // 呼吸频率

    [SerializeField]
    private bool m_Tip;

    private bool IsHit;

    [SerializeField]
    private AudioClip m_BrokenCound;

    private Coroutine breathingRoutine;

    [SerializeField]
    private bool m_ActiveBoss;

    public void Init()
    {
        foreach (var item in spriteGlows)
        {
            item.gameObject.SetActive(true);
        }

        this.IsHit = false;
        if (this.m_Tip)
            transform.DOScale(0.95f, 0.08f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        if (breathingRoutine != null)
        {
            base.StopCoroutine(breathingRoutine);
        }

        breathingRoutine = StartCoroutine(DoBreathing());
    }



    public void DoHit()
    {
        if (!this.IsHit)
        {
            this.IsHit = true;
            this.StopBreathing();
            this.SpawnLoot();
            //this.gameObject.SetActive(false);
            foreach (var item in spriteGlows)
            {
                item.gameObject.SetActive(false);
                if (item.name.Contains("mutong"))
                {
                    PoolManager.Instance.EffectPool.Get("mutong_break", item.transform.position, Quaternion.identity);
                }
                else if (item.name.Contains("muxiang"))
                {
                    PoolManager.Instance.EffectPool.Get("muxiang_break", item.transform.position, Quaternion.identity);
                }
            }

            AudioManager.Instance.PlaySound(this.m_BrokenCound);

            if (this.m_ActiveBoss)
            {
                SWGameManager.Instance.CurrentPlayer.IsActiveBoss = true;
            }
        }
    }

    private void SpawnLoot()
    {
        if (this.m_LootItemSoList == null || this.m_LootItemSoList.Count == 0) return;

        for (int i = 0; i < this.m_LootItemSoList.Count; i++)
        {
            LootItemSo lootItemSo = this.m_LootItemSoList[i];

            LootLevelInfo lootLevelInfo = null;
            List<GameObject> rewardGoList = new List<GameObject>();
            if (lootItemSo.LootType == LootItemSo.Type.Equip)
            {
                lootLevelInfo = lootItemSo.GetLootLevelInfoByNextLevel(SWGameManager.Instance.CurrentPlayer.CurrentLevel);
                foreach (var label in lootLevelInfo.PrefabLabelsList)
                {
                    GameObject go = PoolManager.Instance.RewardPool.Get(label, transform.position, Quaternion.identity);
                    rewardGoList.Add(go);
                }
            }
            else
            {
                lootLevelInfo = lootItemSo.GetLootLevelInfoByNextLevel();
                GameObject go = PoolManager.Instance.RewardPool.Get(lootLevelInfo.GetPrefabLabel(), transform.position, Quaternion.identity);
                rewardGoList.Add(go);
            }
 

            // 寻找玩家
            GameObject player = SWGameManager.Instance.CurrentPlayer.gameObject;

            for (int j = 0; j < rewardGoList.Count; j++)
            {
                GameObject go = rewardGoList[j];
                if (go.TryGetComponent<LootItem>(out var loot))
                {
                    if (lootItemSo.LootType == LootItemSo.Type.Coin)
                    {
                        loot.transform.position += new Vector3(j % 3 * Random.Range(-.2f, .2f), j % 2 * Random.Range(-1f, 1f));
                    }
                    else
                    {
                        loot.transform.position += new Vector3(j * 1.25f, 0);
                    }

                    loot.Initialize(this.transform.position, lootItemSo, loot, player.transform);
                }
            }
        }
    }

    private IEnumerator DoBreathing()
    {
        float timer = 0f;


        Color tempColor = this.spriteGlows[0].GlowColor;

        while (true)
        {
            timer += Time.deltaTime * speed;

            // 使用 Sin 函数产生 0 到 1 的平滑波动
            // Sin 返回 -1 到 1，通过 (sin + 1) / 2 映射到 0 到 1
            float lerpFactor = (Mathf.Sin(timer) + 1f) / 2f;

            // 根据映射值计算当前的 Alpha
            tempColor.a = Mathf.Lerp(minAlpha, maxAlpha, lerpFactor);

            foreach (var item in this.spriteGlows)
            {
                item.GlowColor = tempColor;
            }

            yield return null; // 等待下一帧
        }


    }

    // 提供一个停止的方法，可以在被击中或消失时调用
    public void StopBreathing()
    {
        if (breathingRoutine != null)
        {
            StopCoroutine(breathingRoutine);
            // 恢复不透明，防止物体消失
            foreach (var item in this.spriteGlows)
            {
                item.GlowColor = new Color(item.GlowColor.r, item.GlowColor.g, item.GlowColor.b, 1);
            }
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        this.spriteGlows = this.GetComponentsInChildren<SpriteGlow>();
    }
#endif

}