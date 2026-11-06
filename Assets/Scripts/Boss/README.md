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
