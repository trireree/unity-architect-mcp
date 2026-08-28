import { z } from "zod";
export function registerPhysicsTools(server, client) {
    // Setup Rigidbody
    server.tool("unity_physics_setup_rigidbody", "Configures a Rigidbody physics component on a GameObject.", {
        target: z.string().describe("Target GameObject name or Instance ID"),
        mass: z.number().optional().describe("Mass in kg (default 1.0)"),
        drag: z.number().optional().describe("Linear drag (default 0.0)"),
        angularDrag: z.number().optional().describe("Angular drag (default 0.05)"),
        useGravity: z.boolean().optional().describe("Whether gravity is enabled (default true)"),
        isKinematic: z.boolean().optional().describe("Whether the body is kinematic (default false)"),
    }, async (args) => {
        const res = await client.execute({
            action: "physics_setup_rigidbody",
            target: args.target,
            mass: args.mass,
            drag: args.drag,
            angularDrag: args.angularDrag,
            useGravity: args.useGravity ?? true,
            isKinematic: args.isKinematic ?? false,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Rigidbody configured." }] };
    });
    // Setup Collider
    server.tool("unity_physics_setup_collider", "Configures Box, Sphere, Capsule, or Mesh Collider on a GameObject.", {
        target: z.string().describe("Target GameObject name or Instance ID"),
        colliderType: z.enum(["box", "sphere", "capsule", "mesh"]).describe("Type of collider"),
        isTrigger: z.boolean().optional().describe("Whether collider acts as a trigger"),
        center: z.array(z.number()).length(3).optional().describe("[x, y, z] center offset"),
        size: z.array(z.number()).optional().describe("Dimensions (Box: [x,y,z], Sphere: [radius], Capsule: [radius, height])"),
    }, async (args) => {
        const res = await client.execute({
            action: "physics_setup_collider",
            target: args.target,
            colliderType: args.colliderType,
            isTrigger: args.isTrigger ?? false,
            center: args.center,
            size: args.size,
        });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "Collider configured." }] };
    });
    // Bake NavMesh
    server.tool("unity_physics_bake_navmesh", "Bakes the NavMesh navigation data for the active scene.", {}, async () => {
        const res = await client.execute({ action: "physics_bake_navmesh" });
        if (!res.success)
            return { content: [{ type: "text", text: `Error: ${res.error}` }] };
        return { content: [{ type: "text", text: res.message || "NavMesh baked successfully." }] };
    });
}
