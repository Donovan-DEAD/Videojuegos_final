# Scripts/Boss — Jefes y avance de mundo

| Archivo | Rol |
|---|---|
| `IBossController` | Interfaz de un jefe (`ConfigureBoss`, `CurrentPhase`). |
| `CaperucitaBoss` | Jefe del bosque: FSM `Stalk → Windup → Slash → Lunge → Recover → Summon → Dead`, fases por vida, invocación de lobos, enrage y `lootPrefab` al morir. |
| `BossAI` | Jefe genérico basado en `EnemyAI` + fases e invocación. |
| `BossArenaController` | Trigger de la arena: sella las salidas, instancia el jefe y dispara `BossSpawned` / `BossDefeated`. Expone `BossInstance`. |
| `BossManager` | Recibe el `WorldConfigSO`, configura la arena y **reemite `BossDefeated`**. |
| `BossSpawnPoint` | Marcador de posición de aparición del jefe. |
| `PeterPanBoss` | Jefe volador del mar: kiting, andanadas de dagas, blink, clon de sombra y oleadas de esbirros. Al invocar usa `TerrainSpawnUtility` para que cada esbirro caiga en un terreno compatible. |
| `MiniBossController` | Miniboss de cuarto con FSM `Chase → ChargeWindup → Charge → Recover`: persigue, ataca cuerpo a cuerpo y embiste con windup (parpadeo blanco). Se configura por prefab: `MiniBoss_LoboAlfa` (bosque, rápido) y `MiniBoss_Garfio` (mar, pesado). Suelta `AbilityPickup` y muestra el outro al morir. |

## Cadena de avance

`CaperucitaBoss` muere → `Health.Died` → `BossArenaController.OnBossDefeated` → `BossDefeated` →
`BossManager.BossDefeated` → `WorldPortalController` **activa el Portal** →
el jugador lo toca → `RunManager.RequestAdvance()` → `02_Loading` → `03_Gameplay` con el **siguiente mundo**
(índice persistido en `GameFlowManager`).

> Si no hay `BossManager` en la escena (p. ej. `Greybox`, con el jefe colocado a mano),
> `WorldPortalController` engancha directamente el `Health` del jefe de la escena.

> Peter Pan invoca con `TerrainSpawnUtility.ResolveSpawnPosition`: la Mermaid busca agua, el
> CabinBoy/Crocodile piso/anfibio y los voladores cualquier celda libre.

Ver: [../Gameplay/README.md](../Gameplay/README.md) · [../Core/README.md](../Core/README.md)

## Minibosses por mundo

- **Lobo Alfa** (`Assets/Prefabs/NPCs/MiniBoss_LoboAlfa.prefab`, bosque): 60 vida,
  vel. 3.5, zarpazo 8 dmg, embestida veloz (windup 0.3 s, dash 12, 10 dmg). Suelta WolfClaw.
- **Capitán Garfio** (`Assets/Prefabs/NPCs/MiniBoss_Garfio.prefab`, mar): 90 vida, vel. 2.2,
  zarpazo 12 dmg, embestida pesada (windup 0.45 s, dash 9, 16 dmg). Suelta PeterDagger.
- Intros/outros propios en `Assets/Data/Intros/` (`BossIntro_Miniboss_*.asset`).
- El jefe del mar tiene su propia intro (`BossIntro_PeterPan`); su recompensa es FairyDust.
