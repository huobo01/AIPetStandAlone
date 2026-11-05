using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 插件管理器
/// 负责插件的注册、加载、卸载、更新，维护插件生命周期
/// 
/// v1 单机版实现：
///   - 内置插件：在代码中注册，编译在主程序集
///   - 资源型插件：从 Addressables 加载资源，逻辑仍在主程序集
///   - 不支持外部脚本加载（无热更新）
/// 
/// 未来扩展点（热更新）：
///   - LoadExternalPlugin 方法预留，接入 Lua 热更后可加载外部 Lua 脚本
///   - manifest 签名校验预留
/// </summary>
public class PluginManager : BaseManager<PluginManager>
{
    //已加载插件字典：pluginId -> IPlugin
    private Dictionary<string, IPlugin> loadedPlugins = new Dictionary<string, IPlugin>();
    //插件清单字典：pluginId -> PluginManifest
    private Dictionary<string, PluginManifest> manifestDic = new Dictionary<string, PluginManifest>();
    //插件资源 AB 包引用计数（防止重复加载/卸载）
    private Dictionary<string, int> pluginAbRefCount = new Dictionary<string, int>();

    private PluginManager()
    {
        //注册到 MonoMgr 的 Update 循环，驱动所有插件的 OnUpdate
        MonoMgr.Instance.AddUpdateListener(UpdateAllPlugins);
    }

    /// <summary>
    /// 注册内置插件（v1 主要加载方式）
    /// </summary>
    /// <param name="plugin">插件实例</param>
    /// <param name="manifest">插件清单</param>
    public void RegisterBuiltInPlugin(IPlugin plugin, PluginManifest manifest)
    {
        if (plugin == null || manifest == null)
        {
            Debug.LogError("[PluginManager] 注册插件失败：插件或清单为空");
            return;
        }

        if (loadedPlugins.ContainsKey(plugin.PluginId))
        {
            Debug.LogWarning($"[PluginManager] 插件 {plugin.PluginId} 已注册，跳过");
            return;
        }

        manifestDic[plugin.PluginId] = manifest;
        loadedPlugins[plugin.PluginId] = plugin;
    }

    /// <summary>
    /// 加载所有已注册的内置插件
    /// 通常在游戏启动时调用
    /// </summary>
    public void LoadAllBuiltInPlugins()
    {
        foreach (var kvp in loadedPlugins)
        {
            LoadPlugin(kvp.Key);
        }
    }

    /// <summary>
    /// 加载指定插件
    /// 1. 检查依赖是否已加载
    /// 2. 加载插件资源 AB 包（若有）
    /// 3. 调用 OnLoad
    /// </summary>
    public bool LoadPlugin(string pluginId)
    {
        if (!loadedPlugins.TryGetValue(pluginId, out IPlugin plugin))
        {
            Debug.LogError($"[PluginManager] 加载失败：未找到插件 {pluginId}");
            return false;
        }

        PluginManifest manifest = manifestDic[pluginId];

        //检查依赖
        if (manifest.dependencies != null)
        {
            foreach (string depId in manifest.dependencies)
            {
                if (!loadedPlugins.ContainsKey(depId))
                {
                    Debug.LogError($"[PluginManager] 插件 {pluginId} 依赖 {depId} 未加载");
                    return false;
                }
            }
        }

        //加载插件资源 AB 包
        if (!string.IsNullOrEmpty(manifest.abName))
        {
            if (!pluginAbRefCount.ContainsKey(manifest.abName))
                pluginAbRefCount[manifest.abName] = 0;
            pluginAbRefCount[manifest.abName]++;
            //AddrMgr 会缓存已加载的资源，重复加载不会出错
        }

        try
        {
            //若插件实现 IResourceProvider，注册其资源到 ResourceRegistry
            if (plugin is IResourceProvider resourceProvider)
            {
                try { resourceProvider.RegisterResources(); }
                catch (Exception e)
                {
                    Debug.LogError($"[PluginManager] 插件 {pluginId} 注册资源异常：{e.Message}");
                }
            }

            //若插件实现 IAIActivatable，将其能力注册到 ResourceRegistry
            if (plugin is IAIActivatable activatable)
            {
                try { RegisterPluginCapabilities(pluginId, activatable); }
                catch (Exception e)
                {
                    Debug.LogError($"[PluginManager] 插件 {pluginId} 注册能力异常：{e.Message}");
                }
            }

            plugin.OnLoad();
            EventCenter.Instance.Trigger<string>(E_EventType.E_PluginLoaded, pluginId);
            Debug.Log($"[PluginManager] 插件加载成功：{plugin.PluginName} v{plugin.Version}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PluginManager] 插件 {pluginId} OnLoad 异常：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 将 IAIActivatable 插件的动作注册为 ResourceRegistry 能力条目
    /// </summary>
    private void RegisterPluginCapabilities(string pluginId, IAIActivatable activatable)
    {
        var actions = activatable.GetExposedActions();
        if (actions == null) return;

        foreach (var action in actions)
        {
            string key = $"capability.{pluginId}.{action.actionName}";
            ResourceRegistry.Instance.Register(
                E_RegistryCategory.Capability, key, action,
                providerPluginId: pluginId, priority: 10,
                displayName: action.actionName, description: action.description);
        }
    }

    /// <summary>
    /// 卸载指定插件
    /// 1. 若为重要插件且实现 IDataExportable，触发导出提示事件（由 UI 层处理）
    /// 2. 调用 OnUnload
    /// 3. 若实现 IResourceProvider，注销资源
    /// 4. 从 ResourceRegistry 清理该插件注册的所有条目
    /// 5. 释放插件资源 AB 包
    /// </summary>
    public void UnloadPlugin(string pluginId)
    {
        if (!loadedPlugins.TryGetValue(pluginId, out IPlugin plugin))
            return;

        //重要插件且实现数据导出：触发导出提示事件
        if (manifestDic.TryGetValue(pluginId, out PluginManifest manifest) &&
            manifest.isImportant && plugin is IDataExportable exportable)
        {
            EventCenter.Instance.Trigger<string>(E_EventType.E_PluginDataExportRequested, pluginId);
            Debug.Log($"[PluginManager] 重要插件 {pluginId} 请求数据导出，已触发事件");
        }

        try
        {
            plugin.OnUnload();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PluginManager] 插件 {pluginId} OnUnload 异常：{e.Message}");
        }

        //若实现 IResourceProvider，注销资源
        if (plugin is IResourceProvider resourceProvider)
        {
            try { resourceProvider.UnregisterResources(); }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 插件 {pluginId} 注销资源异常：{e.Message}");
            }
        }

        //从 ResourceRegistry 清理该插件注册的所有条目（能力/资源/UI 等）
        ResourceRegistry.Instance.UnregisterByPlugin(pluginId);

        //释放资源 AB 包
        if (manifest != null && !string.IsNullOrEmpty(manifest.abName))
        {
            if (pluginAbRefCount.ContainsKey(manifest.abName))
            {
                pluginAbRefCount[manifest.abName]--;
                if (pluginAbRefCount[manifest.abName] <= 0)
                {
                    AddrMgr.Instance.UnLoadAB(manifest.abName, null);
                    pluginAbRefCount.Remove(manifest.abName);
                }
            }
        }

        EventCenter.Instance.Trigger<string>(E_EventType.E_PluginUnloaded, pluginId);
        Debug.Log($"[PluginManager] 插件已卸载：{pluginId}");
    }

    /// <summary>
    /// 卸载所有插件
    /// </summary>
    public void UnloadAllPlugins()
    {
        List<string> ids = new List<string>(loadedPlugins.Keys);
        for (int i = 0; i < ids.Count; i++)
            UnloadPlugin(ids[i]);
    }

    /// <summary>
    /// 获取已加载的插件实例
    /// </summary>
    public IPlugin GetPlugin(string pluginId)
    {
        loadedPlugins.TryGetValue(pluginId, out IPlugin plugin);
        return plugin;
    }

    /// <summary>
    /// 获取插件清单
    /// </summary>
    public PluginManifest GetManifest(string pluginId)
    {
        manifestDic.TryGetValue(pluginId, out PluginManifest manifest);
        return manifest;
    }

    /// <summary>
    /// 导出指定插件的数据（若实现 IDataExportable）
    /// 由 UI 层在用户确认导出后调用
    /// </summary>
    /// <returns>JSON 数据包，不支持导出时返回 null</returns>
    public string ExportPluginData(string pluginId)
    {
        if (!loadedPlugins.TryGetValue(pluginId, out IPlugin plugin))
            return null;

        if (plugin is IDataExportable exportable)
        {
            try { return exportable.ExportData(); }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 插件 {pluginId} 导出数据异常：{e.Message}");
                return null;
            }
        }
        return null;
    }

    /// <summary>
    /// 向指定插件导入数据（若实现 IDataExportable）
    /// 重装插件后由应用本体调用
    /// </summary>
    public bool ImportPluginData(string pluginId, string jsonData)
    {
        if (!loadedPlugins.TryGetValue(pluginId, out IPlugin plugin))
            return false;

        if (plugin is IDataExportable exportable)
        {
            try { return exportable.ImportData(jsonData); }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 插件 {pluginId} 导入数据异常：{e.Message}");
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 插件是否已加载
    /// </summary>
    public bool IsPluginLoaded(string pluginId)
    {
        return loadedPlugins.ContainsKey(pluginId);
    }

    /// <summary>
    /// 获取所有已加载插件 ID 列表
    /// </summary>
    public List<string> GetAllPluginIds()
    {
        return new List<string>(loadedPlugins.Keys);
    }

    //驱动所有已加载插件的 OnUpdate
    private void UpdateAllPlugins()
    {
        float deltaTime = Time.deltaTime;
        foreach (var plugin in loadedPlugins.Values)
        {
            try
            {
                plugin.OnUpdate(deltaTime);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PluginManager] 插件 {plugin.PluginId} OnUpdate 异常：{e.Message}");
            }
        }
    }

    //====================================================================
    // 未来热更新扩展点（v1 不实现，仅预留接口结构）
    //====================================================================

    /// <summary>
    /// 从外部路径加载插件（预留，v1 不实现）
    /// 未来接入 Lua 热更后实现：
    ///   1. 校验 plugin.json 签名
    ///   2. 下载/校验 Addressables 资源包
    ///   3. XLua 加载热更 Lua 脚本
    ///   4. 反射实例化 IPlugin 入口类
    /// </summary>
    public void LoadExternalPlugin(string manifestPath)
    {
        Debug.LogWarning("[PluginManager] LoadExternalPlugin 在 v1 单机版中未实现，需接入 Lua 热更后启用");
    }
}
