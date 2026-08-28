import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerScriptTools(server: any, client: UnityClient) {
  // Create / Update Script
  server.tool(
    "unity_script_create",
    "Creates or updates a C# script file in the project and triggers Unity compilation pipeline.",
    {
      path: z.string().describe("Path to the script (e.g. 'Assets/Scripts/GameManager.cs')"),
      content: z.string().describe("Full C# source code content"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "script_create",
        path: args.path,
        content: args.content,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: `${res.message} (Path: ${res.data})` }] };
    }
  );

  // Get Compilation Status
  server.tool(
    "unity_script_get_compilation_status",
    "Checks if Unity Editor is currently compiling scripts or updating asset database.",
    {},
    async () => {
      const res = await client.execute({ action: "script_status" });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: `${res.message} (Status: ${res.data})` }] };
    }
  );

  // Evaluate C# (REPL)
  server.tool(
    "unity_csharp_eval",
    "Executes arbitrary C# Editor code directly in Unity without saving a file or waiting for compilation. Returns the result.",
    {
      code: z.string().describe("C# code body to execute (can use UnityEditor, UnityEngine namespaces, return values)"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "csharp_eval",
        code: args.code,
      });
      if (!res.success) return { content: [{ type: "text", text: `Execution Failed:\n${res.error}` }] };
      return { content: [{ type: "text", text: `Result: ${res.data}` }] };
    }
  );
}
