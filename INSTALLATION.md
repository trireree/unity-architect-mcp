# 📦 Installation & Setup Guide — Unity Architect MCP

Follow these steps to set up **Unity Architect MCP** in your local Unity project and connect it to your preferred MCP client (Antigravity, Claude Desktop, Cursor, Windsurf, VS Code).

---

## Step 1: Add Unity Package to Your Project

1. Open your Unity project (Unity 2021.3 LTS, 2022.3 LTS, 2023, or Unity 6).
2. Open **Window → Package Manager**.
3. Click the `+` icon in the top left and choose **"Add package from disk..."**.
4. Select `unity-package/com.antigravity.unitymcp/package.json` in this repository.
5. The bridge server will automatically start on `http://127.0.0.1:8080/`.
6. Verify by opening **Window → Antigravity → Unity Architect MCP** in Unity.

---

## Step 2: Build the MCP Server

Ensure Node.js (v18+) is installed:

```bash
cd mcp-server
npm install
npm run build
```

---

## Step 3: Configure Your AI Client

### For Antigravity / Claude Desktop / Cursor / Windsurf

Add the following to your MCP client configuration file (e.g. `claude_desktop_config.json` or Antigravity MCP settings):

```json
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": [
        "c:/Users/fbara/Documents/antigravity/valiant-tesla/mcp-server/dist/index.js"
      ],
      "env": {
        "UNITY_BRIDGE_URL": "http://127.0.0.1:8080"
      }
    }
  }
}
```

---

## Step 4: Verification

Test your setup by asking your AI agent:
> *"Inspect the Unity project state and summarize what scenes and GameObjects exist."*

The AI will call `unity_inspect_project` and report your project state instantly!
