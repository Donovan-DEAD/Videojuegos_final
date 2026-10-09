# Scripts/Pickups — Recogibles

| Archivo | Rol |
|---|---|
| `AbilityPickup` | Al tocarlo llama a `PlayerAbilities.Learn(...)`; sólo se destruye si la habilidad se aprendió (si ya la tenía, queda en el mapa). |

**Prefabs:** `Assets/Prefabs/Pickups/Pickup_WolfClaw`, `Pickup_ThornShot`, `Pickup_PeterDagger`,
`Pickup_FairyDust` (cada uno referencia su `BossAbilitySO`).

El jefe puede soltar un pickup al morir mediante su campo `lootPrefab` (ver `Scripts/Boss`).
