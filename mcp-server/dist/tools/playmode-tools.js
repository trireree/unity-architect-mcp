import { z } from "zod";
export function registerPlayModeTools(server, client) {
    // Start Play Mode
    server.tool("unity_playmode_start", "Enters Play Mode in the Unity Editor to run the game simulation.", {}, async () => {
        const res = await client.execute({ action: "playmode_start" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Play Mode started." }] };
    });
    // Stop Play Mode
    server.tool("unity_playmode_stop", "Exits Play Mode and returns to Edit Mode.", {}, async () => {
        const res = await client.execute({ action: "playmode_stop" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Play Mode stopped." }] };
    });
    // Pause Play Mode
    server.tool("unity_playmode_pause", "Pauses or unpauses Play Mode simulation.", {
        pause: z.boolean().describe("True to pause, false to unpause"),
    }, async (args) => {
        const res = await client.execute({
            action: "playmode_pause",
            pause: args.pause,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Play mode pause state toggled." }] };
    });
    // Get Console Logs
    server.tool("unity_console_get_logs", "Retrieves recent Unity Console messages, warnings, errors, and exception stack traces.", {
        count: z.number().optional().describe("Number of recent logs to fetch (default 50)"),
        filterType: z.enum(["Log", "Warning", "Error", "Exception"]).optional().describe("Optional filter by log severity"),
    }, async (args) => {
        const res = await client.execute({
            action: "console_get_logs",
            count: args.count || 50,
            filterType: args.filterType,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
    // Clear Console Logs
    server.tool("unity_console_clear", "Clears the Unity Editor Console logs.", {}, async () => {
        const res = await client.execute({ action: "console_clear" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Console logs cleared." }] };
    });
}
