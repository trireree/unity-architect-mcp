#pragma warning disable CS0618, CS0619
using System;
using System.Diagnostics;
using System.IO;
using Antigravity.UnityMCP.Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Antigravity.UnityMCP.Editor.Handlers
{
    public static class GitAndVersionControlHandler
    {
        public static McpResponse GetGitStatus()
        {
            var result = RunGitCommand("status --short");
            return McpResponse.Success("Retrieved Git working directory status.", result);
        }

        public static McpResponse GetGitDiff()
        {
            var result = RunGitCommand("diff --stat");
            return McpResponse.Success("Retrieved Git diff summary.", result);
        }

        public static McpResponse AutoCommit(string commitMessage)
        {
            if (string.IsNullOrEmpty(commitMessage)) commitMessage = "feat: automated architecture changes via Unity Architect MCP";
            RunGitCommand("add .");
            var result = RunGitCommand($"commit -m \"{commitMessage}\"");
            return McpResponse.Success($"Git commit executed: {commitMessage}", result);
        }

        public static McpResponse RollbackGit()
        {
            var result = RunGitCommand("checkout .");
            AssetDatabase.Refresh();
            return McpResponse.Success("Rolled back working directory to HEAD.", result);
        }

        private static string RunGitCommand(string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit(5000);
                    return string.IsNullOrEmpty(output) ? error : output;
                }
            }
            catch (Exception ex)
            {
                return $"Git Error: {ex.Message}";
            }
        }
    }
}
