import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerVisionTools(server: any, client: UnityClient) {
  // Capture Scene View
  server.tool(
    "unity_vision_capture_scene",
    "Captures an instant screenshot of the active Unity SceneView camera as an image for visual inspection of lighting, layout, and composition.",
    {
      width: z.number().optional().describe("Image width (default 1280)"),
      height: z.number().optional().describe("Image height (default 720)"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "vision_capture_scene",
        width: args.width || 1280,
        height: args.height || 720,
      });

      if (!res.success || !res.data) {
        return { content: [{ type: "text", text: `Error capturing scene: ${res.error || "No data"}` }] };
      }

      return {
        content: [
          { type: "text", text: res.message || "Captured Scene View screenshot." },
          { type: "image", data: res.data, mimeType: "image/png" },
        ],
      };
    }
  );

  // Capture Game View
  server.tool(
    "unity_vision_capture_game",
    "Captures an instant screenshot of the Main Game Camera view.",
    {
      width: z.number().optional().describe("Image width (default 1280)"),
      height: z.number().optional().describe("Image height (default 720)"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "vision_capture_game",
        width: args.width || 1280,
        height: args.height || 720,
      });

      if (!res.success || !res.data) {
        return { content: [{ type: "text", text: `Error capturing game view: ${res.error || "No data"}` }] };
      }

      return {
        content: [
          { type: "text", text: res.message || "Captured Game View screenshot." },
          { type: "image", data: res.data, mimeType: "image/png" },
        ],
      };
    }
  );

  // Inspect Object Visual
  server.tool(
    "unity_vision_inspect_object",
    "Frames the SceneView camera directly on a specific GameObject and takes a high-res screenshot for visual inspection.",
    {
      target: z.string().describe("Name or Instance ID of the GameObject to inspect visually"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "vision_inspect_object",
        target: args.target,
      });

      if (!res.success || !res.data) {
        return { content: [{ type: "text", text: `Error inspecting object: ${res.error || "No data"}` }] };
      }

      return {
        content: [
          { type: "text", text: res.message || `Framed and captured visual of '${args.target}'.` },
          { type: "image", data: res.data, mimeType: "image/png" },
        ],
      };
    }
  );
}
