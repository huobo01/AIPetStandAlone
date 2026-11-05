using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管家插件（StewardPlugin）
/// 
/// 定位：本体内置的核心插件，承担多重身份：
///   (a) 新手教程引导者（脚本模式，固定对话树）
///   (b) 跨界通讯联络员（异常反馈叙事化包装）
///   (c) 资源管理入口（宠物图鉴/物品背包/插件管理的便捷入口）
///   (d) 可选宠物化（AI 模式下成为桌宠，接受 AI 调度与聊天）
/// 
/// 架构：内核（StewardCore）+ 外核（StewardSkin 皮套）
///   - 内核自知是管家/工具，仅在 AI 模式下智能沟通
///   - 外核是皮套人设/身体，可替换身份，切换皮套同步切换人设
/// 
/// 注意：管家的物种/皮套配置属于本插件，随插件删除一同消失，不影响应用本体
/// </summary>
public class StewardPlugin : PluginBase, IAIActivatable, IDataExportable, IResourceProvider
{
    public override string PluginId => "com.aipet.steward";
    public override string PluginName => "管家";

    /// <summary>管家内核</summary>
    public StewardCore Core { get; private set; }

    public override void OnLoad()
    {
        Core = new StewardCore();
        Debug.Log("[StewardPlugin] 管家插件已加载，内核启动");
    }

    public override void OnUnload()
    {
        Debug.Log("[StewardPlugin] 管家插件已卸载");
    }

    //========================================================================
    // IAIActivatable：向 AI 暴露管家可执行动作
    //========================================================================

    public List<PluginActionDef> GetExposedActions()
    {
        return new List<PluginActionDef>
        {
            new PluginActionDef("switch_skin", "切换管家皮套（机器人/鹦鹉/小孩），同步切换人设", true)
            {
                paramDefs = { { "skinId", "string: skin.robot / skin.parrot / skin.child" } },
            },
            new PluginActionDef("open_resource_manager", "打开资源管理面板（宠物图鉴/背包/插件）", true),
            new PluginActionDef("give_feedback", "管家以跨界通讯联络员身份反馈异常或提示", true)
            {
                paramDefs = { { "errorType", "string: timeout / connection_failed / unknown_error" } },
            },
        };
    }

    public object ExecuteAction(string actionName, object param)
    {
        switch (actionName)
        {
            case "switch_skin":
                string skinId = param?.ToString();
                if (Core.SwitchSkin(skinId))
                    return $"已切换为皮套：{Core.CurrentSkin.displayName}";
                return "切换失败，未找到指定皮套";

            case "open_resource_manager":
                //由 UI 层监听此动作事件后打开面板
                EventCenter.Instance.Trigger(E_EventType.E_StewardOpenResourceManager);
                return "正在打开资源管理面板…";

            case "give_feedback":
                string errorType = param?.ToString() ?? "unknown_error";
                return Core.GetLiaisonFeedback(errorType);

            default:
                Debug.LogWarning($"[StewardPlugin] 未知动作：{actionName}");
                return null;
        }
    }

    //========================================================================
    // IDataExportable：管家数据导出/导入
    // 管家是重要插件，删除前用户可导出当前皮套/人设自定义等数据
    //========================================================================

    public string ExportDescription =>
        "管家当前皮套选择、人设自定义、教程进度等数据。重装管家插件后可恢复。";

    public string ExportData()
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "currentSkinId", Core?.CurrentSkin?.skinId ?? "skin.robot" },
                { "currentMode", Core?.CurrentMode.ToString() ?? "Script" },
            };
            return JsonMgr.Instance.ToJson(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"[StewardPlugin] 导出数据异常：{e.Message}");
            return "{}";
        }
    }

    public bool ImportData(string jsonData)
    {
        try
        {
            var data = JsonMgr.Instance.FromJson<Dictionary<string, object>>(jsonData);
            if (data == null) return false;

            if (data.TryGetValue("currentSkinId", out object skinId))
                Core.SwitchSkin(skinId.ToString());

            if (data.TryGetValue("currentMode", out object mode))
            {
                if (Enum.TryParse<E_StewardMode>(mode.ToString(), out E_StewardMode m))
                    Core.SwitchMode(m);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[StewardPlugin] 导入数据异常：{e.Message}");
            return false;
        }
    }

    //========================================================================
    // IResourceProvider：向 ResourceRegistry 注册管家提供的资源
    // 管家注册其皮套资源，供本体或其他插件取用（如极简模式取用头像）
    //========================================================================

    public void RegisterResources()
    {
        if (Core == null) return;

        //注册每套皮套的头像资源（极简模式使用）
        foreach (var skin in Core.GetAllSkins())
        {
            string key = $"ui.avatar.{skin.skinId}";
            ResourceRegistry.Instance.Register(
                E_RegistryCategory.UI, key, skin.avatarPath,
                providerPluginId: PluginId, priority: 5,
                displayName: $"{skin.displayName}头像",
                description: $"管家插件提供的{skin.displayName}头像资源");
        }

        //注册管家能力为可兑换项示例（资源兑换系统占位）
        //实际兑换规则由 Excel 配置表驱动，此处仅注册管家自身作为可用资源
        ResourceRegistry.Instance.Register(
            E_RegistryCategory.Resource, "pet.steward", PluginId,
            providerPluginId: PluginId, priority: 10,
            displayName: "管家", description: "管家角色资源，可作为桌宠使用");
    }

    public void UnregisterResources()
    {
        //ResourceRegistry.UnregisterByPlugin 会在插件卸载时自动清理
        //此处无需重复操作，仅作接口实现
    }
}
