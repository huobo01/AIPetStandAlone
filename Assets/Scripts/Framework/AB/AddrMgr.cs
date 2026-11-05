using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressables 资源加载管理器
/// 替代原 ABMgr（AssetBundle 加载器），使用 Unity Addressables 系统
/// 优势：自动管理依赖、内置缓存与引用计数、支持本地/远程资源、无需手动维护主包与依赖
/// 
/// 接口与原 ABMgr 保持一致（LoadResAsync / UnLoadAB / ClearAB），
/// 上层 ABResMgr、UIMgr、MusicMgr、PluginManager 无需修改调用方式。
/// </summary>
public class AddrMgr : SingletonAutoMono<AddrMgr>
{
    //已加载资源的句柄字典（key = "abName/resName"，value = AsyncOperationHandle）
    //用于卸载时查找对应句柄并释放
    private Dictionary<string, AsyncOperationHandle> handleDic = new Dictionary<string, AsyncOperationHandle>();

    //Addressables 是否已初始化
    private bool isInitialized = false;

    /// <summary>
    /// 初始化 Addressables 系统（加载资源前必须先初始化）
    /// </summary>
    public IEnumerator InitializeAsync()
    {
        if (isInitialized)
            yield break;

        AsyncOperationHandle initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        isInitialized = initHandle.Status == AsyncOperationStatus.Succeeded;

        if (!isInitialized)
            Debug.LogError("[AddrMgr] Addressables 初始化失败");
    }

    /// <summary>
    /// 同步初始化（等待完成）
    /// </summary>
    public void InitializeSync()
    {
        if (isInitialized)
            return;
        Addressables.InitializeAsync().WaitForCompletion();
        isInitialized = true;
    }

    /// <summary>
    /// 组合资源地址：abName/resName
    /// Addressables 中的地址由资源组的路径决定，与原 AB 包名/资源名对应
    /// </summary>
    private string GetAddress(string abName, string resName)
    {
        return $"{abName}/{resName}";
    }

    /// <summary>
    /// 泛型异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="abName">资源组名（对应原 AB 包名）</param>
    /// <param name="resName">资源名</param>
    /// <param name="callBack">加载完成回调</param>
    /// <param name="isSync">是否同步加载</param>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync = false) where T : Object
    {
        StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callBack, isSync));
    }

    private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync) where T : Object
    {
        //确保 Addressables 已初始化
        if (!isInitialized)
            yield return InitializeAsync();

        string address = GetAddress(abName, resName);

        //已加载则直接返回缓存
        if (handleDic.TryGetValue(address, out AsyncOperationHandle cachedHandle))
        {
            //等待缓存加载完成
            if (!cachedHandle.IsDone)
                yield return cachedHandle;

            if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
                callBack?.Invoke(cachedHandle.Result as T);
            else
                callBack?.Invoke(null);
            yield break;
        }

        if (isSync)
        {
            //同步加载
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T res = handle.WaitForCompletion();
            handleDic[address] = handle;
            callBack?.Invoke(res);
        }
        else
        {
            //异步加载
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            handleDic[address] = handle;
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                callBack?.Invoke(handle.Result);
            else
            {
                Debug.LogError($"[AddrMgr] 资源加载失败：{address}，{handle.OperationException}");
                callBack?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Type 异步加载资源
    /// </summary>
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync = false)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, type, callBack, isSync));
    }

    private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack, bool isSync)
    {
        if (!isInitialized)
            yield return InitializeAsync();

        string address = GetAddress(abName, resName);

        if (handleDic.TryGetValue(address, out AsyncOperationHandle cachedHandle))
        {
            if (!cachedHandle.IsDone)
                yield return cachedHandle;
            callBack?.Invoke(cachedHandle.Status == AsyncOperationStatus.Succeeded ? cachedHandle.Result : null);
            yield break;
        }

        if (isSync)
        {
            AsyncOperationHandle handle = Addressables.LoadAssetAsync(address, type);
            Object res = handle.WaitForCompletion() as Object;
            handleDic[address] = handle;
            callBack?.Invoke(res);
        }
        else
        {
            AsyncOperationHandle handle = Addressables.LoadAssetAsync(address, type);
            handleDic[address] = handle;
            yield return handle;
            callBack?.Invoke(handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null);
        }
    }

    /// <summary>
    /// 按名字异步加载资源
    /// </summary>
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callBack, bool isSync = false)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, callBack, isSync));
    }

    private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<Object> callBack, bool isSync)
    {
        if (!isInitialized)
            yield return InitializeAsync();

        string address = GetAddress(abName, resName);

        if (handleDic.TryGetValue(address, out AsyncOperationHandle cachedHandle))
        {
            if (!cachedHandle.IsDone)
                yield return cachedHandle;
            callBack?.Invoke(cachedHandle.Status == AsyncOperationStatus.Succeeded ? cachedHandle.Result : null);
            yield break;
        }

        if (isSync)
        {
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<Object>(address);
            Object res = handle.WaitForCompletion();
            handleDic[address] = handle;
            callBack?.Invoke(res);
        }
        else
        {
            AsyncOperationHandle handle = Addressables.LoadAssetAsync<Object>(address);
            handleDic[address] = handle;
            yield return handle;
            callBack?.Invoke(handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null);
        }
    }

    /// <summary>
    /// 卸载指定资源
    /// </summary>
    /// <param name="name">资源地址（abName/resName）或资源组名</param>
    /// <param name="callBackResult">卸载结果回调</param>
    public void UnLoadAB(string name, UnityAction<bool> callBackResult)
    {
        //尝试按完整地址查找
        if (handleDic.TryGetValue(name, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle);
            handleDic.Remove(name);
            callBackResult?.Invoke(true);
            return;
        }

        //尝试按资源组前缀批量卸载（name = abName）
        bool anyReleased = false;
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in handleDic)
        {
            if (kvp.Key.StartsWith(name + "/"))
            {
                Addressables.Release(kvp.Value);
                keysToRemove.Add(kvp.Key);
                anyReleased = true;
            }
        }
        foreach (var key in keysToRemove)
            handleDic.Remove(key);

        callBackResult?.Invoke(anyReleased);
    }

    /// <summary>
    /// 清空所有已加载资源
    /// </summary>
    public void ClearAB()
    {
        StopAllCoroutines();
        foreach (var handle in handleDic.Values)
            Addressables.Release(handle);
        handleDic.Clear();
    }
}
