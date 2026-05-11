using System.Collections.Generic;
using UnityEngine;

public class RewardObjGroup : MonoBehaviour
{
    public List<RewardObj> RewardObject_WeaponList => this.m_RewardObject_WeaponList;

    [SerializeField]
    private List<RewardObj> m_RewardObjList;

    [SerializeField]
    private List<RewardObj> m_RewardObject_WeaponList;

    private void Start()
    {
        this.m_RewardObjList.ForEach(_ => _.Init());
        SWGameManager.Instance.OnGameRetryEvent.AddListener(this.OnGameRetryEvent);
    }

    private void OnGameRetryEvent(Transform initTransform)
    {
        this.m_RewardObjList.ForEach(_ => _.Init());
        SWGameManager.Instance.HittedRewardObj_weapon.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var rewardObjs = this.GetComponentsInChildren<RewardObj>();
        if (rewardObjs != null && rewardObjs.Length > 0)
        {
            this.m_RewardObjList.Clear();
            m_RewardObjList.AddRange(rewardObjs);
        }
    }
#endif
}