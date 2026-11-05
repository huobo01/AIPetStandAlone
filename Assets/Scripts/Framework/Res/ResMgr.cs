using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源信息基类，用于里式替换原则，父类容器装子类对象
/// </summary>
public abstract class ResInfoBase
{
    //引用计数
    public int refCount;
}

/// <summary>
/// 资源信息对象，存储资源、异步加载委托、协程信息
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class ResInfo<T> : ResInfoBase
{
    //加载的资源
    public T asset;
    //异步加载结束后的回调委托
    public UnityAction<T> callBack;
    //异步加载时开启的协程
    public Coroutine coroutine;
    //引用计数归零时是否立即移除
    public bool isDel;

    /// <summary>
    /// 引用计数+1
    /// </summary>
    public void AddRefCount()
    {
        ++refCount;
    }

    /// <summary>
    /// 引用计数-1
    /// </summary>
    public void SubRefCount()
    {
        --refCount;
        if (refCount < 0)
            Debug.LogError("[ResInfo] 引用计数小于0，请检查加载和卸载是否配对执行");
    }
}

/// <summary>
/// Resources 资源加载管理器
/// 支持同步/异步加载，带引用计数管理，资源复用避免重复加载
/// </summary>
public class ResMgr : BaseManager<ResMgr>
{
    //资源字典：resName -> ResInfoBase
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();

    private ResMgr() { }

    /// <summary>
    /// 构建资源唯一ID（路径_类型名）
    /// </summary>
    private string GetResName(string path, Type type)
    {
        return path + "_" + type.Name;
    }

    /// <summary>
    /// 同步加载Resources下资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">Resources下的路径</param>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string resName = GetResName(path, typeof(T));
        if (!resDic.ContainsKey(resName))
        {
            T res = Resources.Load<T>(path);
            ResInfo<T> info = new ResInfo<T>();
            info.asset = res;
            info.AddRefCount();
            resDic.Add(resName, info);
            return res;
        }
        else
        {
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            info.AddRefCount();
            //如果之前在异步加载中，这里转为同步加载
            if (info.asset == null)
            {
                MonoMgr.Instance.StopCoroutine(info.coroutine);
                T res = Resources.Load<T>(path);
                info.asset = res;
                //执行等待中的回调
                info.callBack?.Invoke(res);
                info.callBack = null;
                info.coroutine = null;
                return res;
            }
            else
            {
                return info.asset;
            }
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">Resources下的路径</param>
    /// <param name="callBack">加载完成回调</param>
    public void LoadAsync<T>(string path, UnityAction<T> callBack) where T : UnityEngine.Object
    {
        string resName = GetResName(path, typeof(T));
        if (!resDic.ContainsKey(resName))
        {
            ResInfo<T> info = new ResInfo<T>();
            info.AddRefCount();
            resDic.Add(resName, info);
            info.callBack += callBack;
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }
        else
        {
            ResInfo<T> info = resDic[resName] as ResInfo<T>;
            info.AddRefCount();
            if (info.asset == null)
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }
    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        ResourceRequest rq = Resources.LoadAsync<T>(path);
        yield return rq;

        string resName = GetResName(path, typeof(T));
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            resInfo.asset = rq.asset as T;

            //引用计数为0且标记为待删除，则移除资源
            if (resInfo.refCount == 0)
                UnloadAsset<T>(path, resInfo.isDel, null, false);
            else
            {
                resInfo.callBack?.Invoke(resInfo.asset);
                //清空回调和协程引用，避免内存泄漏
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }
        }
    }

    /// <summary>
    /// 按Type异步加载资源（非泛型版本）
    /// </summary>
    [Obsolete("建议使用泛型加载方式。若必须用Type加载，请勿与泛型加载混用同路径同类型资源")]
    public void LoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callBack)
    {
        string resName = GetResName(path, type);
        if (!resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> info = new ResInfo<UnityEngine.Object>();
            info.AddRefCount();
            resDic.Add(resName, info);
            info.callBack += callBack;
            info.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path, type));
        }
        else
        {
            ResInfo<UnityEngine.Object> info = resDic[resName] as ResInfo<UnityEngine.Object>;
            info.AddRefCount();
            if (info.asset == null)
                info.callBack += callBack;
            else
                callBack?.Invoke(info.asset);
        }
    }

    private IEnumerator ReallyLoadAsync(string path, Type type)
    {
        ResourceRequest rq = Resources.LoadAsync(path, type);
        yield return rq;

        string resName = GetResName(path, type);
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            resInfo.asset = rq.asset;
            if (resInfo.refCount == 0)
                UnloadAsset(path, type, resInfo.isDel, null, false);
            else
            {
                resInfo.callBack?.Invoke(resInfo.asset);
                resInfo.callBack = null;
                resInfo.coroutine = null;
            }
        }
    }

    /// <summary>
    /// 卸载指定资源（泛型版本）
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <param name="isDel">引用计数归零时是否立即从内存移除</param>
    /// <param name="callBack">异步加载中取消的回调（从回调列表中移除）</param>
    /// <param name="isSub">是否减少引用计数（内部递归调用时传false）</param>
    public void UnloadAsset<T>(string path, bool isDel = false, UnityAction<T> callBack = null, bool isSub = true)
    {
        string resName = GetResName(path, typeof(T));
        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> resInfo = resDic[resName] as ResInfo<T>;
            if (isSub)
                resInfo.SubRefCount();
            resInfo.isDel = isDel;
            //已加载完成且引用计数归零且标记删除 -> 真正卸载
            if (resInfo.asset != null && resInfo.refCount == 0 && resInfo.isDel)
            {
                resDic.Remove(resName);
                Resources.UnloadAsset(resInfo.asset as UnityEngine.Object);
            }
            else if (resInfo.asset == null)
            {
                //正在异步加载中：仅移除回调，不直接卸载
                if (callBack != null)
                    resInfo.callBack -= callBack;
            }
        }
    }

    /// <summary>
    /// 卸载指定资源（Type版本）
    /// </summary>
    public void UnloadAsset(string path, Type type, bool isDel = false, UnityAction<UnityEngine.Object> callBack = null, bool isSub = true)
    {
        string resName = GetResName(path, type);
        if (resDic.ContainsKey(resName))
        {
            ResInfo<UnityEngine.Object> resInfo = resDic[resName] as ResInfo<UnityEngine.Object>;
            if (isSub)
                resInfo.SubRefCount();
            resInfo.isDel = isDel;
            if (resInfo.asset != null && resInfo.refCount == 0 && resInfo.isDel)
            {
                resDic.Remove(resName);
                Resources.UnloadAsset(resInfo.asset);
            }
            else if (resInfo.asset == null)
            {
                if (callBack != null)
                    resInfo.callBack -= callBack;
            }
        }
    }

    /// <summary>
    /// 异步卸载所有未使用的Resources资源
    /// </summary>
    public void UnloadUnusedAssets(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callBack));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callBack)
    {
        //先清除引用计数为0的记录
        List<string> list = new List<string>();
        foreach (string path in resDic.Keys)
        {
            if (resDic[path].refCount == 0)
                list.Add(path);
        }
        foreach (string path in list)
        {
            resDic.Remove(path);
        }

        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        callBack();
    }

    /// <summary>
    /// 获取指定资源的当前引用计数
    /// </summary>
    public int GetRefCount<T>(string path)
    {
        string resName = GetResName(path, typeof(T));
        if (resDic.ContainsKey(resName))
            return (resDic[resName] as ResInfo<T>).refCount;
        return 0;
    }

    /// <summary>
    /// 清空资源字典并卸载未使用资源
    /// </summary>
    public void ClearDic(UnityAction callBack)
    {
        MonoMgr.Instance.StartCoroutine(ReallyClearDic(callBack));
    }

    private IEnumerator ReallyClearDic(UnityAction callBack)
    {
        resDic.Clear();
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        callBack();
    }
}
