using System;

/// <summary>
/// 插件接口 —— 万物皆插件架构的核心契约
/// 所有功能插件（应用插件、游戏插件、功能插件）必须实现此接口
/// v1 单机版：插件以内置形式编译在主程序集中，或仅加载 Addressables 资源
/// 未来扩展：通过 Lua 热更加载外部脚本，接口无需变更
/// </summary>
public interface IPlugin
{
    /// <summary>插件唯一标识，如 "com.aipet.wallpaper"</summary>
    string PluginId { get; }

    /// <summary>插件显示名称</summary>
    string PluginName { get; }

    /// <summary>插件版本</summary>
    Version Version { get; }

    /// <summary>
    /// 插件加载时调用
    /// 在此完成资源加载、事件注册、面板注册等初始化工作
    /// </summary>
    void OnLoad();

    /// <summary>
    /// 插件卸载时调用
    /// 在此释放资源、取消事件监听、清理面板
    /// </summary>
    void OnUnload();

    /// <summary>
    /// 每帧更新（可选实现，不需要可留空）
    /// </summary>
    /// <param name="deltaTime">帧间隔时间</param>
    void OnUpdate(float deltaTime);
}
