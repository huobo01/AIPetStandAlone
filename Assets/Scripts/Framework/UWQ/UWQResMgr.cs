using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// UnityWebRequest 资源加载管理器
/// 用于从网络/本地加载 string、byte[]、Texture、AssetBundle 类型资源
/// </summary>
public class UWQResMgr : SingletonAutoMono<UWQResMgr>
{
    /// <summary>
    /// 通过UnityWebRequest加载资源
    /// </summary>
    /// <typeparam name="T">仅支持 string、byte[]、Texture、AssetBundle</typeparam>
    /// <param name="path">资源路径（需自带协议头 http/ftp/file）</param>
    /// <param name="callBack">加载成功回调</param>
    /// <param name="failCallBack">加载失败回调</param>
    public void LoadRes<T>(string path, UnityAction<T> callBack, UnityAction failCallBack) where T : class
    {
        StartCoroutine(ReallyLoadRes<T>(path, callBack, failCallBack));
    }

    private IEnumerator ReallyLoadRes<T>(string path, UnityAction<T> callBack, UnityAction failCallBack) where T : class
    {
        Type type = typeof(T);
        UnityWebRequest req = null;
        if (type == typeof(string) || type == typeof(byte[]))
            req = UnityWebRequest.Get(path);
        else if (type == typeof(Texture))
            req = UnityWebRequestTexture.GetTexture(path);
        else if (type == typeof(AssetBundle))
            req = UnityWebRequestAssetBundle.GetAssetBundle(path);
        else
        {
            failCallBack?.Invoke();
            yield break;
        }

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            if (type == typeof(string))
                callBack?.Invoke(req.downloadHandler.text as T);
            else if (type == typeof(byte[]))
                callBack?.Invoke(req.downloadHandler.data as T);
            else if (type == typeof(Texture))
                callBack?.Invoke(DownloadHandlerTexture.GetContent(req) as T);
            else if (type == typeof(AssetBundle))
                callBack?.Invoke(DownloadHandlerAssetBundle.GetContent(req) as T);
        }
        else
        {
            failCallBack?.Invoke();
        }
        req.Dispose();
    }
}
