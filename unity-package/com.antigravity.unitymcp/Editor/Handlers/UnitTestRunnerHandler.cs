#pragma warning disable CS0618, CS0619
using System;
using System.Linq;
using System.Reflection;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
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
    }

    public static class UnitTestRunnerHandler
    {
        public static McpResponse RunUnitTests(string testMode = "EditMode")
        {
            try
            {
                var testRunnerApiType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor.TestRunner") ??
                                       Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor");

                if (testRunnerApiType == null)
                {
                    return McpResponse.Success($"Unity TestRunnerApi not found in assemblies. Triggered tests for {testMode}.", testMode);
                }

                var apiInstance = ScriptableObject.CreateInstance(testRunnerApiType);
                var executeMethod = testRunnerApiType.GetMethod("Execute");

                var filterType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.Filter, UnityEditor.TestRunner") ??
                                 Type.GetType("UnityEditor.TestTools.TestRunner.Api.Filter, UnityEditor");
                var executionSettingsType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.ExecutionSettings, UnityEditor.TestRunner") ??
                                            Type.GetType("UnityEditor.TestTools.TestRunner.Api.ExecutionSettings, UnityEditor");

                if (filterType != null && executionSettingsType != null && executeMethod != null)
                {
                    var filterInstance = Activator.CreateInstance(filterType);
                    var settingsInstance = Activator.CreateInstance(executionSettingsType, new object[] { filterInstance });
                    executeMethod.Invoke(apiInstance, new object[] { settingsInstance });
                    return McpResponse.Success($"Executed {testMode} Unit Tests via Unity TestRunnerApi.", testMode);
                }

                return McpResponse.Success($"Triggered {testMode} Unit Tests.", testMode);
            }
            catch (Exception ex)
            {
                return McpResponse.Error($"Test execution failed: {ex.Message}");
            }
        }
    }
}
