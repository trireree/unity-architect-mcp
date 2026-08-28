import { z } from "zod";
export function registerAssetTools(server, client) {
    // Create Prefab
    server.tool("unity_asset_create_prefab", "Saves an existing GameObject in the scene as a reusable Prefab asset.", {
        target: z.string().describe("Name or Instance ID of the GameObject to convert into prefab"),
        path: z.string().optional().describe("Asset path (e.g. 'Assets/Prefabs/Player.prefab'). Defaults to Assets/{ObjectName}.prefab"),
    }, async (args) => {
        const res = await client.execute({
            action: "asset_create_prefab",
            target: args.target,
            path: args.path,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (Path: ${res.data})` }] };
    });
    // Instantiate Prefab
    server.tool("unity_asset_instantiate_prefab", "Instantiates a Prefab asset into the current scene with position, rotation, and parent.", {
        path: z.string().describe("Asset path or name of the Prefab (e.g. 'Assets/Prefabs/Tree.prefab' or 'Tree')"),
        position: z.array(z.number()).length(3).optional().describe("[x, y, z] world position"),
        rotation: z.array(z.number()).length(3).optional().describe("[x, y, z] Euler angles"),
        parent: z.string().optional().describe("Parent GameObject name or Instance ID"),
    }, async (args) => {
        const res = await client.execute({
            action: "asset_instantiate_prefab",
            path: args.path,
            position: args.position,
            rotation: args.rotation,
            parent: args.parent,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (ID: ${res.data})` }] };
    });
    // Create Material
    server.tool("unity_asset_create_material", "Creates a new Material asset with custom shader and hex color.", {
        name: z.string().describe("Name of the Material (e.g. 'GrassMaterial')"),
        shaderName: z.string().optional().describe("Shader name (e.g. 'Universal Render Pipeline/Lit', 'Standard', 'Unlit/Color')"),
        colorHex: z.string().optional().describe("Hex color string (e.g. '#FF0000', '#00FF0088')"),
        path: z.string().optional().describe("Directory to save material in (defaults to 'Assets/Materials')"),
    }, async (args) => {
        const res = await client.execute({
            action: "asset_create_material",
            name: args.name,
            shaderName: args.shaderName,
            colorHex: args.colorHex,
            path: args.path,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (Created: ${res.data})` }] };
    });
    // Find Assets
    server.tool("unity_asset_find", "Searches the Unity AssetDatabase for assets matching a filter or type (e.g. 't:Prefab', 't:Material', 't:AudioClip', 't:Scene', 'Player').", {
        filter: z.string().describe("Search filter (e.g. 't:Prefab', 't:Texture', 'Car')"),
        path: z.string().optional().describe("Folder to search in (e.g. 'Assets/Models')"),
    }, async (args) => {
        const res = await client.execute({
            action: "asset_find",
            filter: args.filter,
            path: args.path,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message}\n${res.data || ""}` }] };
    });
}
