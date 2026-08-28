import { z } from "zod";
export function registerEvolutionTools(server, client) {
    // Phase 21: LLM-Assisted Minimal Repair Context
    server.tool("unity_repair_context", "Generates a minimal, token-efficient repair payload for the host LLM containing the exact error, code snippet around the line, relevant graph dependencies, and targeted Unity API knowledge to patch cleanly.", {}, async () => {
        const res = await client.execute({ action: "repair_context" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // Phase 25 & 26: Impact Analysis
    server.tool("unity_analyze_impact", "Calculates the blast radius and risk level (LOW, MEDIUM, HIGH, CRITICAL) before modifying or deleting an asset, script, or GameObject.", {
        target: z.string().describe("Asset path, script name, or GameObject identifier to analyze"),
        operation: z.enum(["DELETE", "MODIFY", "RENAME"]).optional().describe("Planned operation (default DELETE)"),
    }, async (args) => {
        const res = await client.execute({
            action: "analyze_impact",
            target: args.target,
            text: args.operation || "DELETE",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
    // Phase 27, 28 & 29: Automated Playtest Engine
    server.tool("unity_run_playtest", "Executes automated runtime smoke test verifying object presence, attached components, active camera, and clean console without exceptions.", {
        targetObject: z.string().optional().describe("GameObject to verify in playtest (default 'PlayerCharacter')"),
    }, async (args) => {
        const res = await client.execute({
            action: "run_playtest",
            target: args.targetObject || "PlayerCharacter",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Playtest Failed: ${res.error}\n${res.data || ""}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 35 & 36: Procedural City Framework
    server.tool("unity_generate_city", "Generates a seed-based procedural city layout with roads, districts, building blocks, and player spawn points.", {
        seed: z.number().optional().describe("Random seed for reproducible city generation (default 48392)"),
        gridSize: z.number().optional().describe("Grid width/height (default 4)"),
    }, async (args) => {
        const res = await client.execute({
            action: "generate_city",
            count: args.gridSize || 4,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 38: Quality Gate
    server.tool("unity_quality_gate", "Evaluates project quality score (0 - 100) combining C# compilation, scene integrity, gameplay readiness, and performance.", {}, async () => {
        const res = await client.execute({ action: "quality_gate" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 44: Package Intelligence
    server.tool("unity_inspect_packages", "Inspects Unity Package Manager manifest for key dependencies (URP, Input System, Cinemachine, TextMeshPro, AI Navigation).", {}, async () => {
        const res = await client.execute({ action: "inspect_packages" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
}
