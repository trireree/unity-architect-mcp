# 🛡️ Security & Safety Policies — Unity Architect MCP v4.0.0

---

## 🔒 Destructive Safeguards

1. **Mandatory Checkpoint Snapshots**: Every delete or mass modification automatically triggers `TransactionManager.BeginTransaction()` before execution.
2. **Impact Analysis Required**: Destructive operations evaluate `ImpactAnalysisEngine.AnalyzeImpact()` (blast radius & risk levels).
3. **Hard Limits**:
   - `MAX_REPAIR_ATTEMPTS = 3`: Prevents infinite self-healing loops.
   - Atomic rollback on failed batch operations.
4. **Path Traversal Protection**: File operations are constrained within project `Assets/` and `Packages/` directories.
