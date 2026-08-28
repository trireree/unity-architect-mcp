# 🧠 Unity Knowledge Engine & Ingestion Pipeline

**Unity Architect MCP** includes a version-aware, local-first offline Unity Knowledge Engine designed to prevent AI hallucinations, deprecated API usage, and incorrect shader properties.

---

## 📚 Indexed Domains & Version Awareness

1. **CORE & SCRIPTING**:
   - Modern API standards (Unity 6 / 2023+): Uses `Object.FindFirstObjectByType<T>()` and `Object.FindAnyObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`.
   - Allocation-free lookups: `TryGetComponent<T>(out var comp)`.

2. **INPUT SYSTEM**:
   - Modern `UnityEngine.InputSystem` actions and `PlayerInput` integration versus legacy `Input.GetAxis`.

3. **PHYSICS & MOTOR CONTROL**:
   - `CharacterController` velocity integration (`isGrounded` contact offset, slope sliding).
   - `WheelCollider` suspension springs, friction curves, motorTorque, and visual pose synchronization via `GetWorldPose`.

4. **RENDERING & URP LIT SHADERS**:
   - Universal Render Pipeline (URP) property mapping: `_BaseColor`, `_BaseMap`, `_Smoothness` (SRP Batcher compatible).

5. **AI & NAVIGATION**:
   - `NavMeshAgent` destination management, dynamic `NavMeshObstacle` carving.

6. **UI & UI TOOLKIT**:
   - Runtime UI Document (`UIDocument` / UXML / USS) versus Canvas TextMeshProUGUI.

7. **WORLD STREAMING & ASSETS**:
   - Additive scene loading, LOD groups, Occlusion Culling, and Addressables.

8. **PERFORMANCE & GC OPTIMIZATION**:
   - `UnityEngine.Pool.ObjectPool<GameObject>` and `Physics.RaycastNonAlloc`.
