using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 单例模式基类（非Mono）
/// 通过反射获取私有无参构造函数实例化，保证外部无法直接new
/// 线程安全的双重检查锁定（DCL）实现
/// </summary>
/// <typeparam name="T">单例的具体类型</typeparam>
public abstract class BaseManager<T> where T : class
{
    private static T instance;

    /// <summary>
    /// 单例实例是否为null，供子类判断使用
    /// </summary>
    protected bool InstanceIsNull => instance == null;

    /// <summary>
    /// 加锁对象，保证多线程下的单例安全
    /// </summary>
    protected static readonly object lockObj = new object();

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static T Instance
    {
        get
        {
            //双重检查锁定，既保证线程安全又避免每次访问都加锁
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //利用反射得到无参私有的构造函数来实例化对象
                        //（子类需提供私有无参构造函数）
                        Type type = typeof(T);
                        ConstructorInfo info = type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null);
                        if (info != null)
                            instance = info.Invoke(null) as T;
                        else
                            Debug.LogError($"[BaseManager] 类型 {type.Name} 没有找到无参私有构造函数，请检查子类实现");
                    }
                }
            }
            return instance;
        }
    }
}
