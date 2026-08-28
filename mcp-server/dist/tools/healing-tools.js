import { z } from "zod";
export function registerHealingTools(server, client) {
    // Run Self-Healing Loop
    server.tool("unity_self_heal", "Runs an automated self-healing loop up to MAX_REPAIR_ATTEMPTS (3). Detects C# compile errors, missing namespaces, missing components and applies automated patches. Auto-rolls back if unrepairable.", {
        transactionId: z.string().optional().describe("Active transaction ID to rollback if healing fails"),
    }, async (args) => {
        const res = await client.execute({
            action: "self_heal_loop",
            target: args.transactionId,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Healing Failed: ${res.error}\n${res.data || ""}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Diagnose Errors
    server.tool("unity_diagnose_errors", "Classifies all active C# compiler errors, runtime exceptions, missing scripts, and broken shaders with suggested root-cause fixes.", {}, async () => {
        const res = await client.execute({ action: "diagnose_errors" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
}
