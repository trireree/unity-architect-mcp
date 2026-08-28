import { z } from "zod";
export function registerArchitectTools(server, client) {
    // 1. Inspect Project (Progressive Disclosure Summary)
    server.tool("unity_inspect_project", "Returns a lightweight, token-optimized summary of the Unity Project (scene count, object count, scripts, prefabs, materials, active hash, key scene elements) without context bloat.", {}, async () => {
        const res = await client.execute({ action: "inspect_project" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // 2. Inspect Scene
    server.tool("unity_inspect_scene", "Returns high-level structural overview of the active scene (scene hash, object count, root objects list).", {}, async () => {
        const res = await client.execute({ action: "inspect_scene" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // 3. Inspect Specific Object / Subtree (Progressive Disclosure)
    server.tool("unity_inspect_object", "Returns the detailed graph subtree, components, and relationships for a specific GameObject name or Stable ID.", {
        target: z.string().describe("GameObject name or Stable ID to inspect"),
    }, async (args) => {
        const res = await client.execute({
            action: "inspect_object",
            target: args.target,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
    // 4. State Diff
    server.tool("unity_state_diff", "Calculates the exact incremental diff (ADDED, REMOVED, MODIFIED, UNCHANGED) since the last state baseline. Saves enormous token context.", {}, async () => {
        const res = await client.execute({ action: "state_diff" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
    // 5. Snapshot Create
    server.tool("unity_snapshot", "Creates a safe checkpoint / snapshot of the current scene and assets before executing complex AI modifications.", {
        name: z.string().optional().describe("Optional label or reason for the snapshot"),
    }, async (args) => {
        const res = await client.execute({
            action: "snapshot_create",
            name: args.name,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (Transaction ID: ${res.transactionId || res.data})` }] };
    });
    // 6. Rollback
    server.tool("unity_rollback", "Atomically rolls back the last AI change or a specific Transaction ID, reverting scene and asset modifications.", {
        transactionId: z.string().optional().describe("Transaction ID to revert (e.g. 'tx_20260828_120000'). Defaults to last transaction."),
    }, async (args) => {
        const res = await client.execute({
            action: "snapshot_rollback",
            target: args.transactionId,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Rollback completed successfully." }] };
    });
    // 7. Validate Scene Integrity
    server.tool("unity_validate", "Runs full integrity verification across the active scene and project (detects missing scripts, broken shaders/pink materials, missing cameras, compilation errors).", {}, async () => {
        const res = await client.execute({ action: "validate_scene" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // 8. Execute Batch (Multi-step atomic transaction)
    server.tool("unity_execute_batch", "Executes multiple Unity operations sequentially in a single atomic transaction. Automatically validates integrity and rolls back on failure.", {
        transactionId: z.string().optional().describe("Optional transaction identifier"),
        autoRollbackOnError: z.boolean().optional().describe("Whether to rollback automatically if any action or validation fails (default true)"),
        actions: z.array(z.object({
            action: z.string().describe("Low-level action name (e.g. 'gameobject_create', 'component_add', 'component_set_property', 'script_create')"),
            target: z.string().optional(),
            name: z.string().optional(),
            path: z.string().optional(),
            content: z.string().optional(),
            primitiveType: z.string().optional(),
            componentType: z.string().optional(),
            propertyName: z.string().optional(),
            propertyValue: z.string().optional(),
            position: z.array(z.number()).length(3).optional(),
            rotation: z.array(z.number()).length(3).optional(),
            scale: z.array(z.number()).length(3).optional(),
            parent: z.string().optional(),
            tag: z.string().optional(),
            layer: z.string().optional(),
        })).describe("List of actions to execute sequentially in batch"),
    }, async (args) => {
        const res = await client.execute({
            action: "execute_batch",
            transactionId: args.transactionId,
            autoRollbackOnError: args.autoRollbackOnError ?? true,
            actions: args.actions,
        });
        if (!res.success) {
            return {
                content: [
                    { type: "text", text: `Batch Failed: ${res.error}\nDetails:\n${res.data || ""}` },
                ],
            };
        }
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
}
