using System.Collections.Generic;

/// <summary>
/// 插件可向 AI 暴露的动作定义
/// AI 通过 actions[] 数组中的 type=plugin 项调用此动作
/// </summary>
public class PluginActionDef
{
    /// <summary>动作名称（AI 调用时使用的标识）</summary>
    public string actionName;

    /// <summary>动作描述（拼入 System Prompt，让 AI 理解何时调用）</summary>
    public string description;

    /// <summary>参数定义（key=参数名，value=类型说明，用于 Prompt）</summary>
    public Dictionary<string, string> paramDefs;

    /// <summary>是否允许 AI 自主触发（否则仅用户主动操作时触发）</summary>
    public bool allowSelfTrigger;

    public PluginActionDef(string actionName, string description, bool allowSelfTrigger = false)
    {
        this.actionName = actionName;
        this.description = description;
        this.allowSelfTrigger = allowSelfTrigger;
        paramDefs = new Dictionary<string, string>();
    }
}

/// <summary>
/// AI 可激活插件接口（可选实现）
/// 实现此接口的插件可向 AI 暴露可调用动作，
/// AI 在输出的 actions[] 中以 type=plugin 项调用
/// </summary>
public interface IAIActivatable
{
    /// <summary>
    /// 获取该插件向 AI 暴露的动作清单
    /// PromptManager 会将清单拼入 System Prompt
    /// </summary>
    List<PluginActionDef> GetExposedActions();

    /// <summary>
    /// 执行 AI 调用的动作
    /// </summary>
    /// <param name="actionName">动作名称</param>
    /// <param name="param">动作参数（JSON 对象或简单值）</param>
    /// <returns>执行结果（可返回描述性文字供 AI 继续推理）</returns>
    object ExecuteAction(string actionName, object param);
}
