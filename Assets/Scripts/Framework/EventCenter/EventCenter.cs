using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 事件信息基类，用于里式替换原则，用父类容器装载子类对象
/// </summary>
public abstract class EventInfoBase { }

/// <summary>
/// 带一个泛型参数的事件信息类，记录观察者的函数委托
/// </summary>
/// <typeparam name="T">事件参数类型</typeparam>
public class EventInfo<T> : EventInfoBase
{
    //观察者对应的函数信息
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

/// <summary>
/// 无参数的事件信息类
/// </summary>
public class EventInfo : EventInfoBase
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件中心模块
/// 提供事件的订阅、取消订阅、触发功能，实现观察者模式
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    //事件名 -> 事件信息 的映射表
    private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();

    private EventCenter() { }

    /// <summary>
    /// 触发带参数的事件
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="eventName">事件名</param>
    /// <param name="info">传递的参数</param>
    public void Trigger<T>(E_EventType eventName, T info)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
            (eventInfo as EventInfo<T>)?.actions?.Invoke(info);
    }

    /// <summary>
    /// 触发无参数的事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    public void Trigger(E_EventType eventName)
    {
        if (eventDic.TryGetValue(eventName, out EventInfoBase eventInfo))
            (eventInfo as EventInfo)?.actions?.Invoke();
    }

    /// <summary>
    /// 添加带参数的事件监听
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="eventName">事件名</param>
    /// <param name="func">响应函数</param>
    public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }

    /// <summary>
    /// 添加无参数的事件监听
    /// </summary>
    public void AddEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions += func;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo(func));
        }
    }

    /// <summary>
    /// 移除带参数的事件监听
    /// </summary>
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo<T>).actions -= func;
    }

    /// <summary>
    /// 移除无参数的事件监听
    /// </summary>
    public void RemoveEventListener(E_EventType eventName, UnityAction func)
    {
        if (eventDic.ContainsKey(eventName))
            (eventDic[eventName] as EventInfo).actions -= func;
    }

    /// <summary>
    /// 清空所有事件监听
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }

    /// <summary>
    /// 清除指定事件的所有监听
    /// </summary>
    /// <param name="eventName">事件名</param>
    public void ClearEvent(E_EventType eventName)
    {
        if (eventDic.ContainsKey(eventName))
            eventDic.Remove(eventName);
    }
}
