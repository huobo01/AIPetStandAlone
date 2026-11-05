using UnityEngine;

/// <summary>
/// 游戏主入口 / 生命周期管理器
/// 负责框架初始化、插件加载、模式切换的总调度
/// 挂载在启动场景的 GameObject 上
/// </summary>
public class GameManager : SingletonMono<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        InitFramework();
    }

    private void Start()
    {
        InitPlugins();
    }

    /// <summary>
    /// 初始化框架各管理器
    /// 访问 Instance 即触发懒加载构造
    /// </summary>
    private void InitFramework()
    {
        //触发各管理器的单例构造（构造函数中完成初始化）
        _ = MonoMgr.Instance;
        _ = EventCenter.Instance;
        _ = PoolMgr.Instance;
        _ = ResMgr.Instance;
        _ = AddrMgr.Instance;
        _ = ABResMgr.Instance;
        _ = UIMgr.Instance;
        _ = MusicMgr.Instance;
        _ = InputMgr.Instance;
        _ = TimerMgr.Instance;
        _ = SceneMgr.Instance;
        _ = ResourceRegistry.Instance;
        _ = PluginManager.Instance;
        _ = ModeManager.Instance;
    }

    /// <summary>
    /// 初始化插件系统
    /// 在此注册所有内置插件并加载
    /// </summary>
    private void InitPlugins()
    {
        //注册管家插件（内置核心插件，提供新手引导/异常反馈/资源管理入口）
        PluginManager.Instance.RegisterBuiltInPlugin(
            new StewardPlugin(),
            new PluginManifest
            {
                id = "com.aipet.steward",
                name = "管家",
                version = "1.0.0",
                author = "AIPet",
                description = "内置核心插件：新手引导、跨界通讯联络员、资源管理入口、可选桌宠化",
                entryClass = "StewardPlugin",
                dependencies = new string[0],
                abName = "",
                isBuiltIn = true,
                isImportant = true,
                category = "system",
            });

        //TODO: Iter-2 起逐步注册各内置功能插件
        //PluginManager.Instance.RegisterBuiltInPlugin(new WallpaperPlugin(), new PluginManifest { ... });

        PluginManager.Instance.LoadAllBuiltInPlugins();
    }

    private void OnDestroy()
    {
        //卸载所有插件
        PluginManager.Instance.UnloadAllPlugins();
    }
}
