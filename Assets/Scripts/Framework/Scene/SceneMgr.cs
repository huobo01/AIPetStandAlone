using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换管理器
/// 支持同步/异步切换场景，异步切换时通过事件中心广播加载进度
/// </summary>
public class SceneMgr : BaseManager<SceneMgr>
{
    private SceneMgr() { }

    /// <summary>同步切换场景</summary>
    public void LoadScene(string name, UnityAction callBack = null)
    {
        SceneManager.LoadScene(name);
        callBack?.Invoke();
    }

    /// <summary>异步切换场景</summary>
    public void LoadSceneAsyn(string name, UnityAction callBack = null)
    {
        MonoMgr.Instance.StartCoroutine(ReallyLoadSceneAsyn(name, callBack));
    }

    private IEnumerator ReallyLoadSceneAsyn(string name, UnityAction callBack)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        while (!ao.isDone)
        {
            //通过事件中心每帧广播加载进度
            EventCenter.Instance.Trigger<float>(E_EventType.E_SceneLoadChange, ao.progress);
            yield return 0;
        }
        //确保最后一帧进度为1
        EventCenter.Instance.Trigger<float>(E_EventType.E_SceneLoadChange, 1f);

        callBack?.Invoke();
    }
}
