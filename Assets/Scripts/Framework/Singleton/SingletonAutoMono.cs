using UnityEngine;

/// <summary>
/// 自动挂载式 Mono 单例基类（推荐使用）
/// 无需手动挂载，首次访问 Instance 时自动创建GameObject并挂载脚本
/// 过场景时不会被销毁
/// </summary>
/// <typeparam name="T">单例的具体类型，必须继承MonoBehaviour</typeparam>
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //动态创建空物体并以类名命名，方便在编辑器中识别
                GameObject obj = new GameObject(typeof(T).Name);
                //动态挂载对应的单例脚本
                instance = obj.AddComponent<T>();
                //过场景时不移除，保证整个生命周期都存在
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
}
