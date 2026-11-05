using System;

/// <summary>
/// 插件清单数据结构
/// 对应插件目录下的 plugin.json 文件
/// v1：内置插件的清单在代码中构造；资源型插件从 AB 包内的 plugin.json 读取
/// 未来：外部插件下载后解析此清单并校验签名
/// </summary>
[Serializable]
public class PluginManifest
{
    /// <summary>插件唯一标识，需与 IPlugin.PluginId 一致</summary>
    public string id;

    /// <summary>插件显示名称</summary>
    public string name;

    /// <summary>版本号字符串，如 "1.0.0"</summary>
    public string version;

    /// <summary>作者</summary>
    public string author;

    /// <summary>插件描述</summary>
    public string description;

    /// <summary>入口类全名（命名空间.类名），v1 内置插件使用</summary>
    public string entryClass;

    /// <summary>依赖插件 ID 列表，加载前需确保依赖已加载</summary>
    public string[] dependencies;

    /// <summary>插件资源 AB 包名（可选，无资源则为空）</summary>
    public string abName;

    /// <summary>是否为内置插件（v1 全部为 true）</summary>
    public bool isBuiltIn;

    /// <summary>
    /// 是否为重要插件
    /// 重要插件卸载/删除前，若实现 IDataExportable，应用会提示用户导出数据
    /// </summary>
    public bool isImportant;

    /// <summary>
    /// 插件分类标签（如 "ui" / "game" / "pet" / "ai" / "system"）
    /// 用于插件管理器面板分类展示
    /// </summary>
    public string category;

    /// <summary>将版本字符串转为 Version 对象</summary>
    public Version GetVersion()
    {
        if (Version.TryParse(version, out Version ver))
            return ver;
        return new Version(0, 0, 0);
    }
}
