using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public GenericObjectPool EnemyPool;
    public GenericObjectPool ServantPool;
    public GenericObjectPool BulletPool;
    public GenericObjectPool RewardPool;
    public GenericObjectPool EffectPool;
    public GenericObjectPool Particle2DEffectPool;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SWGameManager.Instance.OnGameRetryEvent.AddListener(this.OnPoolReleaseAllEvent);
    }

    private void OnPoolReleaseAllEvent(Transform initTrans)
    {
        this.EnemyPool.ReleaseAll();
        this.ServantPool.ReleaseAll();
        this.BulletPool.ReleaseAll();
        this.RewardPool.ReleaseAll();
        this.EffectPool.ReleaseAll();
    }
}