# 🪐 Unity Architect MCP — Ultra-Advanced AI Bridge for Unity 6 & 2021+

**Unity Architect MCP** (Model Context Protocol) is an enterprise-grade AI bridge that enables autonomous AI coding assistants (Google Antigravity, Claude, Cursor, Windsurf, ChatGPT) to directly orchestrate, inspect, generate, and self-heal Unity projects in real-time.

---

## 🚀 Key Features

* **⚡ C# Live Dynamic REPL (`csharp_eval`):** Execute C# code directly in the Editor context with zero domain reload lag.
* **👁️ Vision & Viewport Feedback (`get_viewport_screenshot`):** Capture Scene and Game view frames for AI visual grounding.
* **🏗️ Autonomous World & Asset Scaffolding:** Generate procedural road networks, city districts, vehicle physics, and pedestrian AI.
* **🛡️ Self-Healing & Quality Gate:** Automatic compilation repair, scene integrity audits, missing reference fixes, and 0-error enforcement.
* **🔄 Transactional Snapshots & Rollback:** Safe batch execution with undo/redo and atomic scene states.
* **🚗 Open World & Gameplay Systems:** Pre-architected FPS controllers, 3D spatial vehicle audio, and autonomous traffic graphs.

---

## 📦 How to Install in Any Unity Project (Package Manager)

You can install this package into any Unity project (`2021.3`, `2022.3`, `Unity 6 / 6000+`) using any of the following 4 methods:

### Method 1: Install via Git URL (Recommended)
1. Open your Unity project.
2. Go to **`Window` > `Package Manager`**.
3. Click the **`+`** icon in the top-left corner.
4. Select **`Add package from git URL...`**.
5. Enter your repository URL (or subpath):
   ```
   https://github.com/<your-username>/<your-repo>.git?path=unity-package/com.antigravity.unitymcp
   ```
6. Click **`Add`**. Unity will download and import the package automatically.

---

### Method 2: Install from Local Disk
1. Open your Unity project.
2. Go to **`Window` > `Package Manager`**.
3. Click the **`+`** icon and select **`Add package from disk...`**.
4. Browse to the `com.antigravity.unitymcp` folder and select `package.json`.
5. Unity will immediately link the package into your project.

---

### Method 3: Install via `Packages/manifest.json`
Open your project's `Packages/manifest.json` file and add the package to the `dependencies` object:

```json
{
  "dependencies": {
    "com.antigravity.unitymcp": "https://github.com/<your-username>/<your-repo>.git?path=unity-package/com.antigravity.unitymcp",
    "com.unity.modules.core": "1.0.0"
  }
}
```

---

### Method 4: Install from Tarball (`.tgz`)
1. In Package Manager, click **`+`** > **`Add package from tarball...`**.
2. Select `com.antigravity.unitymcp-1.0.0.tgz`.

---

## 🕹️ Quick Start Guide

1. After importing the package, open the MCP Panel in Unity:
   * Go to **`Window` > `Antigravity` > `Unity Architect MCP`**.
2. The server will automatically start on **`http://127.0.0.1:8080/`**.
3. The status indicator in the editor window will show **`● ONLINE`**.
4. Your AI agent (Antigravity / Claude) is now connected and ready to build games!

---

## 📡 MCP Tools & REST Endpoints

| Endpoint / Action | Description |
| :--- | :--- |
| `csharp_eval` | Dynamic C# expression & script evaluation in Editor context. |
| `execute_batch` | Atomic transactional execution with snapshot/rollback. |
| `quality_gate` | 4-tier audit (Compile, Scene, Gameplay, Performance Score). |
| `get_hierarchy` | Full GameObject hierarchy with component reflection. |
| `capture_screenshot` | Viewport & Game view visual feedback for AI vision. |
| `generate_world` | Autonomous asset-first procedural world builder. |

---

## 📄 License
MIT License. Created by the Antigravity Team.
