using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardEffect : MonoBehaviour
{

    public void AnimatiomOver(string key)
    {
        this.Delay(key);
    }

    private async void Delay(string key)
    {
        if (!SWGameManager.Instance.IsInit) return;

        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

        PoolManager.Instance.EffectPool.Release(key, this.transform.parent.gameObject);
    }
}