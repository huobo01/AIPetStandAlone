using NUnit.Framework;
using System;

/// <summary>
/// 测试环境冒烟测试
/// 目的：验证 Tests.EditMode 程序集能够引用 AIPet.Runtime，并确认 NUnit 可用
/// 后续新增的单元测试都应放在同一个程序集内
/// </summary>
public class TestEnvironmentSmokeTest
{
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

        Assert.AreEqual(0, version.Major, "非法版本号应降级为 0.0.0");
        Assert.AreEqual(0, version.Minor);
        Assert.AreEqual(0, version.Build);
    }
}
