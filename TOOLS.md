# 🛠️ Unity Architect MCP — Complete Tools Reference

Below is the complete reference of tools available in **Unity Architect MCP v2.5.0**.

---

## 1. 🚀 Autonomous & High-Level Architect Tools

| Tool Name | Parameters | Description |
| :--- | :--- | :--- |
| `unity_autonomous_build` | `intent` (string) | Executes full autonomous pipeline: Intent → State Inspection → Plan → Snapshot → Batch Execution → Validation → Auto Self-Healing → Commit/Rollback. |
| `unity_inspect_project` | none | Returns a compact (~100 tokens), progressive-disclosure summary of active scenes, object counts, scripts, and key scene anchors. |
| `unity_inspect_scene` | none | Returns active scene hierarchy overview, scene hash, and root objects. |
| `unity_inspect_object` | `target` (string) | Returns detailed graph subtree, components, and dependencies for a specific GameObject or Stable ID. |
| `unity_state_diff` | none | Returns incremental diff (`ADDED`, `REMOVED`, `MODIFIED`, `UNCHANGED`) since last baseline. |
| `unity_snapshot` | `name` (optional) | Creates an atomic scene and file checkpoint snapshot. |
| `unity_rollback` | `transactionId` (optional) | Atomically reverts to the specified or most recent checkpoint snapshot. |
| `unity_validate` | none | Runs deep integrity scan for missing scripts, broken shaders/materials, missing cameras, and compile errors. |
| `unity_execute_batch` | `actions` (array), `autoRollbackOnError` (bool) | Runs multiple Unity operations in a single atomic transaction with auto-validation. |

---

## 2. 🩺 Self-Healing & Diagnostics Tools

| Tool Name | Parameters | Description |
| :--- | :--- | :--- |
| `unity_self_heal` | `transactionId` (optional) | Runs automated 3-attempt repair loop on active errors (injects missing namespaces, clears missing script components). |
| `unity_diagnose_errors` | none | Classifies active C# compiler and runtime exceptions with suggested root-cause fixes. |

---

## 3. 🧠 Knowledge & Context Intelligence Tools

| Tool Name | Parameters | Description |
| :--- | :--- | :--- |
| `unity_search_knowledge` | `query` (string), `category` (optional) | Queries local-first offline Unity knowledge base (WheelCollider physics, URP, CharacterController, NavMesh, Object Pooling). |
| `unity_query_context` | `query` (string) | Answers natural queries about the project graph (e.g. "Where is Player?", "What objects use PlayerController?"). |
| `unity_asset_dependencies` | `path` (string) | Returns deep dependency hierarchy for any asset (Prefab → Mesh → Material → Texture). |
| `unity_find_duplicates` | `folder` (optional) | Finds duplicate textures, models, and audio files by file size and content hash. |

---

## 4. 🎮 Planning & Game System Scaffolding Tools

| Tool Name | Parameters | Description |
| :--- | :--- | :--- |
| `unity_plan_system` | `intent` (string) | Generates dependency-ordered step-by-step implementation blueprint. |
| `unity_execute_plan` | `intent` (string) | Generates and immediately executes a system plan in a single transaction. |
| `unity_scaffold_system` | `systemName` (enum) | Generates production C# patterns: `player`, `vehicle`, `weapon`, `health`, `inventory`, `enemy`, `interaction`, `saveload`, `objectpool`, `daynight`, `police`, `traffic`. |
| `unity_architect_game` | `genre` (optional) | Builds complete multi-system 3D open-world prototype (City grid, Buildings, Player, Police Car, HUD, Lighting). |

---

## 5. 📜 Memory & Journaling Tools

| Tool Name | Parameters | Description |
| :--- | :--- | :--- |
| `unity_memory_history` | `count` (number), `query` (optional) | Queries persistent development journal of recent AI actions, modified objects, and transaction IDs. |

---

## 6. ⚙️ Granular Low-Level Engine Tools (Backward-Compatible)

- **Scene**: `unity_scene_get_hierarchy`, `unity_gameobject_create`, `unity_gameobject_modify`, `unity_gameobject_delete`, `unity_gameobject_duplicate`
- **Components**: `unity_component_add`, `unity_component_remove`, `unity_component_get_properties`, `unity_component_set_property`
- **Assets**: `unity_asset_create_prefab`, `unity_asset_instantiate_prefab`, `unity_asset_create_material`, `unity_asset_find`
- **Scripting & REPL**: `unity_script_create`, `unity_script_get_compilation_status`, `unity_csharp_eval`
- **Vision**: `unity_vision_capture_scene`, `unity_vision_capture_game`, `unity_vision_inspect_object`
- **Physics**: `unity_physics_setup_rigidbody`, `unity_physics_setup_collider`, `unity_physics_bake_navmesh`
- **Animation & UI**: `unity_animator_create`, `unity_animator_add_state`, `unity_ui_create_canvas`, `unity_ui_create_element`
- **PlayMode**: `unity_playmode_start`, `unity_playmode_stop`, `unity_playmode_pause`, `unity_console_get_logs`, `unity_console_clear`
