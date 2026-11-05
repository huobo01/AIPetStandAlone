using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源加载统一入口
/// 开发时可通过 isDebug 开关切换为 EditorResMgr 直接加载资源（跳过 Addressables），
/// 发布后强制走 AddrMgr 的 Addressables 加载流程
/// </summary>
public class ABResMgr : BaseManager<ABResMgr>
{
    //true=编辑器下走EditorResMgr加载；false=走AddrMgr加载
    private bool isDebug = true;

    private ABResMgr() { }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="abName">资源组名（对应 Addressables 中的资源组/路径前缀）</param>
    /// <param name="resName">资源名</param>
    /// <param name="callBack">加载完成回调</param>
    /// <param name="isSync">是否同步加载</param>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack, bool isSync = false) where T : Object
    {
#if UNITY_EDITOR
        if (isDebug)
        {
            //编辑器调试模式：直接从工程目录加载资源，无需构建 Addressables
            T res = EditorResMgr.Instance.LoadEditorRes<T>($"{abName}/{resName}");
            callBack?.Invoke(res as T);
        }
        else
        {
            AddrMgr.Instance.LoadResAsync<T>(abName, resName, callBack, isSync);
        }
#else
        AddrMgr.Instance.LoadResAsync<T>(abName, resName, callBack, isSync);
#endif
    }
}
