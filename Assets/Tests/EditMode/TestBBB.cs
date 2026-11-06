using NUnit.Framework;
using System;
using UnityEngine;
using System.IO;

/// <summary>
/// AIPet EditMode 测试集
/// 覆盖：Runtime 程序集可达性 + 插件清单版本解析 + Excel→.Bo→BinaryDataMgr 链路
/// </summary>
public class TestBBB
{
    // ========== 原冒烟测试（Runtime 程序集可达性检查） ==========

    [Test]
    public void RuntimeAssembly_IsReachable()
    {
        Assert.IsNotNull(typeof(PluginManifest), "测试程序集无法引用 AIPet.Runtime");
    }

    [Test]
    public void PluginManifest_ParseVersion_Valid()
    {
        PluginManifest manifest = new PluginManifest();
        manifest.version = "1.2.3";
        Version version = manifest.GetVersion();
        Assert.AreEqual(1, version.Major);
        Assert.AreEqual(2, version.Minor);
        Assert.AreEqual(3, version.Build);
    }

    [Test]
    public void PluginManifest_ParseVersion_Invalid_ReturnsZero()
    {
        PluginManifest manifest = new PluginManifest();
        manifest.version = "not-a-version";
        Version version = manifest.GetVersion();
        Assert.AreEqual(0, version.Major);
        Assert.AreEqual(0, version.Minor);
        Assert.AreEqual(0, version.Build);
    }

    // ========== Excel → .Bo → BinaryDataMgr 链路测试 ==========

    private static string BoPath => Path.Combine(Application.streamingAssetsPath, "Binary", "PluginRegistry.Bo");

    [Test]
    public void LoadTable_Container_NotNull()
    {
        Assert.IsTrue(File.Exists(BoPath), $"PluginRegistry.Bo 不存在，请先跑 ExcelTool。路径={BoPath}");
        BinaryDataMgr.Instance.LoadTable<PluginRegistryContainer, PluginRegistry>();
        var container = BinaryDataMgr.Instance.GetTable<PluginRegistryContainer>();
        Assert.IsNotNull(container);
        Assert.IsNotNull(container.dataDic);
        Assert.IsTrue(container.dataDic.Count >= 1);
    }

    [Test]
    public void LoadTable_StewardPlugin_KeyFields()
    {
        Assert.IsTrue(File.Exists(BoPath), $"PluginRegistry.Bo 不存在，请先跑 ExcelTool。");
        BinaryDataMgr.Instance.LoadTable<PluginRegistryContainer, PluginRegistry>();
        var container = BinaryDataMgr.Instance.GetTable<PluginRegistryContainer>();
        Assert.IsTrue(container.dataDic.ContainsKey("com.aipet.steward"));

        var steward = container.dataDic["com.aipet.steward"];
        Assert.AreEqual("管家", steward.name);
        Assert.AreEqual("1.0.0", steward.version);
        Assert.IsTrue(steward.isBuiltIn);
        Assert.IsTrue(steward.isImportant);
        Assert.AreEqual("system", steward.category);
    }

    [Test]
    public void BoFile_SizeInRange()
    {
        Assert.IsTrue(File.Exists(BoPath), $"PluginRegistry.Bo 不存在，请先跑 ExcelTool。");
        var info = new FileInfo(BoPath);
        Assert.IsTrue(info.Length > 0);
        Assert.IsTrue(info.Length >= 100 && info.Length <= 500);
    }
}
