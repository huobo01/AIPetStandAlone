using System;

/// <summary>
/// 插件抽象基类（可选继承，简化 IPlugin 实现）
/// 提供默认实现，子类只需重写需要的方法
/// </summary>
public abstract class PluginBase : IPlugin
{
    public abstract string PluginId { get; }
    public abstract string PluginName { get; }
    public virtual Version Version => new Version(1, 0, 0);

    public virtual void OnLoad() { }
    public virtual void OnUnload() { }
    public virtual void OnUpdate(float deltaTime) { }
}
