using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class GameObjectPair
{
    public string Name;
    public GameObject prefab;
    public int initialSize = 20; // 初始容量
}

public class GenericObjectPool : MonoBehaviour
{
    [SerializeField] private GameObjectPair[] prefabs; // 预制体

    private readonly Dictionary<string, Queue<GameObject>> poolDic = new Dictionary<string, Queue<GameObject>>();

    private readonly List<(string name, GameObject obj)> activeObjects = new List<(string, GameObject)>();

    private void Awake()
    {
        this.Init();
    }

    public void Init()
    {
        poolDic.Clear();
        foreach (var item in prefabs)
        {
            if (string.IsNullOrEmpty(item.Name))
            {
                continue;
            }
            Queue<GameObject> q_pool = new Queue<GameObject>();
            // 预填充池子
            for (int i = 0; i < item.initialSize; i++)
            {
                CreateNewObject(item, ref q_pool);
            }
            poolDic.Add(item.Name, q_pool);
        }
    }

    private GameObject CreateNewObject(GameObjectPair objectPair, ref Queue<GameObject> queue)
    {
        GameObject obj = Instantiate(objectPair.prefab, this.transform);
        obj.SetActive(false);
        queue.Enqueue(obj);
        return obj;
    }

    public string GetRandomObjName(string withOutName)
    {
        List<string> names = new List<string>();
        foreach (var item in this.prefabs)
        {
            if (!withOutName.Equals(item.Name))
            {
                names.Add(item.Name);
            }
        }

        return names[UnityEngine.Random.Range(0, names.Count)];
    }

    // 从池中取出
    public GameObject Get(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            return null;
        }
        //GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        GameObject obj = null;

        if (poolDic.ContainsKey(prefabName))
        {
            Queue<GameObject> queue = poolDic[prefabName];

            if (queue.Count > 0)
            {
                obj = queue.Dequeue();
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
            }
            else
            {
                GameObject source = this.GetPrefabSourceByName(prefabName);
                if (source != null)
                {
                    obj = Instantiate(source, this.transform);
                }
            }
        }

        if (obj != null)
        {
            // 记录该对象正在使用
            activeObjects.Add((prefabName, obj));
        }

        return obj;
    }

    public async UniTask<GameObject> GetAsync(string prefabName, Vector3 position, Quaternion rotation, float releaseTime = 0f)
    {
        GameObject go = this.Get(prefabName, position, rotation);

        if (releaseTime > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(releaseTime));
            this.Release(prefabName, go);
        }

        return go;
    }

    // 回收到池中
    public void Release(string prefabName, GameObject obj)
    {
        if (obj == null && string.IsNullOrEmpty(prefabName)) return;

        if (poolDic.ContainsKey(prefabName))
        {
            activeObjects.RemoveAll(x => x.obj == obj);

            obj.SetActive(false);
            // 回收时放回池子节点下，保持层级整洁
            obj.transform.SetParent(transform);
            Queue<GameObject> queue = poolDic[prefabName];
            queue.Enqueue(obj);
        }
    }

    private GameObject GetPrefabSourceByName(string name)
    {
        foreach (var item in prefabs)
        {
            if (name.Equals(item.Name))
            {
                return item.prefab;
            }
        }
        return null;
    }

    public void ReleaseAll()
    {
        // 倒序遍历，防止 Remove 导致的索引问题
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var entry = activeObjects[i];

            // 检查对象是否还存在（防止被意外 Destroy）
            if (entry.obj != null)
            {
                // 直接调用原有的 Release 逻辑
                // 注意：为了效率，可以直接在这里写逻辑，避免反复查 List
                entry.obj.SetActive(false);
                entry.obj.transform.SetParent(transform);
                if (poolDic.TryGetValue(entry.name, out var queue))
                {
                    queue.Enqueue(entry.obj);
                }
            }
        }
        activeObjects.Clear();
    }
}