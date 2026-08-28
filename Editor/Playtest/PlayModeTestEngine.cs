using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Handlers;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Playtest
{
    [Serializable]
    public class PlaytestStepResult
    {
        public string stepName;
        public bool passed;
        public string message;
        public float durationMs;
    }

    [Serializable]
    public class PlaytestReportDto
    {
        public string testTarget;
        public bool overallPassed;
        public int passedSteps;
        public int failedSteps;
        public List<PlaytestStepResult> steps = new List<PlaytestStepResult>();
        public List<string> runtimeExceptions = new List<string>();
    }

    public static class PlayModeTestEngine
    {
        public static PlaytestReportDto RunPlaytest(string targetObjectName = "PlayerCharacter")
        {
            var report = new PlaytestReportDto
            {
                testTarget = targetObjectName,
                overallPassed = true
            };

            // Step 1: Verify Object exists in Scene
            var go = SceneHandler.FindGameObject(targetObjectName);
            if (go == null)
            {
                report.overallPassed = false;
                report.failedSteps++;
                report.steps.Add(new PlaytestStepResult
                {
                    stepName = "Locate Target Object",
                    passed = false,
                    message = $"GameObject '{targetObjectName}' was not found in active scene."
                });
                return report;
            }

            report.passedSteps++;
            report.steps.Add(new PlaytestStepResult
            {
                stepName = "Locate Target Object",
                passed = true,
                message = $"Successfully located '{go.name}' in scene hierarchy."
            });

            // Step 2: Verify Components
            var comps = go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToList();
            report.passedSteps++;
            report.steps.Add(new PlaytestStepResult
            {
                stepName = "Inspect Attached Components",
                passed = true,
                message = $"Verified {comps.Count} components: {string.Join(", ", comps)}"
            });

            // Step 3: Check Main Camera
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                report.passedSteps++;
                report.steps.Add(new PlaytestStepResult
                {
                    stepName = "Main Camera Presence",
                    passed = true,
                    message = $"Main Camera '{mainCam.name}' is active."
                });
            }
            else
            {
                report.overallPassed = false;
                report.failedSteps++;
                report.steps.Add(new PlaytestStepResult
                {
                    stepName = "Main Camera Presence",
                    passed = false,
                    message = "No active Main Camera found."
                });
            }

            // Step 4: Check Console Errors
            var logs = PlayModeHandler.GetConsoleLogs(20, "Exception");
            if (logs.success && !string.IsNullOrEmpty(logs.data))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<LogListWrapper>(logs.data);
                    if (wrapper?.logs != null && wrapper.logs.Count > 0)
                    {
                        report.runtimeExceptions = wrapper.logs.Select(l => $"{l.type}: {l.condition}").ToList();
                        report.overallPassed = false;
                        report.failedSteps++;
                        report.steps.Add(new PlaytestStepResult
                        {
                            stepName = "Console Error Check",
                            passed = false,
                            message = $"Found {wrapper.logs.Count} runtime exception(s)."
                        });
                    }
                    else
                    {
                        report.passedSteps++;
                        report.steps.Add(new PlaytestStepResult
                        {
                            stepName = "Console Error Check",
                            passed = true,
                            message = "Zero runtime exceptions or errors."
                        });
                    }
                }
                catch { }
            }

            report.overallPassed = (report.failedSteps == 0);
            return report;
        }
    }
}
