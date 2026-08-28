import { z } from "zod";
export function registerAutonomousGameTools(server, client) {
    // Phase 88: Full Autonomous Game Build
    server.tool("unity_build_game", "Executes end-to-end autonomous game generation: Intent -> Architecture -> World Generation -> Character -> Vehicle -> AI -> UI -> Quality Gate in a single atomic transaction.", {
        description: z.string().describe("Game concept prompt (e.g. 'third-person open-world crime game in an urban setting')"),
        quality: z.enum(["prototype", "high", "ultra"]).optional().describe("Build quality target (default 'prototype')"),
    }, async (args) => {
        const res = await client.execute({
            action: "build_game_full",
            text: args.description,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Build Failed: ${res.error}\n${res.data || ""}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 60: Game Architecture Plan Generator
    server.tool("unity_generate_game_architecture", "Generates a multi-layer (World, Gameplay, AI Simulation, Presentation, Technical) game architecture blueprint with dependency graph.", {
        prompt: z.string().describe("Game design vision prompt"),
    }, async (args) => {
        const res = await client.execute({
            action: "generate_game_architecture",
            text: args.prompt,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // Phase 61: Hierarchical Task Planner
    server.tool("unity_decompose_game", "Decomposes a game concept into dependency-ordered, prioritized technical development tasks.", {
        prompt: z.string().describe("Game concept prompt to decompose"),
    }, async (args) => {
        const res = await client.execute({
            action: "decompose_game",
            text: args.prompt,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // Phase 87: Intent Compiler
    server.tool("unity_compile_intent", "Compiles natural language game requests into structured game design intent specifications.", {
        prompt: z.string().describe("Natural language user game request"),
    }, async (args) => {
        const res = await client.execute({
            action: "compile_intent",
            text: args.prompt,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    });
    // Phase 63: Procedural World Engine 2.0
    server.tool("unity_generate_world", "Generates a seed-based procedural open world with districts, asphalt road grid, buildings, and spawn anchors.", {
        seed: z.number().optional().describe("Random seed for deterministic world generation (default 48392)"),
        districts: z.number().optional().describe("District size (default 3)"),
    }, async (args) => {
        const res = await client.execute({
            action: "generate_world",
            count: args.districts || 3,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 62: Asset Intelligence 2.0
    server.tool("unity_inspect_assets_v2", "Inspects all project asset files, texture counts, material dependencies, and sizes.", {}, async () => {
        const res = await client.execute({ action: "inspect_assets_v2" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
    // Phase 81: Project Optimization
    server.tool("unity_optimize_project", "Applies automatic safe performance optimizations (static batching flags, camera far clipping) and reports draw call savings.", {}, async () => {
        const res = await client.execute({ action: "optimize_project" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Phase 94: Engine Capabilities & Self-Awareness
    server.tool("unity_capabilities", "Returns engine capabilities, verified pipelines, active Unity version, render pipeline, and system limitations.", {}, async () => {
        const res = await client.execute({ action: "engine_capabilities" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
}
