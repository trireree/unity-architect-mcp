import { z } from "zod";
export function registerScaffoldingTools(server, client) {
    // Scaffold Third Person Player
    server.tool("unity_scaffold_player", "Generates a complete, ready-to-play Third-Person Character with CharacterController, movement, sprinting, jump physics, and auto-generated C# PlayerController script.", {
        name: z.string().optional().describe("Name of the Player GameObject (default 'PlayerCharacter')"),
    }, async (args) => {
        const res = await client.execute({
            action: "scaffold_player",
            name: args.name || "PlayerCharacter",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Player scaffolded successfully." }] };
    });
    // Scaffold Enemy AI
    server.tool("unity_scaffold_enemy", "Generates an intelligent Enemy NPC with NavMeshAgent, automatic player detection, chasing logic, and auto-generated C# EnemyAI script.", {
        name: z.string().optional().describe("Name of the Enemy GameObject (default 'EnemyAI')"),
    }, async (args) => {
        const res = await client.execute({
            action: "scaffold_enemy",
            name: args.name || "EnemyAI",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Enemy AI scaffolded successfully." }] };
    });
    // Scaffold Follow Camera
    server.tool("unity_scaffold_camera", "Configures a smooth third-person follow & orbit camera with auto-generated SmoothFollowCamera script.", {
        target: z.string().optional().describe("Target GameObject to follow (default 'PlayerCharacter')"),
    }, async (args) => {
        const res = await client.execute({
            action: "scaffold_camera",
            target: args.target || "PlayerCharacter",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Camera scaffolded successfully." }] };
    });
}
