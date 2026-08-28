import { z } from "zod";
export function registerAnimationTools(server, client) {
    // Create Animator Controller
    server.tool("unity_animator_create", "Creates a new AnimatorController asset.", {
        name: z.string().describe("Name of the controller (e.g. 'PlayerAnimator')"),
        path: z.string().optional().describe("Directory to save (default 'Assets/Animations')"),
    }, async (args) => {
        const res = await client.execute({
            action: "animator_create",
            name: args.name,
            path: args.path,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: `${res.message} (Path: ${res.data})` }] };
    });
    // Add State
    server.tool("unity_animator_add_state", "Adds a state (e.g. 'Idle', 'Run', 'Attack') to an AnimatorController with optional Motion clip.", {
        path: z.string().describe("Path to AnimatorController asset"),
        name: z.string().describe("State name"),
        motionPath: z.string().optional().describe("Path to AnimationClip asset"),
    }, async (args) => {
        const res = await client.execute({
            action: "animator_add_state",
            path: args.path,
            name: args.name,
            motionPath: args.motionPath,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "State added." }] };
    });
    // Add Parameter
    server.tool("unity_animator_add_param", "Adds a parameter (Float, Int, Bool, Trigger) to an AnimatorController.", {
        path: z.string().describe("Path to AnimatorController asset"),
        name: z.string().describe("Parameter name (e.g. 'Speed', 'IsGrounded', 'Jump')"),
        paramType: z.enum(["float", "int", "bool", "trigger"]).describe("Parameter type"),
    }, async (args) => {
        const res = await client.execute({
            action: "animator_add_param",
            path: args.path,
            name: args.name,
            paramType: args.paramType,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Parameter added." }] };
    });
}
