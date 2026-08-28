import { z } from "zod";
export function registerKnowledgeTools(server, client) {
    // Search Knowledge
    server.tool("unity_search_knowledge", "Searches the offline local-first Unity Knowledge Base (WheelCollider physics, URP shaders, modern API standards, CharacterController, NavMesh, Object Pooling) for accurate implementation guidance.", {
        query: z.string().describe("Topic or question (e.g. 'wheelcollider', 'urp shader color', 'charactercontroller jump', 'object pool')"),
        category: z.string().optional().describe("Optional category: 'Physics', 'Scripting', 'Rendering', 'Gameplay', 'AI', 'Optimization'"),
    }, async (args) => {
        const res = await client.execute({
            action: "search_knowledge",
            query: args.query,
            filter: args.category,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
}
