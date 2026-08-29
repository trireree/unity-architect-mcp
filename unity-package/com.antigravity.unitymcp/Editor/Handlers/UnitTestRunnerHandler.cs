#pragma warning disable CS0618, CS0619
using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    [Serializable]
    public class TestSuiteResultDto
    {
        public bool hasPassed;
        public int totalTests;
        public int passedTests;
        public int failedTests;
        public List<TestResultItemDto> results = new List<TestResultItemDto>();
    }

    [Serializable]
    public class TestResultItemDto
    {
        public string testName;
        public string status; // Passed, Failed, Inconclusive
        public float durationSeconds;
        public string failureMessage;
    }

    public static class UnitTestRunnerHandler
    {
        public static McpResponse RunUnitTests(string testMode = "EditMode")
        {
            var testModeEnum = TestMode.EditMode;
            if (testMode.Equals("PlayMode", StringComparison.OrdinalIgnoreCase))
            {
                testModeEnum = TestMode.PlayMode;
            }

            try
            {
                var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                var filter = new Filter
                {
                    testMode = testModeEnum
                };

                var callback = new TestRunnerCallback();
                testRunnerApi.RegisterCallbacks(callback);
                testRunnerApi.Execute(new ExecutionSettings(filter));

                return McpResponse.Success($"Executed {testMode} Unit Tests. Test Runner execution triggered via Unity TestRunnerApi.", testMode);
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Test execution failed: {ex.Message}");
            }
        }

        private class TestRunnerCallback : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"<color=#00ff88>[Unity Test Runner]</color> Tests Finished: Passed={result.PassCount}, Failed={result.FailCount}, Total={result.Test?.Children?.Count() ?? 0}");
            }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
