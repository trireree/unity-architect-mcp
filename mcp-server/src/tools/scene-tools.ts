import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerSceneTools(server: any, client: UnityClient) {
  // Get Scene Hierarchy
  server.tool(
    "unity_scene_get_hierarchy",
    "Returns the complete scene hierarchy tree with GameObject IDs, tags, layers, active states, transforms, and attached components.",
    {},
    async () => {
      const res = await client.execute({ action: "scene_get_hierarchy" });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || res.message || "{}" }] };
    }
  );

  // Create GameObject
  server.tool(
    "unity_gameobject_create",
    "Creates a new GameObject in the active Unity scene (Empty or Primitive: Cube, Sphere, Capsule, Cylinder, Plane, Quad) with optional parent, position, rotation, and scale.",
    {
      name: z.string().optional().describe("Name of the GameObject"),
      primitiveType: z.enum(["Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad"]).optional().describe("Primitive type if not empty"),
      position: z.array(z.number()).length(3).optional().describe("[x, y, z] local position"),
      rotation: z.array(z.number()).length(3).optional().describe("[x, y, z] local Euler angles"),
      scale: z.array(z.number()).length(3).optional().describe("[x, y, z] local scale"),
      parent: z.string().optional().describe("Name or Instance ID of parent GameObject"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "gameobject_create",
        name: args.name,
        primitiveType: args.primitiveType,
        position: args.position,
        rotation: args.rotation,
        scale: args.scale,
        parent: args.parent,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: `${res.message} (ID: ${res.data})` }] };
    }
  );

  // Modify GameObject
  server.tool(
    "unity_gameobject_modify",
    "Modifies an existing GameObject's transform, name, tag, layer, or active state.",
    {
      target: z.string().describe("Name or Instance ID of the target GameObject"),
      name: z.string().optional().describe("New name for the GameObject"),
      position: z.array(z.number()).length(3).optional().describe("[x, y, z] local position"),
      rotation: z.array(z.number()).length(3).optional().describe("[x, y, z] local Euler angles"),
      scale: z.array(z.number()).length(3).optional().describe("[x, y, z] local scale"),
      tag: z.string().optional().describe("Tag name"),
      layer: z.string().optional().describe("Layer name"),
      active: z.boolean().optional().describe("Active state"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "gameobject_modify",
        target: args.target,
        name: args.name,
        position: args.position,
        rotation: args.rotation,
        scale: args.scale,
        tag: args.tag,
        layer: args.layer,
        active: args.active,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.message || "GameObject modified successfully." }] };
    }
  );

  // Delete GameObject
  server.tool(
    "unity_gameobject_delete",
    "Deletes a GameObject from the scene.",
    {
      target: z.string().describe("Name or Instance ID of the GameObject to delete"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "gameobject_delete",
        target: args.target,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.message || "GameObject deleted." }] };
    }
  );

  // Duplicate GameObject
  server.tool(
    "unity_gameobject_duplicate",
    "Duplicates a GameObject in the scene.",
    {
      target: z.string().describe("Name or Instance ID of the GameObject to duplicate"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "gameobject_duplicate",
        target: args.target,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: `${res.message} (ID: ${res.data})` }] };
    }
  );
}
