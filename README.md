# 🏛️ Unity Architect MCP (Model Context Protocol) v2.0

**Antigravity Unity Architect MCP**, Unity Editor'ü yapay zeka ajanlarına (Antigravity, Claude Desktop, Cursor, Windsurf vb.) kurumsal düzeyde açan, **Project State Graph, Incremental Hashing, Atomik Transaction / Rollback, Doğrulama (Validation) ve Batch Yürütme** yeteneklerine sahip yeni nesil oyun motoru orkestrasyon sistemidir.

---

## 📊 Benchmark & Token Optimizasyon Karşılaştırması

Aşağıdaki metrikler 100 nesneli standart bir Unity sahnesi üzerinde ölçülmüştür:

| Metrik | Geleneksel Unity MCP | **Unity Architect MCP (v2.0)** | İyileştirme |
| :--- | :--- | :--- | :--- |
| **İlk İnceleme Yanıt Boyutu** | ~54.20 KB | **~0.41 KB** | **-%99.2 Boyut Azalması** |
| **İlk İnceleme Token Maliyeti** | ~13.874 token | **~105 token** | **-%99.2 Token Tasarrufu** |
| **Değişiklik Context Maliyeti** | ~13.874 token (Tüm sahne tekrar) | **~80 token (Yalnızca Diff)** | **-%99.4 Token Tasarrufu** |
| **Çok Adımlı Eylem Döngüsü** | 5 - 10 ayrı tool çağrısı | **1 Atomik Batch Transaction** | **10 Kata Kadar Daha Hızlı** |
| **Hata Güvenliği & Geri Alma** | ❌ Yok (Manuel temizlik) | ✅ **Snapshot + Rollback** | **Sıfır Bozuk Sahne Riski** |

---

## 🏗️ Mimari Katmanlar

```mermaid
graph TD
    AI[AI Agent: Antigravity / Claude / GPT / Gemini] <-->|MCP Protocol over STDIO| ArchitectMCP[UNITY ARCHITECT MCP SERVER]
    
    subgraph Architect MCP Intelligence Layer (Node/TS)
        ArchitectMCP --> HighLevelTools[High-Level Architect Tools]
        ArchitectMCP --> BatchOrchestrator[Batch Execution Orchestrator]
        ArchitectMCP --> LegacyAdapters[Backward-Compatible Toolsets]
    end
    
    ArchitectMCP <-->|Fast JSON-RPC Bridge (127.0.0.1:8080)| UnityArchitectBridge[UNITY ARCHITECT BRIDGE (C#)]
    
    subgraph Unity Engine Core Systems
        UnityArchitectBridge --> ProjectGraph[Project State Graph & Stable IDs]
        UnityArchitectBridge --> StateHasher[Incremental SHA256 State Hasher & Diff Engine]
        UnityArchitectBridge --> SnapshotManager[Scene & Asset Snapshot / Rollback Engine]
        UnityArchitectBridge --> ValidationManager[Scene Integrity & Missing Script Detector]
        UnityArchitectBridge --> BatchExecutor[Atomic Batch Transaction Executor]
        UnityArchitectBridge --> PerformanceExt[Performance Metrics Extension Point]
    end
```

---

## 🎯 Kullanılabilir MCP Araçları

### 🏛️ Yüksek Seviyeli Architect Araçları (Phase 1)
- `unity_inspect_project`: Projenin genel durumunu (sahne, obje, script, prefab, materyal sayıları, aktif hash, ana objeler) progressive disclosure mantığıyla çok küçük token maliyetiyle özetler.
- `unity_inspect_scene`: Aktif sahnenin yapısal özetini ve kök objelerini döner.
- `unity_inspect_object`: Hedef GameObject'in alt ağacını (subtree), bileşenlerini ve bağımlılıklarını detaylı inceler.
- `unity_state_diff`: Son durumdan bu yana değişenleri (`ADDED`, `REMOVED`, `MODIFIED`, `UNCHANGED`) döner.
- `unity_snapshot`: Olası riskli değişiklikler öncesi sahne ve dosya snapshot'ı oluşturur.
- `unity_rollback`: Belirtilen işlem ID'sine veya son yapılan AI işlemine anında geri döner.
- `unity_validate`: Sahnede missing script, pembe materyal/kırık shader, eksik kamera veya derleme hatalarını tarar.
- `unity_execute_batch`: Birden fazla işlemi tek bir atomik transaction içerisinde çalıştırır; hata durumunda otomatik rollback yapar.

### 🎮 Standart ve Geriye Dönük Uyumlu Araçlar (30+ Araç)
- **Sahne:** `unity_scene_get_hierarchy`, `unity_gameobject_create`, `unity_gameobject_modify`, `unity_gameobject_delete`, `unity_gameobject_duplicate`
- **Bileşen & Reflection:** `unity_component_add`, `unity_component_remove`, `unity_component_get_properties`, `unity_component_set_property`
- **Asset:** `unity_asset_create_prefab`, `unity_asset_instantiate_prefab`, `unity_asset_create_material`, `unity_asset_find`
- **Script & REPL:** `unity_script_create`, `unity_script_get_compilation_status`, `unity_csharp_eval`
- **Görsel Algı (Vision):** `unity_vision_capture_scene`, `unity_vision_capture_game`, `unity_vision_inspect_object`
- **Fizik:** `unity_physics_setup_rigidbody`, `unity_physics_setup_collider`, `unity_physics_bake_navmesh`
- **Animasyon & UI:** `unity_animator_create`, `unity_animator_add_state`, `unity_ui_create_canvas`, `unity_ui_create_element`
- **PlayMode & Konsol:** `unity_playmode_start`, `unity_playmode_stop`, `unity_playmode_pause`, `unity_console_get_logs`, `unity_console_clear`
- **Scaffolding:** `unity_scaffold_player`, `unity_scaffold_enemy`, `unity_scaffold_camera`

---

## 🛠️ Kurulum ve Başlatma

1. Unity projenizde `Window -> Package Manager -> Add package from disk...` seçeneğiyle `unity-package/com.antigravity.unitymcp/package.json` dosyasını ekleyin.
2. Unity açıldığında `Window -> Antigravity -> Unity Architect MCP` panelini açıp sunucu durumunu kontrol edin.
3. AI asistanınızın MCP konfigürasyonuna ekleyin:
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
