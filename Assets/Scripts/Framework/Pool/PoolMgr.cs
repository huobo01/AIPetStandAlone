using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽屉（池子中的数据）对象
/// 管理同一类型的对象：未使用的放入Stack，使用中的放入List
/// 支持数量上限，超限时复用最旧的使用中对象
/// </summary>
public class PoolData
{
    //存储未使用的对象（栈，后进先出）
    private Stack<GameObject> dataStack = new Stack<GameObject>();
    //记录使用中的对象
    private List<GameObject> usedList = new List<GameObject>();
    //场景上同时存在的对象上限个数
    private int maxNum;
    //抽屉根对象，用于层级布局管理
    private GameObject rootObj;

    //未使用对象数量
    public int Count => dataStack.Count;
    //使用中对象数量
    public int UsedCount => usedList.Count;
    //是否需要创建新对象（使用中数量 < 上限）
    public bool NeedCreate => usedList.Count < maxNum;

    /// <summary>
    /// 初始化抽屉
    /// </summary>
    /// <param name="root">柜子（缓存池）父对象</param>
    /// <param name="name">抽屉父对象的名字</param>
    /// <param name="usedObj">第一个被创建并使用的对象</param>
    public PoolData(GameObject root, string name, GameObject usedObj)
    {
        //开启布局功能时才动态创建抽屉根对象
        if (PoolMgr.isOpenLayout)
        {
            rootObj = new GameObject(name);
            rootObj.transform.SetParent(root.transform);
        }
        //创建抽屉时外部已创建一个对象，将其记录到使用中容器
        PushUsedList(usedObj);

        PoolObj poolObj = usedObj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            Debug.LogError("[PoolData] 请为使用缓存池功能的预设体挂载PoolObj脚本（用于设置数量上限）");
            return;
        }
        maxNum = poolObj.maxNum;
    }

    /// <summary>
    /// 从抽屉中取出一个对象使用
    /// </summary>
    /// <returns>可用的对象</returns>
    public GameObject Pop()
    {
        GameObject obj;
        if (Count > 0)
        {
            //从未使用栈中弹出
            obj = dataStack.Pop();
            usedList.Add(obj);
        }
        else
        {
            //栈为空：复用使用中最旧的对象（索引0），移到末尾表示最新使用
            obj = usedList[0];
            usedList.RemoveAt(0);
            usedList.Add(obj);
        }

        obj.SetActive(true);
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(null);

        return obj;
    }

    /// <summary>
    /// 将对象放回抽屉（回收）
    /// </summary>
    public void Push(GameObject obj)
    {
        obj.SetActive(false);
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(rootObj.transform);
        dataStack.Push(obj);
        usedList.Remove(obj);
    }

    /// <summary>
    /// 将对象记录到使用中容器
    /// </summary>
    public void PushUsedList(GameObject obj)
    {
        usedList.Add(obj);
    }
}

/// <summary>
/// 对象池基类，用于字典中里式替换存储子类
/// </summary>
public abstract class PoolObjectBase { }

/// <summary>
/// 用于存储数据结构类、逻辑类（不继承Mono）的对象池
/// </summary>
/// <typeparam name="T">对象类型</typeparam>
public class PoolObject<T> : PoolObjectBase where T : class
{
    public Queue<T> poolObjs = new Queue<T>();
}

/// <summary>
/// 可被复用的数据结构类、逻辑类必须实现此接口
/// </summary>
public interface IPoolObject
{
    /// <summary>
    /// 回收时重置数据
    /// </summary>
    void ResetInfo();
}

/// <summary>
/// 缓存池（对象池）管理器
/// 支持GameObject池（带数量上限）和非Mono数据结构池
/// </summary>
public class PoolMgr : BaseManager<PoolMgr>
{
    //GameObject池：name -> PoolData
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    //非Mono对象池：name -> PoolObjectBase
    private Dictionary<string, PoolObjectBase> poolObjectDic = new Dictionary<string, PoolObjectBase>();
    //池子根对象
    private GameObject poolObj;
    //是否开启布局功能（在Hierarchy中创建Pool根节点组织对象）
    public static bool isOpenLayout = true;

    private PoolMgr()
    {
        if (poolObj == null && isOpenLayout)
            poolObj = new GameObject("Pool");
    }

    /// <summary>
    /// 从池中获取一个GameObject
    /// </summary>
    /// <param name="name">资源名（Resources下的路径）</param>
    /// <returns>可用的GameObject</returns>
    public GameObject GetObj(string name)
    {
        if (poolObj == null && isOpenLayout)
            poolObj = new GameObject("Pool");

        GameObject obj;
        //不存在抽屉 或 栈空且未达上限 -> 创建新对象
        if (!poolDic.ContainsKey(name) ||
            (poolDic[name].Count == 0 && poolDic[name].NeedCreate))
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            obj.name = name;

            if (!poolDic.ContainsKey(name))
                poolDic.Add(name, new PoolData(poolObj, name, obj));
            else
                poolDic[name].PushUsedList(obj);
        }
        else
        {
            //有空闲对象 或 已达上限 -> 从池中取出/复用
            obj = poolDic[name].Pop();
        }
        return obj;
    }

    /// <summary>
    /// 从池中获取一个非Mono的数据结构/逻辑类对象
    /// </summary>
    /// <typeparam name="T">对象类型，需实现IPoolObject且有无参构造</typeparam>
    /// <param name="nameSpace">命名空间前缀（用于区分同名类型）</param>
    public T GetObj<T>(string nameSpace = "") where T : class, IPoolObject, new()
    {
        string poolName = nameSpace + "_" + typeof(T).Name;
        if (poolObjectDic.ContainsKey(poolName))
        {
            PoolObject<T> pool = poolObjectDic[poolName] as PoolObject<T>;
            if (pool.poolObjs.Count > 0)
                return pool.poolObjs.Dequeue();
            return new T();
        }
        return new T();
    }

    /// <summary>
    /// 将GameObject放回池中
    /// </summary>
    /// <param name="obj">要回收的对象</param>
    public void PushObj(GameObject obj)
    {
        //若该对象从未通过GetObj取出过（池中无记录），直接销毁避免异常
        if (!poolDic.ContainsKey(obj.name))
        {
            Debug.LogWarning($"[PoolMgr] 对象 {obj.name} 不属于任何池子，直接销毁");
            GameObject.Destroy(obj);
            return;
        }
        poolDic[obj.name].Push(obj);
    }

    /// <summary>
    /// 将非Mono对象放回池中
    /// </summary>
    public void PushObj<T>(T obj, string nameSpace = "") where T : class, IPoolObject
    {
        if (obj == null) return;
        string poolName = nameSpace + "_" + typeof(T).Name;
        if (!poolObjectDic.ContainsKey(poolName))
            poolObjectDic.Add(poolName, new PoolObject<T>());
        PoolObject<T> pool = poolObjectDic[poolName] as PoolObject<T>;
        //回收前重置数据
        obj.ResetInfo();
        pool.poolObjs.Enqueue(obj);
    }

    /// <summary>
    /// 清空所有池子（切场景时调用）
    /// </summary>
    public void ClearPool()
    {
        poolDic.Clear();
        poolObj = null;
        poolObjectDic.Clear();
    }
}
