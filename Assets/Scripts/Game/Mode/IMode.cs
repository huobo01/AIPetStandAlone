/// <summary>
/// 运行模式枚举
/// </summary>
public enum ModeType
{
    /// <summary>极简模式：仅头像+对话，极致性能</summary>
    Minimal = 0,
    /// <summary>日常模式：完整互动桌宠</summary>
    Interactive = 1,
    /// <summary>游戏模式：全屏窗口+小游戏</summary>
    Window = 2,
}

/// <summary>
/// 模式接口 —— 策略模式
/// 三种运行模式各自实现此接口，由 ModeManager 统一调度
/// </summary>
public interface IMode
{
    /// <summary>模式类型</summary>
    ModeType ModeType { get; }

    /// <summary>进入模式时调用（加载资源、显示 UI）</summary>
    void Enter();

    /// <summary>退出模式时调用（卸载资源、隐藏 UI）</summary>
    void Exit();

    /// <summary>每帧更新</summary>
    void Update(float deltaTime);
}
