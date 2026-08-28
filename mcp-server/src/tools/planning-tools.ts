import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerPlanningTools(server: any, client: UnityClient) {
  // Plan System
  server.tool(
    "unity_plan_system",
    "Generates a dependency-ordered, step-by-step implementation plan for a requested gameplay or engine system without executing it yet.",
    {
      intent: z.string().describe("System intent description (e.g. 'Third person player system', 'Drivable car with physics', 'Enemy AI with patrol', 'Weapon combat system')"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "plan_system",
        text: args.intent,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || "{}" }] };
    }
  );

  // Execute Plan
  server.tool(
    "unity_execute_plan",
    "Translates an intent into a plan and executes it atomically with validation and self-healing in a single transaction.",
    {
      intent: z.string().describe("System intent description to generate and immediately execute"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "execute_plan",
        text: args.intent,
      });
      if (!res.success) return { content: [{ type: "text", text: `Execution Failed: ${res.error}\n${res.data || ""}` }] };
      return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    }
  );
}
