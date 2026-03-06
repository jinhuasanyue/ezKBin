using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池组件
/// 挂载在任何空物体上即可使用
/// </summary>
public class Pool : MonoBehaviour
{
    // 单例模式，方便全局访问
    public static Pool Instance { get; private set; }
    
    [Header("池子设置")]
    [SerializeField] private bool autoExpand = true; // 池子用尽时是否自动扩容
    [SerializeField] private int defaultCapacity = 10; // 默认容量
    
    // 存储所有对象池的字典
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    
    // 存储每个对象的原始预制体引用（用于重新创建）
    private Dictionary<GameObject, GameObject> originalPrefabs = new Dictionary<GameObject, GameObject>();
    
    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // 对象被销毁时（游戏结束或场景卸载时），清空所有对象
        ClearAllPools();
    }

// 添加一个应用程序退出时的处理
    private void OnApplicationQuit()
    {
        // 游戏退出时，不需要手动清理，因为操作系统会回收所有内存
        // 但如果你有特殊需求，也可以在这里处理
        ClearAllPools();
    }  

    /// <summary>
    /// 初始化一个对象池
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="initialCount">初始数量</param>
    /// <param name="poolKey">池子键名（默认使用预制体名称）</param>
    public void CreatePool(GameObject prefab, int initialCount, string poolKey = null)
    {
        if (prefab == null)
        {
            Debug.LogError("无法创建对象池：预制体为空");
            return;
        }
        
        // 如果没有指定键名，使用预制体名称
        string key = poolKey ?? prefab.name;
        
        // 如果池子已存在，不重复创建
        if (poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"对象池 '{key}' 已存在");
            return;
        }
        
        // 创建新的对象队列
        Queue<GameObject> objectQueue = new Queue<GameObject>();
        
        // 生成初始对象
        for (int i = 0; i < initialCount; i++)
        {
            GameObject obj = CreateNewObject(prefab, key);
            objectQueue.Enqueue(obj);
        }
        
        // 添加到字典
        poolDictionary.Add(key, objectQueue);
        
        Debug.Log($"已创建对象池 '{key}'，初始数量：{initialCount}");
    }
    
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <returns>获取的对象</returns>
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("无法获取对象：预制体为空");
            return null;
        }
        
        string key = prefab.name;
        
        // 如果池子不存在，自动创建
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.Log($"对象池 '{key}' 不存在，自动创建默认池子");
            CreatePool(prefab, defaultCapacity, key);
        }
        
        Queue<GameObject> queue = poolDictionary[key];
        GameObject objToGet;
        
        // 如果队列为空且允许扩容
        if (queue.Count == 0 && autoExpand)
        {
            Debug.Log($"对象池 '{key}' 已用尽，自动扩容创建新对象");
            objToGet = CreateNewObject(prefab, key);
        }
        else if (queue.Count > 0)
        {
            // 从队列取出
            objToGet = queue.Dequeue();
            
            // 检查对象是否已被销毁（比如场景切换时被意外删除）
            if (objToGet == null)
            {
                Debug.LogWarning($"对象池 '{key}' 中的对象已被销毁，重新创建");
                objToGet = CreateNewObject(prefab, key);
            }
        }
        else
        {
            Debug.LogError($"对象池 '{key}' 已用尽且不允许自动扩容");
            return null;
        }
        
        // 设置对象状态
        objToGet.transform.position = position;
        objToGet.transform.rotation = rotation;
        objToGet.SetActive(true);
        
        // 调用初始化方法（如果实现了IPoolable接口）
        IPoolable poolable = objToGet.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnObjectSpawn();
        }
        
        return objToGet;
    }
    
    /// <summary>
    /// 简化版：只传预制体和位置
    /// </summary>
    public GameObject GetObject(GameObject prefab, Vector3 position)
    {
        return GetObject(prefab, position, Quaternion.identity);
    }
    
    /// <summary>
    /// 简化版：只传预制体
    /// </summary>
    public GameObject GetObject(GameObject prefab)
    {
        return GetObject(prefab, Vector3.zero, Quaternion.identity);
    }
    
    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("无法回收空对象");
            return;
        }
        
        // 查找这个对象属于哪个池子
        GameObject originalPrefab;
        if (!originalPrefabs.TryGetValue(obj, out originalPrefab))
        {
            Debug.LogWarning($"对象 {obj.name} 不是由对象池创建的，将直接销毁");
            Destroy(obj);
            return;
        }
        
        string key = originalPrefab.name;
        
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"对象池 '{key}' 不存在，将直接销毁对象");
            Destroy(obj);
            return;
        }
        
        // 调用回收方法（如果实现了IPoolable接口）
        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnObjectReturn();
        }
        
        // 重置对象状态
        obj.SetActive(false);
        obj.transform.SetParent(transform); // 设置为对象池的子对象，保持场景整洁
        
        // 回收对象
        poolDictionary[key].Enqueue(obj);
    }
    
    /// <summary>
    /// 创建新对象（内部使用）
    /// </summary>
    private GameObject CreateNewObject(GameObject prefab, string poolKey)
    {
        GameObject newObj = Instantiate(prefab, transform);
        newObj.name = prefab.name; // 移除 "(Clone)" 后缀
        newObj.SetActive(false);
        
        // 记录原始预制体引用
        originalPrefabs[newObj] = prefab;
        
        return newObj;
    }
    
    /// <summary>
    /// 预加载对象池（在游戏开始时调用）
    /// </summary>
    public void PreloadPool(GameObject prefab, int count)
    {
        CreatePool(prefab, count);
    }
    
    /// <summary>
    /// 清空指定对象池
    /// </summary>
    public void ClearPool(string poolKey)
    {
        if (poolDictionary.ContainsKey(poolKey))
        {
            Queue<GameObject> queue = poolDictionary[poolKey];
            foreach (GameObject obj in queue)
            {
                if (obj != null)
                {
                    originalPrefabs.Remove(obj);
                    Destroy(obj);
                }
            }
            queue.Clear();
            poolDictionary.Remove(poolKey);
            Debug.Log($"已清空对象池 '{poolKey}'");
        }
    }
    
    /// <summary>
    /// 清空所有对象池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var kvp in poolDictionary)
        {
            foreach (GameObject obj in kvp.Value)
            {
                if (obj != null)
                {
                    originalPrefabs.Remove(obj);
                    Destroy(obj);
                }
            }
            kvp.Value.Clear();
        }
        poolDictionary.Clear();
        Debug.Log("已清空所有对象池");
    }
    
    /// <summary>
    /// 获取对象池当前大小
    /// </summary>
    public int GetPoolSize(string poolKey)
    {
        if (poolDictionary.ContainsKey(poolKey))
        {
            return poolDictionary[poolKey].Count;
        }
        return 0;
    }
}

/// <summary>
/// 对象池接口 - 让对象可以感知自己被生成/回收
/// </summary>
public interface IPoolable
{
    void OnObjectSpawn();  // 从池中取出时调用
    void OnObjectReturn(); // 回收回池时调用
}