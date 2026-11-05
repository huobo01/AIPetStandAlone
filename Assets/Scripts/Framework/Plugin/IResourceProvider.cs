/// <summary>
/// 资源提供者插件接口（可选实现）
/// 实现此接口的插件可向 ResourceRegistry 注册资源/UI/能力等，
/// 供本体或其他插件按需取用，实现 UI 美化替换等场景
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// 向资源注册中心注册本插件提供的资源/UI/能力
    /// 通常在 OnLoad 中调用 ResourceRegistry.Instance.Register(...)
    /// </summary>
    void RegisterResources();

    /// <summary>
    /// 从资源注册中心注销本插件提供的所有资源
    /// 通常在 OnUnload 中调用 ResourceRegistry.Instance.UnregisterByPlugin(pluginId)
    /// </summary>
    void UnregisterResources();
}
