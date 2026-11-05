using System;

/// <summary>
/// 管家皮套（外核）数据定义
/// 外核 = 身体 + 人设，可替换身份
/// 切换皮套时同步切换人设/语言风格
/// 注意：管家的物种/皮套配置属于管家插件，随插件删除一同消失，不影响应用本体
/// </summary>
[Serializable]
public class StewardSkin
{
    /// <summary>皮套唯一 ID（如 "skin.robot" / "skin.parrot" / "skin.child"）</summary>
    public string skinId;

    /// <summary>皮套显示名称</summary>
    public string displayName;

    /// <summary>物种 ID（关联管家插件内置的物种配置表）</summary>
    public string speciesId;

    /// <summary>模型资源路径（AB 包内，管家插件资源）</summary>
    public string modelPath;

    /// <summary>人设：名字</summary>
    public string personaName;

    /// <summary>人设：性格描述（拼入 System Prompt）</summary>
    public string personality;

    /// <summary>人设：说话风格（拼入 System Prompt）</summary>
    public string speechStyle;

    /// <summary>人设：背景故事</summary>
    public string backstory;

    /// <summary>头像资源路径（极简模式使用）</summary>
    public string avatarPath;

    /// <summary>表情资源路径前缀（拼接情感标签得到完整路径）</summary>
    public string expressionPathPrefix;
}
