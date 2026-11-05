using UnityEngine;

/// <summary>
/// 挂载式 Mono 单例基类
/// 需手动将脚本挂载到场景中的GameObject上
/// 注意：Awake中请先调用 base.Awake()
/// </summary>
/// <typeparam name="T">单例的具体类型，必须继承MonoBehaviour</typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance => instance;

    protected virtual void Awake()
    {
        //已存在实例则销毁新挂载的整个GameObject，避免重复单例
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
        //依附的对象过场景时不会被移除，保证整个生命周期都存在
        DontDestroyOnLoad(gameObject);
    }
}
