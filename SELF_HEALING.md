# 🩺 LLM-Assisted Self-Healing & Diagnostics

**Unity Architect MCP** bridges raw Unity C# compiler errors and runtime exceptions directly to the host LLM through a minimal-token, high-signal repair context protocol.

---

## 🔄 Self-Healing Pipeline

```text
UNITY COMPILE / RUNTIME ERROR
↓
ERROR CLASSIFICATION (CS0246, CS0103, CS1002, NullReferenceException, MissingScript)
↓
SOURCE LINE EXTRACTION (+- 6 lines of code snippet)
↓
RELEVANT DEPENDENCY SUBGRAPH (Attached Components & Connected Scripts)
↓
TARGETED UNITY KNOWLEDGE (Exact API replacement / usage pattern)
↓
MINIMAL REPAIR PAYLOAD (< 250 tokens)
↓
HOST LLM PATCH GENERATION
↓
ATOMIC PATCH APPLICATION
↓
COMPILATION & INTEGRITY VERIFICATION
↓
COMMIT (Success) OR ROLLBACK (Failure after MAX_REPAIR_ATTEMPTS = 3)
```

---

## 🛡️ Automated vs LLM-Assisted Repairs

1. **Automated (No LLM round-trip needed)**:
   - Injects missing standard namespaces (`using UnityEngine.AI;`, `using TMPro;`, `using UnityEngine.UI;`).
   - Cleans orphaned `MissingScript` components from GameObjects.

2. **LLM-Assisted (Minimal token payload provided)**:
   - Missing variable declarations (CS0103).
   - Method signature & type mismatches (CS0117, CS0029).
   - Logic-level `NullReferenceException` repairs.
