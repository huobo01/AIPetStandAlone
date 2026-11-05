/// <summary>
/// 事件类型枚举
/// 各模块共用的事件定义在此统一管理
/// 如需添加项目特定事件，可在对应模块自行扩展或在此追加
/// </summary>
public enum E_EventType
{
    /// <summary>
    /// 场景切换进度变化 —— 参数：float (0~1)
    /// </summary>
    E_SceneLoadChange,

    /// <summary>
    /// 水平轴输入 (-1~1) —— 参数：float
    /// </summary>
    E_Input_Horizontal,

    /// <summary>
    /// 竖直轴输入 (-1~1) —— 参数：float
    /// </summary>
    E_Input_Vertical,

    /// <summary>
    /// 测试事件 —— 参数：无
    /// </summary>
    E_Test,

    //========================================================================
    // 插件系统事件（PluginManager 使用）
    //========================================================================

    /// <summary>
    /// 插件加载完成 —— 参数：string (pluginId)
    /// </summary>
    E_PluginLoaded,

    /// <summary>
    /// 插件卸载 —— 参数：string (pluginId)
    /// </summary>
    E_PluginUnloaded,

    /// <summary>
    /// 重要插件数据导出请求 —— 参数：string (pluginId)
    /// UI 层监听此事件，弹出导出确认对话框
    /// </summary>
    E_PluginDataExportRequested,

    //========================================================================
    // 资源注册中心事件（ResourceRegistry 使用）
    //========================================================================

    /// <summary>
    /// 资源/UI/能力注册完成 —— 参数：string (key)
    /// </summary>
    E_ResourceRegistered,

    /// <summary>
    /// 资源/UI/能力注销 —— 参数：string (key)
    /// </summary>
    E_ResourceUnregistered,

    //========================================================================
    // 模式切换事件（ModeManager 使用）
    //========================================================================

    /// <summary>
    /// 模式切换中 —— 参数：ModeType (目标模式)
    /// </summary>
    E_ModeChanging,

    /// <summary>
    /// 模式切换完成 —— 参数：ModeType (当前模式)
    /// </summary>
    E_ModeChanged,

    //========================================================================
    // 对话事件（DialogueManager 使用）
    //========================================================================

    /// <summary>
    /// 收到 AI 文字回复 —— 参数：string (回复内容)
    /// </summary>
    E_ChatMessageReceived,

    /// <summary>
    /// 用户发送消息 —— 参数：string (消息内容)
    /// </summary>
    E_ChatMessageSent,

    /// <summary>
    /// 对话历史更新 —— 参数：无
    /// </summary>
    E_ChatHistoryUpdated,

    //========================================================================
    // 宠物事件（Pet 系统使用）
    //========================================================================

    /// <summary>
    /// 宠物数值变化 —— 参数：无
    /// </summary>
    E_PetStatsChanged,

    /// <summary>
    /// 情感变化 —— 参数：string (情感标签)
    /// </summary>
    E_PetEmotionChanged,

    /// <summary>
    /// 动作执行 —— 参数：string (动作名)
    /// </summary>
    E_PetActionPerformed,

    /// <summary>
    /// 宠物被点击 —— 参数：无
    /// </summary>
    E_PetClicked,

    //========================================================================
    // 设置事件（SettingPanel 使用）
    //========================================================================

    /// <summary>
    /// 设置变更 —— 参数：无
    /// </summary>
    E_SettingsChanged,

    //========================================================================
    // 管家事件（StewardPlugin 使用）
    //========================================================================

    /// <summary>
    /// 管家皮套切换 —— 参数：string (skinId)
    /// </summary>
    E_StewardSkinChanged,

    /// <summary>
    /// 管家运行模式切换 —— 参数：E_StewardMode
    /// </summary>
    E_StewardModeChanged,

    /// <summary>
    /// 管家请求打开资源管理面板 —— 参数：无
    /// </summary>
    E_StewardOpenResourceManager,
}
