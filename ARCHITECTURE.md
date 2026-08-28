# 🏛️ Unity Architect MCP — System Architecture

**Unity Architect MCP v2.5.0** is an enterprise-grade execution, reasoning-context, and self-healing engine bridging any Model Context Protocol (MCP) AI client (Antigravity, Claude Desktop, Cursor, Windsurf, VS Code) with the Unity Editor.

```mermaid
graph TD
    LLM[Any Modern LLM: Claude / GPT / Gemini / DeepSeek]
    -->|MCP Protocol over STDIO| ArchitectMCP[UNITY ARCHITECT MCP SERVER]
    
    subgraph Architect MCP Intelligence Layer (Node/TS)
        ArchitectMCP --> AutonomousLoop[Autonomous Development Loop Engine]
        ArchitectMCP --> IntentPlanner[Intent & Planning Engine]
        ArchitectMCP --> KnowledgeEngine[Local Offline Unity Knowledge Index]
        ArchitectMCP --> ContextOptimizer[Incremental State & Diff Optimizer]
        ArchitectMCP --> SelfHealingAgent[Self-Healing & Auto-Patch Loop]
        ArchitectMCP --> TransactionCoordinator[Atomic Transaction & Rollback Coordinator]
        ArchitectMCP --> MemoryJournal[Structured Development Memory Store]
    end

    ArchitectMCP <-->|Fast JSON-RPC Bridge (127.0.0.1:8080)| UnityArchitectBridge[UNITY ARCHITECT BRIDGE (C# Editor)]

    subgraph Unity Engine Core Systems
        UnityArchitectBridge --> MainDispatcher[Thread-Safe MainThreadDispatcher]
        UnityArchitectBridge --> StateGraph[Project State Graph & Stable ID Indexer]
        UnityArchitectBridge --> Hasher[Incremental SHA256 State Hasher]
        UnityArchitectBridge --> SnapshotManager[Hybrid Scene & File Snapshot Engine]
        UnityArchitectBridge --> Validator[Scene Integrity & Missing Script Detector]
        UnityArchitectBridge --> BatchExecutor[Atomic Batch Transaction Executor]
        UnityArchitectBridge --> ErrorClassifier[C# Compiler & Runtime Error Classifier]
        UnityArchitectBridge --> GameArchitect[Open-World & Multi-System Generator]
        UnityArchitectBridge --> SystemTemplates[12+ Production C# Pattern Blueprints]
        UnityArchitectBridge --> ProfilerMetrics[UnityStats & Profiler Harvester]
        UnityArchitectBridge --> VisionCapture[SceneView / GameView Vision Viewports]
        UnityArchitectBridge --> SafetyPolicy[Destructive Operation Safety Guard]
    end
```

---

## 🔑 Key Architectural Principles

1. **Model Agnostic & Local-First**: Zero external database or cloud AI inference dependencies. All project graphs, snapshots, and indexers run locally in Unity and Node.js.
2. **Progressive Disclosure Context**: Prevents context window explosion. Summaries (~100 tokens) are provided first, allowing the LLM to drill down into subgraphs only when needed.
3. **Atomic Transactions & Checkpoints**: Every complex AI operation is wrapped in a `tx_` transaction. In case of unresolvable errors, a complete rollback restores the scene and files to their exact pre-operation state.
4. **Self-Healing Loop**: C# compiler errors (CS0246, CS0103, CS1002) and scene anomalies are automatically classified and patched with a hard limit of `MAX_REPAIR_ATTEMPTS = 3` to prevent infinite loops.
