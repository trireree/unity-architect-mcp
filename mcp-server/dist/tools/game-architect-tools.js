import { z } from "zod";
export function registerGameArchitectTools(server, client) {
    // Scaffold System
    server.tool("unity_scaffold_system", "Creates production-grade, modular C# gameplay pattern scripts (e.g. 'player', 'vehicle', 'weapon', 'health', 'inventory', 'enemy', 'interaction', 'saveload', 'objectpool', 'daynight', 'police', 'traffic').", {
        systemName: z.enum([
            "player",
            "vehicle",
            "weapon",
            "health",
            "inventory",
            "enemy",
            "interaction",
            "saveload",
            "objectpool",
            "daynight",
            "police",
            "traffic",
        ]).describe("System template name"),
        targetPath: z.string().optional().describe("Directory to save script (defaults to 'Assets/Scripts')"),
    }, async (args) => {
        const res = await client.execute({
            action: "scaffold_system",
            name: args.systemName,
            path: args.targetPath,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "System scaffolded successfully." }] };
    });
    // Architect Full Game Prototype
    server.tool("unity_architect_game", "Generates a complete multi-system 3D open-world prototype (City grid with buildings/ground, 3rd Person Player, Drivable Police Car, Weapon & Health systems, Wanted stars HUD, Day/Night lighting) in a single atomic transaction with auto-validation.", {
        genre: z.string().optional().describe("Game genre / template (defaults to 'OpenWorldCrime')"),
    }, async (args) => {
        const res = await client.execute({
            action: "architect_game",
            name: args.genre || "OpenWorldCrime",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Architect Failed: ${res.error}\n${res.data || ""}` }] };
        return { content: [{ type: "text", text: `🎮 Game Prototype Architecture Completed Successfully!\n${res.message}\nDetails:\n${res.data || ""}` }] };
    });
}
