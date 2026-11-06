using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

/// <summary>
/// CLI 测试运行器
/// 用法：Unity.exe -batchmode -executeMethod TestRunnerCLI.RunAllTests -quit
/// runSynchronously=true 让 Execute 阻塞直到 EditMode 测试全部完成
/// </summary>
public static class TestRunnerCLI
{
    public static void RunAllTests()
    {
        Debug.Log("===== [TestRunnerCLI] Start =====");

        var settings = new ExecutionSettings(new Filter { testMode = TestMode.EditMode })
        {
            runSynchronously = true
        };

        int passed = 0, failed = 0, total = 0;

        var runner = new TestRunnerApi();
        runner.RegisterCallbacks(new CallbacksImpl(
            onRunStarted: tests => {
                total = tests.TestCaseCount;
                Debug.Log($"[TestRunnerCLI] RunStarted: {total} test cases");
            },
            onTestFinished: result => {
                if (result.TestStatus == TestStatus.Passed) {
                    passed++;
                    Debug.Log($"  ✅ {result.Name}");
                } else if (result.TestStatus == TestStatus.Failed) {
                    failed++;
                    Debug.LogError($"  ❌ FAILED {result.Name}: {result.Message}");
                }
                // Skipped / Inconclusive 不计入 passed/failed
            }
        ));

        runner.Execute(settings);

        Debug.Log($"===== [TestRunnerCLI] Done: {passed} passed, {failed} failed / {total} total =====");

        Environment.ExitCode = failed > 0 ? 1 : 0;
    }

    private class CallbacksImpl : ICallbacks
    {
        private readonly Action<ITestAdaptor> onRunStarted;
        private readonly Action<ITestResultAdaptor> onTestFinished;

        public CallbacksImpl(Action<ITestAdaptor> onRunStarted, Action<ITestResultAdaptor> onTestFinished)
        {
            this.onRunStarted = onRunStarted;
            this.onTestFinished = onTestFinished;
        }

        public void RunStarted(ITestAdaptor testsToRun) => onRunStarted?.Invoke(testsToRun);
        public void RunFinished(ITestResultAdaptor result) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) => onTestFinished?.Invoke(result);
    }
}
