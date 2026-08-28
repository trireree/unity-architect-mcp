import { z } from "zod";
export function registerUITools(server, client) {
    // Create Canvas
    server.tool("unity_ui_create_canvas", "Creates a UI Canvas with CanvasScaler, GraphicRaycaster, and EventSystem.", {
        renderMode: z.enum(["ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"]).optional().describe("Render mode (default ScreenSpaceOverlay)"),
    }, async (args) => {
        const res = await client.execute({
            action: "ui_create_canvas",
            renderMode: args.renderMode || "ScreenSpaceOverlay",
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (ID: ${res.data})` }] };
    });
    // Create UI Element
    server.tool("unity_ui_create_element", "Creates a UI Element (Panel, Button, Text, Image) under a Canvas with RectTransform layout.", {
        elementType: z.enum(["panel", "button", "text", "image"]).describe("Type of UI element"),
        parent: z.string().optional().describe("Parent GameObject name or Canvas"),
        name: z.string().optional().describe("Name of the UI GameObject"),
        text: z.string().optional().describe("Text content for Button or Text elements"),
        posX: z.number().optional().describe("Anchored X position"),
        posY: z.number().optional().describe("Anchored Y position"),
        width: z.number().optional().describe("Width in pixels"),
        height: z.number().optional().describe("Height in pixels"),
    }, async (args) => {
        const res = await client.execute({
            action: "ui_create_element",
            elementType: args.elementType,
            parent: args.parent,
            name: args.name,
            text: args.text,
            posX: args.posX || 0,
            posY: args.posY || 0,
            width: args.width || 160,
            height: args.height || 30,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (ID: ${res.data})` }] };
    });
}
