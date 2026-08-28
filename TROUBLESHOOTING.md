# 🔧 Troubleshooting Guide — Unity Architect MCP

### 1. "Cannot connect to Unity Bridge at http://127.0.0.1:8080"
- **Cause**: Unity Editor is not open, or the Unity package is not loaded.
- **Solution**: Open your Unity project, make sure `com.antigravity.unitymcp` is in Package Manager, and check that `Unity Architect MCP` window shows `● ONLINE`.

### 2. Port Conflict (Port 8080 already in use)
- **Cause**: Another service is using port 8080.
- **Solution**: Open `Window → Antigravity → Unity Architect MCP` in Unity, change the port to `8085` (or another free port), and update `"UNITY_BRIDGE_URL": "http://127.0.0.1:8085"` in your AI client MCP configuration.

### 3. Unity Freezes or Blocks Execution
- **Cause**: Unity has opened a modal dialog or is compiling scripts.
- **Solution**: Close any open popup dialogs in Unity. Once compilation finishes, execution will automatically resume.

### 4. Self-Healing Triggered a Rollback
- **Cause**: An operation resulted in C# compile errors that could not be resolved within `MAX_REPAIR_ATTEMPTS = 3`.
- **Solution**: Check the console error output using `unity_diagnose_errors` or inspect the generated script.
