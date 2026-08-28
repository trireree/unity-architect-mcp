import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerIntelligenceTools(server: any, client: UnityClient) {
  // Query Context
  server.tool(
    "unity_query_context",
    "Answers natural language questions about the project graph (e.g. 'Where is Player?', 'What scripts are attached to Car?', 'What objects use PlayerController?').",
    {
      query: z.string().describe("Question or object name to locate"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "query_context",
        query: args.query,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || "{}" }] };
    }
  );

  // Asset Dependencies
  server.tool(
    "unity_asset_dependencies",
    "Retrieves deep dependency tree for an asset (e.g. Prefab -> Mesh -> Material -> Texture).",
    {
      path: z.string().describe("Asset path (e.g. 'Assets/Prefabs/Player.prefab')"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "asset_dependencies",
        path: args.path,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || "{}" }] };
    }
  );

  // Find Duplicates
  server.tool(
    "unity_find_duplicates",
    "Finds duplicate assets (textures, models, audio) in the project by content hash and file size to optimize project storage.",
    {
      folder: z.string().optional().describe("Folder to scan (defaults to 'Assets')"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "find_duplicates",
        path: args.folder || "Assets",
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || "{}" }] };
    }
  );
}
