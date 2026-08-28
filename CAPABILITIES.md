# 🧭 Engine Capabilities & Self-Awareness — Unity Architect MCP v4.0.0

**Unity Architect MCP** provides complete transparency into its capabilities, verified pipelines, and operational limits.

---

## 🟢 Supported Systems & Pipelines

1. **State Graph & Incremental Diff (SHA256)**: Real-time scene indexing (120+ nodes under 30ms).
2. **Atomic Transactions & Scene/Asset Rollback**: Reverts all changes on compilation or runtime errors.
3. **LLM-Assisted Minimal Repair Context Generator**: Generates concise, high-signal (<250 token) repair payloads.
4. **Seed-based Procedural World & Road Grid Generator**: Reproducible world layouts (`seed = 48392`).
5. **Third-Person Player Controller & Follow Camera**: CharacterController physics & mouse orbit.
6. **Physics Drivable Vehicles**: 4 WheelColliders with motorTorque, steering, and center of mass.
7. **5-Star Police Pursuit & Wanted System**: Threat state transitions and police pursuit triggers.
8. **NavMesh AI Simulation & Pedestrian Logic**: Autonomous destination patrolling.
9. **Object Pooled Traffic Spawner**: Scalable road traffic spawning without garbage collection spikes.
10. **Composite Quality Gate (0-100 scoring)**: Objective evaluation across compilation, integrity, and performance.

---

## ⚠️ Known Boundaries & Realistic Limitations

- **Asset Artistry**: MCP is a development intelligence operating system, not a generative 3D mesh modeler. High-fidelity AAA visuals require commercial 3D assets/FBX.
- **Headless / Batchmode Execution**: In automated headless batchmode, visual inspection requires client-side vision LLM evaluation.
- **Algorithmic Complexity**: Architectural C# refactorings require LLM reasoning via `unity_repair_context`.
