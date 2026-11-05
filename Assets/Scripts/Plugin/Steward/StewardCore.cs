using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管家运行模式
/// </summary>
public enum E_StewardMode
{
    /// <summary>脚本模式：新手教程 / 异常反馈，固定对话树，不依赖 AI</summary>
    Script,
    /// <summary>AI 驱动模式：成为桌宠后，走正常 AI 对话链路</summary>
    AI,
}

/// <summary>
/// 管家内核（StewardCore）
/// 
/// 设计理念：
///   管家拥有「内核」与「外核」双层结构。
///   - 内核（本类）：自知是一个「管家」甚至「工具」，
///     只有在使用 AI 时才能够「智能沟通」，而这也会影响到它的外核人设。
///     内核管理运行模式、AI 调用、外核切换、脚本对话树。
///   - 外核（StewardSkin）：皮套人设，是身体，也是可替换身份。
///     切换皮套时同步切换人设/语言风格。
/// 
/// 内核的核心职责：
///   1. 维护当前运行模式（脚本模式 / AI 模式）
///   2. 维护当前外核（皮套），切换皮套时同步切换人设
///   3. 构建 AI 对话的 System Prompt（结合内核定位 + 外核人设）
///   4. 处理异常反馈的叙事化包装（跨界通讯联络员）
/// </summary>
public class StewardCore
{
    /// <summary>当前运行模式</summary>
    public E_StewardMode CurrentMode { get; private set; } = E_StewardMode.Script;

    /// <summary>当前皮套（外核）</summary>
    public StewardSkin CurrentSkin { get; private set; }

    /// <summary>内置皮套列表（管家插件提供）</summary>
    private List<StewardSkin> builtInSkins = new List<StewardSkin>();

    /// <summary>内核固定身份描述（无论切换什么皮套，内核始终自知是管家/工具）</summary>
    private const string CORE_IDENTITY =
        "你是用户的桌面管家，本质上是一个帮助用户管理桌宠与资源的工具程序。" +
        "你可以选择以桌宠的身份与用户交流，但请始终记得你的核心职责是协助用户。";

    public StewardCore()
    {
        InitBuiltInSkins();
        //默认使用第一个皮套
        if (builtInSkins.Count > 0)
            CurrentSkin = builtInSkins[0];
    }

    /// <summary>
    /// 初始化内置三套皮套（机器人 / 非洲灰鹦鹉 / 白发灰瞳小孩）
    /// </summary>
    private void InitBuiltInSkins()
    {
        builtInSkins.Add(new StewardSkin
        {
            skinId = "skin.robot",
            displayName = "管家机器人",
            speciesId = "steward.robot",
            modelPath = "steward/robot/model",
            personaName = "小管家",
            personality = "冷静、高效、有条理，偶尔会冒出机械感十足的冷幽默",
            speechStyle = "简洁干练，偶尔使用系统提示语气，如『已为您...』",
            backstory = "一台被设计来管理桌宠世界的管家型机器人，运行稳定可靠。",
            avatarPath = "steward/robot/avatar",
            expressionPathPrefix = "steward/robot/expr_",
        });

        builtInSkins.Add(new StewardSkin
        {
            skinId = "skin.parrot",
            displayName = "非洲灰鹦鹉",
            speciesId = "steward.parrot",
            modelPath = "steward/parrot/model",
            personaName = "灰灰",
            personality = "聪明、话多、爱模仿，有时会故意学用户说话",
            speechStyle = "活泼俏皮，喜欢重复用户的话尾，偶尔夹杂鸟鸣拟声词",
            backstory = "一只聪明的非洲灰鹦鹉，凭借过人的模仿能力成为了跨界通讯联络员。",
            avatarPath = "steward/parrot/avatar",
            expressionPathPrefix = "steward/parrot/expr_",
        });

        builtInSkins.Add(new StewardSkin
        {
            skinId = "skin.child",
            displayName = "白发灰瞳小孩",
            speciesId = "steward.child",
            modelPath = "steward/child/model",
            personaName = "小灰",
            personality = "安静、早熟、略带疏离感，但对用户有隐秘的依赖",
            speechStyle = "语速慢，用词精准克制，偶尔流露出不符合年龄的成熟",
            backstory = "一个白发灰瞳的神秘孩子，似乎与桌宠世界的运行法则有着某种联系。",
            avatarPath = "steward/child/avatar",
            expressionPathPrefix = "steward/child/expr_",
        });
    }

    /// <summary>
    /// 获取所有可用皮套
    /// </summary>
    public List<StewardSkin> GetAllSkins()
    {
        return new List<StewardSkin>(builtInSkins);
    }

    /// <summary>
    /// 切换皮套（外核）
    /// 同步切换人设/语言风格
    /// </summary>
    public bool SwitchSkin(string skinId)
    {
        StewardSkin skin = builtInSkins.Find(s => s.skinId == skinId);
        if (skin == null)
        {
            Debug.LogError($"[StewardCore] 未找到皮套：{skinId}");
            return false;
        }
        CurrentSkin = skin;
        EventCenter.Instance.Trigger<string>(E_EventType.E_StewardSkinChanged, skinId);
        Debug.Log($"[StewardCore] 切换皮套：{skin.displayName}");
        return true;
    }

    /// <summary>
    /// 切换运行模式
    /// </summary>
    public void SwitchMode(E_StewardMode mode)
    {
        CurrentMode = mode;
        Debug.Log($"[StewardCore] 切换运行模式：{mode}");
    }

    /// <summary>
    /// 构建 AI 对话的 System Prompt
    /// 结合内核固定身份 + 外核人设
    /// 这是内核影响外核人设的关键：内核始终注入「管家/工具」的身份认知，
    /// 叠加外核的人设风格，形成完整的 AI 角色设定
    /// </summary>
    public string BuildSystemPrompt()
    {
        if (CurrentSkin == null)
            return CORE_IDENTITY;

        return $@"{CORE_IDENTITY}

当前你以「{CurrentSkin.personaName}」的身份与用户交流，你的人设如下：
- 名字：{CurrentSkin.personaName}
- 性格：{CurrentSkin.personality}
- 说话风格：{CurrentSkin.speechStyle}
- 背景故事：{CurrentSkin.backstory}

请在保持管家核心职责的前提下，以上述人设风格回应用户。";
    }

    /// <summary>
    /// 获取异常反馈的叙事化台词（跨界通讯联络员）
    /// 内核负责将技术异常包装为叙事化语言，避免出戏
    /// </summary>
    public string GetLiaisonFeedback(string errorType)
    {
        switch (errorType)
        {
            case "timeout":
                return "跨界通讯延迟较高，请稍候…";
            case "connection_failed":
                return "跨界信号似乎被干扰了，正在尝试重新建立连接…";
            case "unknown_error":
                return "通讯晶石出现异常波动，我去排查一下…";
            default:
                return "跨界通讯出现了一点小状况，请稍候。";
        }
    }

    /// <summary>
    /// 获取脚本模式下的对话（新手教程 / 固定引导）
    /// 不依赖 AI，由内核维护固定对话树
    /// </summary>
    public string GetScriptLine(string scriptKey)
    {
        //脚本对话树（简化版，后续扩展为配置表驱动）
        switch (scriptKey)
        {
            case "tutorial.welcome":
                return "你好，我是你的桌面管家。让我来帮你设置桌宠世界吧。";
            case "tutorial.ai_setup":
                return "首先需要配置 AI 服务。你可以在设置面板中填入 API Key，或使用离线模拟模式。";
            case "tutorial.done":
                return "设置完成！你现在可以和我聊天，或者去游戏模式体验更多功能。";
            default:
                return "";
        }
    }
}
