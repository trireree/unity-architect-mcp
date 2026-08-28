using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Antigravity.UnityMCP.Editor.Validation
{
    public static class ValidationManager
    {
        public static ValidationReportDto ValidateScene()
        {
            var report = new ValidationReportDto { isValid = true };
            var activeScene = SceneManager.GetActiveScene();

            // 1. Check Compilation Errors
            if (EditorApplication.isCompiling)
            {
                report.isValid = false;
                report.errorCount++;
                report.issues.Add(new ValidationErrorDto
                {
                    type = "CompileError",
                    target = "Editor",
                    message = "Unity is currently compiling scripts. Wait for compilation to finish.",
                    severity = "Warning"
                });
            }

            // 2. Check Cameras
            if (Camera.allCamerasCount == 0)
            {
                report.warningCount++;
                report.issues.Add(new ValidationErrorDto
                {
                    type = "MissingCamera",
                    target = activeScene.name,
                    message = "No active Camera found in the scene.",
                    severity = "Warning"
                });
            }

            // 3. Scan all GameObjects in scene
            var roots = activeScene.GetRootGameObjects();
            foreach (var root in roots)
            {
                ValidateGameObjectRecursive(root, report);
            }

            report.isValid = (report.errorCount == 0);
            return report;
        }

        private static void ValidateGameObjectRecursive(GameObject go, ValidationReportDto report)
        {
            if (go == null) return;

            // Check Missing Scripts
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.errorCount++;
                    report.isValid = false;
                    report.issues.Add(new ValidationErrorDto
                    {
                        type = "MissingScript",
                        target = go.name,
                        message = $"GameObject '{go.name}' has a Missing Component (Script deleted or broken at index {i}).",
                        severity = "Error"
                    });
                }
            }

            // Check Renderers for Missing Materials / Shaders
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null)
                    {
                        report.warningCount++;
                        report.issues.Add(new ValidationErrorDto
                        {
                            type = "MissingMaterial",
                            target = go.name,
                            message = $"Renderer on '{go.name}' has a null Material at slot {i}.",
                            severity = "Warning"
                        });
                    }
                    else if (mats[i].shader == null || mats[i].shader.name == "Hidden/InternalErrorShader")
                    {
                        report.errorCount++;
                        report.isValid = false;
                        report.issues.Add(new ValidationErrorDto
                        {
                            type = "BrokenShader",
                            target = go.name,
                            message = $"Material '{mats[i].name}' on '{go.name}' uses a broken/missing shader (Pink Material).",
                            severity = "Error"
                        });
                    }
                }
            }

            // Recurse children
            for (int i = 0; i < go.transform.childCount; i++)
            {
                ValidateGameObjectRecursive(go.transform.GetChild(i).gameObject, report);
            }
        }
    }
}
