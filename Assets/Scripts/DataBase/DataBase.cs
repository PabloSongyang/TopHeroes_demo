using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DicBase<T>
{
    public Dictionary<string, object> Body = new Dictionary<string, object>();

    public int Count { get { return Body.Count; } }

    public T Get(string Key)
    {
        object Obj = null;
        foreach (var child in Body)
        {
            if (child.Key == Key)
                Obj = child.Value;
        }
        return (T)Obj;
    }

    public U Get<U>()
    {
        object Obj = null;
        foreach (var child in Body)
        {
            if (child.Key == typeof(U).ToString())
                Obj = child.Value;
        }
        return (U)Obj;
    }

    public object GetObjectValue(string Key)
    {
        object Obj = null;
        foreach (var child in Body)
        {
            if (child.Key == Key)
                Obj = child.Value;
        }
        return Obj;
    }

    public void Add(string Key, object Value)
    {
        if (Body.ContainsKey(Key))
            Body.Remove(Key);
        Body.Add(Key, Value);
    }

    public void Remove(string Key)
    {
        if (Body.ContainsKey(Key))
            Body.Remove(Key);
    }

    public bool ContainsKey(string Key)
    {
        return Body.ContainsKey(Key);
    }

}


public class Dic<U, T>
{
    public Dictionary<U, T> Body = new Dictionary<U, T>();

    public int Count()
    {
        return Body.Count;
    }

    public KeyValuePair<U, T> this[int index]
    {
        get
        {
            if (Count() > 0)
                return Body.ElementAt(index);

            return default;
        }
    }

    public bool Contains(U Key)
    {
        return Body.ContainsKey(Key);
    }

    public T Get(U Key, out bool Result)
    {
        T obj;
        Result = Body.TryGetValue(Key, out obj);
        return obj;
    }

    public T Get(U Key)
    {
        T obj;
        Body.TryGetValue(Key, out obj);
        return obj;
    }

    public void Add(U Key, T Value)
    {
        if (Body.ContainsKey(Key))
            Body.Remove(Key);
        Body.Add(Key, Value);
    }

    public void Remove(U Key)
    {
        if (Body.ContainsKey(Key))
            Body.Remove(Key);
    }

    public void Clear()
    {
        Body.Clear();
    }
}


public class DicObject<T> : DicBase<T> where T : UnityEngine.Object
{
    public void Creat(IEnumerable<T> Group)
    {
        foreach (T child in Group)
        {
            Body.Add(child.name, child);
        }
    }
}

public class DicVar<T> : DicBase<T> where T : KeyObj
{
    public void Creat(IEnumerable<T> Group)
    {
        foreach (T child in Group)
        {
            Body.Add(child.key, child);
        }
    }
}


public class DicResources<T> : DicBase<T> where T : UnityEngine.Object
{

    /// <summary>
    /// 从本地加载到内存
    /// </summary>
    public void LoadAssets(string Key, string path)
    {
        T[] Data = Resources.LoadAll<T>(path);
        Add(Key, Data);
    }
    /// <summary>
    /// 获取资源组，返回为资源类型的一个数组
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T[] GetValueGroup(string key)
    {
        return GetObjectValue(key) as T[];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public DicObject<T> GetValueDic(string key)
    {
        DicObject<T> ItemBody = new DicObject<T>();
        ItemBody.Creat(GetValueGroup(key));
        return ItemBody;
    }
}


public class DicMode<T> where T : KeyData
{
    private Dictionary<string, T> Body = new Dictionary<string, T>();

    public DicMode(IEnumerable<T> array)
    {
        foreach (T child in array)
        {
            Body.Add(child.key, child);
        }
    }
    public T GetItem(string key)
    {
        T item = null;
        Body.TryGetValue(key, out item);
        return item;
    }

    public string GetTag(string key)
    {
        T item = null;
        Body.TryGetValue(key, out item);
        if (item == null)
            return null;
        return item.key;
    }

    public void Clear()
    {
        foreach (KeyData child in Body.Values)
            child.Clear();
    }
}


[System.Serializable]
public class KeyData
{
    public string key;
    public virtual void Clear()
    {

    }
}

public class KeyObj
{
    public string key;
}


