# 🧪 Automated Play Mode & Gameplay Smoke Testing

The **Playtest Engine** (`unity_run_playtest`) enables automated runtime smoke testing to verify that generated systems and GameObjects are fully functional and error-free.

---

## 🎯 Validation Checks Performed

1. **Target Object Presence**: Locates the target GameObject (e.g. `PlayerCharacter`, `Police_Car`) in the active hierarchy.
2. **Component Integrity**: Inspects attached `CharacterController`, `Rigidbody`, `WheelCollider`, and custom scripts.
3. **Camera Attachment**: Verifies active Main Camera presence and alignment.
4. **Console Cleanliness**: Ensures zero runtime `NullReferenceException`, `MissingReferenceException`, or unhandled errors occur during execution.

---

## 📊 Quality Gate Integration

The Playtest Engine directly feeds the **Quality Gate** (`unity_quality_gate`), contributing to the objective 0-100 quality score for every automated build.
