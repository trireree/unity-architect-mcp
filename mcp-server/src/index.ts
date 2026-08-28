#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { UnityClient } from "./unity-client.js";

// Phase 60+ Autonomous AAA Game Development Tools
import { registerAutonomousGameTools } from "./tools/autonomous-game-tools.js";

// Phase 21 - 44 Evolution Toolsets
import { registerEvolutionTools } from "./tools/evolution-tools.js";

// Phase 1 - 20 Toolsets
import { registerArchitectTools } from "./tools/architect-tools.js";
import { registerHealingTools } from "./tools/healing-tools.js";
import { registerKnowledgeTools } from "./tools/knowledge-tools.js";
import { registerIntelligenceTools } from "./tools/intelligence-tools.js";
import { registerPlanningTools } from "./tools/planning-tools.js";
import { registerGameArchitectTools } from "./tools/game-architect-tools.js";
import { registerMemoryTools } from "./tools/memory-tools.js";
import { registerAutonomousLoopTools } from "./tools/autonomous-loop-tools.js";

// Backward-compatible granular toolsets
import { registerSceneTools } from "./tools/scene-tools.js";
import { registerComponentTools } from "./tools/component-tools.js";
import { registerAssetTools } from "./tools/asset-tools.js";
import { registerScriptTools } from "./tools/script-tools.js";
import { registerVisionTools } from "./tools/vision-tools.js";
import { registerPhysicsTools } from "./tools/physics-tools.js";
import { registerAnimationTools } from "./tools/animation-tools.js";
import { registerUITools } from "./tools/ui-tools.js";
import { registerPlayModeTools } from "./tools/playmode-tools.js";
import { registerScaffoldingTools } from "./tools/scaffolding-tools.js";

async function main() {
  const unityClient = new UnityClient();

  const server = new McpServer({
    name: "unity-architect-mcp",
    version: "4.0.0",
  });

  // Register Autonomous & Architect Intelligence Layers
  registerAutonomousGameTools(server, unityClient);
  registerEvolutionTools(server, unityClient);
  registerAutonomousLoopTools(server, unityClient);
  registerArchitectTools(server, unityClient);
  registerHealingTools(server, unityClient);
  registerKnowledgeTools(server, unityClient);
  registerIntelligenceTools(server, unityClient);
  registerPlanningTools(server, unityClient);
  registerGameArchitectTools(server, unityClient);
  registerMemoryTools(server, unityClient);

  // Register Backward-Compatible Granular Engine Tools
  registerSceneTools(server, unityClient);
  registerComponentTools(server, unityClient);
  registerAssetTools(server, unityClient);
  registerScriptTools(server, unityClient);
  registerVisionTools(server, unityClient);
  registerPhysicsTools(server, unityClient);
  registerAnimationTools(server, unityClient);
  registerUITools(server, unityClient);
  registerPlayModeTools(server, unityClient);
  registerScaffoldingTools(server, unityClient);

  const transport = new StdioServerTransport();
  await server.connect(transport);

  console.error("[Unity Architect MCP v4.0.0] Production AI Game Development OS ready.");
}

main().catch((err) => {
  console.error("[Unity Architect MCP] Fatal error:", err);
  process.exit(1);
});
