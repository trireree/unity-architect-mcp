import { z } from "zod";
import { UnityClient } from "../unity-client.js";

export function registerComponentTools(server: any, client: UnityClient) {
  // Add Component
  server.tool(
    "unity_component_add",
    "Attaches a component (e.g. Rigidbody, BoxCollider, AudioSource, Light, Camera, or custom MonoBehaviour script) to a GameObject.",
    {
      target: z.string().describe("Name or Instance ID of the target GameObject"),
      componentType: z.string().describe("Name of the Component class (e.g. 'Rigidbody', 'AudioSource', 'PlayerMovement')"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "component_add",
        target: args.target,
        componentType: args.componentType,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.message || "Component added." }] };
    }
  );

  // Remove Component
  server.tool(
    "unity_component_remove",
    "Removes a component from a GameObject.",
    {
      target: z.string().describe("Name or Instance ID of the target GameObject"),
      componentType: z.string().describe("Name of the Component class to remove"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "component_remove",
        target: args.target,
        componentType: args.componentType,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.message || "Component removed." }] };
    }
  );

  // Get Component Properties
  server.tool(
    "unity_component_get_properties",
    "Inspects and returns all serialized properties and fields of a component on a GameObject.",
    {
      target: z.string().describe("Name or Instance ID of the target GameObject"),
      componentType: z.string().describe("Name of the Component class to inspect"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "component_get_properties",
        target: args.target,
        componentType: args.componentType,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.data || "{}" }] };
    }
  );

  // Set Component Property
  server.tool(
    "unity_component_set_property",
    "Sets the value of a specific field or SerializedProperty on a component (supports int, float, bool, string, enum).",
    {
      target: z.string().describe("Name or Instance ID of the target GameObject"),
      componentType: z.string().describe("Name of the Component class"),
      propertyName: z.string().describe("Name of the property or public/private field"),
      propertyValue: z.string().describe("New value formatted as string (e.g. '5.5', 'true', 'MyText')"),
    },
    async (args: any) => {
      const res = await client.execute({
        action: "component_set_property",
        target: args.target,
        componentType: args.componentType,
        propertyName: args.propertyName,
        propertyValue: args.propertyValue,
      });
      if (!res.success) return { content: [{ type: "text", text: `Error: ${res.error}` }] };
      return { content: [{ type: "text", text: res.message || "Property updated." }] };
    }
  );
}
