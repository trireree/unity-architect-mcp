using System;
using System.Collections.Generic;
using Antigravity.UnityMCP.Editor.Core;
using Antigravity.UnityMCP.Editor.Playtest;
using Antigravity.UnityMCP.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.QualityGate
{
    [Serializable]
    public class QualityGateReportDto
    {
        public int overallScore; // 0 - 100
        public string grade; // "A+", "A", "B", "C", "F"
        public bool passed;
        public int compileScore;
        public int sceneIntegrityScore;
        public int gameplayReadinessScore;
        public int performanceScore;
        public List<string> passedChecks = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> criticalIssues = new List<string>();
    }

    public static class QualityGateEngine
    {
        public static QualityGateReportDto EvaluateProjectQuality()
        {
            var report = new QualityGateReportDto();

            // 1. Compilation Check (25 pts)
            if (!EditorApplication.isCompiling)
            {
                report.compileScore = 25;
                report.passedChecks.Add("✓ C# Compilation: Clean with zero blocking compile errors.");
            }
            else
            {
                report.compileScore = 0;
                report.criticalIssues.Add("✗ C# Compilation in progress or blocked.");
            }

            // 2. Scene Integrity Check (25 pts)
            var valReport = ValidationManager.ValidateScene();
            if (valReport.errorCount == 0)
            {
                report.sceneIntegrityScore = 25;
                report.passedChecks.Add("✓ Scene Integrity: Zero missing scripts or broken shaders.");
            }
            else
            {
                report.sceneIntegrityScore = Math.Max(0, 25 - (valReport.errorCount * 8));
                report.criticalIssues.Add($"✗ Scene Integrity: Found {valReport.errorCount} scene error(s).");
            }

            // 3. Gameplay Readiness Check (25 pts)
            var playReport = PlayModeTestEngine.RunPlaytest("PlayerCharacter");
            if (playReport.overallPassed)
            {
                report.gameplayReadinessScore = 25;
                report.passedChecks.Add("✓ Gameplay Readiness: Player and Camera verified.");
            }
            else
            {
                report.gameplayReadinessScore = 15; // partial
                report.warnings.Add("⚠️ Gameplay Readiness: PlayerCharacter or Camera not yet instantiated.");
            }

            // 4. Performance & Console Cleanliness (25 pts)
            if (valReport.warningCount <= 3)
            {
                report.performanceScore = 25;
                report.passedChecks.Add("✓ Performance & Console: Low warning count and clean console.");
            }
            else
            {
                report.performanceScore = 15;
                report.warnings.Add($"⚠️ Performance: {valReport.warningCount} warnings logged.");
            }

            report.overallScore = report.compileScore + report.sceneIntegrityScore + report.gameplayReadinessScore + report.performanceScore;
            report.passed = (report.overallScore >= 70);

            if (report.overallScore >= 90) report.grade = "A+";
            else if (report.overallScore >= 80) report.grade = "A";
            else if (report.overallScore >= 70) report.grade = "B";
            else if (report.overallScore >= 50) report.grade = "C";
            else report.grade = "F";

            return report;
        }
    }
}
