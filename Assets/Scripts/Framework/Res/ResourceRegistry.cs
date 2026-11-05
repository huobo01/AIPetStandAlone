using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注册条目类别
/// 决定条目的用途与取值方式
/// </summary>
public enum E_RegistryCategory
{
    /// <summary>通用资源（贴图/模型/音频/动画等），value 为资源加载路径</summary>
    Resource,
    /// <summary>UI 面板/皮肤替换，value 为面板类名或皮肤标识</summary>
    UI,
    /// <summary>AI 可调用能力（来自 IAIActivatable），value 为 PluginActionDef</summary>
    Capability,
    /// <summary>AI 上下文片段（来自 IAIContextProvider），value 为 AISegment，供 PromptManager 拼接</summary>
    AIContext,
    /// <summary>可兑换项（资源兑换系统），value 为 ExchangeItemDef</summary>
    ExchangeItem,
}

/// <summary>
/// 资源注册条目
/// 由插件向 ResourceRegistry 注册，供本体/其他插件取用
/// </summary>
public class RegistryEntry
{
    /// <summary>条目唯一键（同类别下唯一，如 "ui.skin.dark"）</summary>
    public string key;

    /// <summary>条目类别</summary>
    public E_RegistryCategory category;

    /// <summary>条目值（资源路径/对象/定义等，具体含义取决于 category）</summary>
    public object value;

    /// <summary>提供此条目的插件 ID（本体注册的为 "core"）</summary>
    public string providerPluginId;

    /// <summary>优先级（数值越大优先级越高，同 key 后注册且优先级高的覆盖前者）</summary>
    public int priority;

    /// <summary>显示名称（用于 UI 展示）</summary>
    public string displayName;

    /// <summary>描述</summary>
    public string description;
}

/// <summary>
/// 可兑换项定义（Excel 配置表驱动）
/// 用于资源兑换系统：消耗 A 获取 B
/// </summary>
[Serializable]
public class ExchangeItemDef
{
    /// <summary>兑换项 ID</summary>
    public string id;

    /// <summary>显示名称</summary>
    public string name;

    /// <summary>消耗资源 key 列表（RegistryEntry.key）</summary>
    public string[] costKeys;

    /// <summary>消耗数量（与 costKeys 一一对应）</summary>
    public int[] costAmounts;

    /// <summary>产出资源 key</summary>
    public string rewardKey;

    /// <summary>产出数量</summary>
    public int rewardAmount;
}

/// <summary>
/// 资源注册中心（框架核心服务，非插件）
/// 
/// 万物皆插件架构的关键枢纽：
///   - 插件通过 IResourceProvider 向此处注册资源/UI/能力/可兑换项
///   - 本体与其他插件通过此处统一取用
///   - 支持覆盖（后注册的高优先级条目覆盖同 key 旧条目），实现 UI 美化替换等场景
///   - 插件卸载时自动清理其注册的所有条目
/// 
/// 设计原则：
///   - 本体只注册不可替换的基础条目（如默认 UI 皮肤、内置能力）
///   - 插件注册的条目可随时覆盖，卸载即恢复
/// </summary>
public class ResourceRegistry : BaseManager<ResourceRegistry>
{
    //category -> (key -> RegistryEntry)
    private Dictionary<E_RegistryCategory, Dictionary<string, RegistryEntry>> registry =
        new Dictionary<E_RegistryCategory, Dictionary<string, RegistryEntry>>();

    //pluginId -> 该插件注册的所有 key 列表（用于卸载时批量清理）
    private Dictionary<string, List<string>> pluginKeys =
        new Dictionary<string, List<string>>();

    private const string CORE_PROVIDER = "core";

    private ResourceRegistry()
    {
        //预初始化各类别字典
        foreach (E_RegistryCategory cat in Enum.GetValues(typeof(E_RegistryCategory)))
            registry[cat] = new Dictionary<string, RegistryEntry>();
    }

    /// <summary>
    /// 注册一个条目
    /// </summary>
    /// <param name="category">类别</param>
    /// <param name="key">唯一键</param>
    /// <param name="value">值</param>
    /// <param name="providerPluginId">提供者插件 ID（本体传 "core"）</param>
    /// <param name="priority">优先级（同 key 高优先级覆盖低优先级）</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="description">描述</param>
    public void Register(
        E_RegistryCategory category, string key, object value,
        string providerPluginId = CORE_PROVIDER, int priority = 0,
        string displayName = null, string description = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[ResourceRegistry] 注册失败：key 为空");
            return;
        }

        var catDic = registry[category];

        //同 key 已存在：比较优先级，高优先级覆盖
        if (catDic.TryGetValue(key, out RegistryEntry existing))
        {
            if (priority >= existing.priority)
            {
                //覆盖前从旧提供者的 key 列表中移除
                RemoveFromPluginKeyList(existing.providerPluginId, key);
            }
            else
            {
                Debug.LogWarning($"[ResourceRegistry] key={key} 已存在更高优先级条目，注册被忽略");
                return;
            }
        }

        RegistryEntry entry = new RegistryEntry
        {
            key = key,
            category = category,
            value = value,
            providerPluginId = providerPluginId,
            priority = priority,
            displayName = displayName ?? key,
            description = description ?? "",
        };
        catDic[key] = entry;

        //记录到提供者的 key 列表
        if (!pluginKeys.ContainsKey(providerPluginId))
            pluginKeys[providerPluginId] = new List<string>();
        if (!pluginKeys[providerPluginId].Contains(key))
            pluginKeys[providerPluginId].Add(key);

        EventCenter.Instance.Trigger<string>(E_EventType.E_ResourceRegistered, key);
    }

    /// <summary>
    /// 注销指定类别下的某个 key
    /// </summary>
    public void Unregister(E_RegistryCategory category, string key)
    {
        if (registry[category].Remove(key))
        {
            //从所有插件的 key 列表中移除（防御性清理）
            foreach (var kvp in pluginKeys)
                kvp.Value.Remove(key);

            EventCenter.Instance.Trigger<string>(E_EventType.E_ResourceUnregistered, key);
        }
    }

    /// <summary>
    /// 注销某插件注册的所有条目
    /// 插件卸载时由 PluginManager 调用
    /// </summary>
    public void UnregisterByPlugin(string pluginId)
    {
        if (!pluginKeys.TryGetValue(pluginId, out List<string> keys))
            return;

        //复制一份避免遍历时修改
        List<string> keysCopy = new List<string>(keys);
        foreach (string key in keysCopy)
        {
            //在所有类别中查找并移除
            foreach (var catDic in registry.Values)
                catDic.Remove(key);

            EventCenter.Instance.Trigger<string>(E_EventType.E_ResourceUnregistered, key);
        }
        pluginKeys.Remove(pluginId);
    }

    /// <summary>
    /// 获取指定类别与 key 的条目
    /// </summary>
    public RegistryEntry Get(E_RegistryCategory category, string key)
    {
        if (registry[category].TryGetValue(key, out RegistryEntry entry))
            return entry;
        return null;
    }

    /// <summary>
    /// 获取指定类别下的所有条目
    /// </summary>
    public List<RegistryEntry> GetAll(E_RegistryCategory category)
    {
        return new List<RegistryEntry>(registry[category].Values);
    }

    /// <summary>
    /// 检查某 key 是否已注册
    /// </summary>
    public bool Contains(E_RegistryCategory category, string key)
    {
        return registry[category].ContainsKey(key);
    }

    /// <summary>
    /// 获取某插件注册的所有 key
    /// </summary>
    public List<string> GetKeysByPlugin(string pluginId)
    {
        if (pluginKeys.TryGetValue(pluginId, out List<string> keys))
            return new List<string>(keys);
        return new List<string>();
    }

    private void RemoveFromPluginKeyList(string pluginId, string key)
    {
        if (pluginKeys.TryGetValue(pluginId, out List<string> keys))
            keys.Remove(key);
    }
}
