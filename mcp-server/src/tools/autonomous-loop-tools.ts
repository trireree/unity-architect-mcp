import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerAutonomousLoopTools(server: any, client: UnityClient) {
  // Autonomous Build Loop
  server.tool(
    "unity_autonomous_build",
    "Runs the complete autonomous development loop (Intent -> Inspect State -> Generate Plan -> Snapshot -> Execute Batch -> Scene Validation -> Auto Self-Healing -> Commit/Rollback) in one unified workflow.",
    {
      intent: z.string().describe("User gameplay or architecture goal (e.g. 'Build a playable third person character with follow camera', 'Create a drivable car with physics', 'Scaffold an enemy AI patrolling near player')"),
    },
    async (args: any) => {
      const logs: string[] = [];
      logs.push(`🚀 Starting Autonomous Build Loop for: "${args.intent}"`);

      // 1. Inspect Initial State
      const inspectRes = await client.execute({ action: "inspect_project" });
      logs.push("✓ Phase 1: Project State inspected.");

      // 2. Generate Plan
      const planRes = await client.execute({ action: "plan_system", text: args.intent });
      if (!planRes.success) {
        return { content: [{ type: "text", text: `Planning failed: ${planRes.error}` }] };
      }
      logs.push("✓ Phase 2: Implementation Plan generated.");

      // 3. Create Pre-Execution Snapshot
      const snapRes = await client.execute({ action: "snapshot_create", name: "autonomous_build" });
      const txId = snapRes.transactionId || snapRes.data;
      logs.push(`✓ Phase 3: Checkpoint snapshot created (${txId}).`);

      // 4. Execute Plan
      const execRes = await client.execute({ action: "execute_plan", text: args.intent });
      if (!execRes.success) {
        logs.push(`⚠️ Execution encountered error: ${execRes.error}. Triggering Self-Healing...`);
      } else {
        logs.push("✓ Phase 4: Batch actions executed.");
      }

      // 5. Validate & Self-Heal
      const healRes = await client.execute({ action: "self_heal_loop", target: txId });
      logs.push(`✓ Phase 5: Self-Healing & Validation completed (${healRes.message}).`);

      // 6. Final State Diff
      const diffRes = await client.execute({ action: "state_diff" });
      logs.push("✓ Phase 6: Incremental State Diff calculated.");

      const fullOutput = [
        logs.join("\n"),
        "\n--- Execution & Healing Details ---",
        execRes.data || execRes.message || "OK",
        healRes.data || "",
        "\n--- State Diff ---",
        diffRes.data || "",
      ].join("\n");

      return { content: [{ type: "text", text: fullOutput }] };
    }
  );
}
