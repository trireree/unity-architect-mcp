import { z } from "zod";
export function registerMemoryTools(server, client) {
    // Memory History
    server.tool("unity_memory_history", "Queries the structured persistent development journal (recent AI actions, modified objects, transaction IDs) to understand project modification history without full context bloat.", {
        count: z.number().optional().describe("Number of recent entries to fetch (defaults to 20)"),
        query: z.string().optional().describe("Optional search filter (e.g. 'car', 'player', 'script_create')"),
    }, async (args) => {
        const res = await client.execute({
            action: "memory_history",
            count: args.count,
            query: args.query,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.data || "{}" }] };
    });
}
