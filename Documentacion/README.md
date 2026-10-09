# Madness Dreams

Roguelite 2D top-down (estilo *Binding of Isaac* / *Cult of the Lamb*): mundos de cuentos
distorsionados, generación procedural por semilla, jefes por fases y progresión persistente.

> **Documentación técnica detallada:** ver **[`Estado_Implementacion_Unity.md`](./Estado_Implementacion_Unity.md)**
> (qué existe hoy en Unity, escena por escena y sistema por sistema).
> El *Manual de montaje* y el *Flujo de componentes* siguen siendo válidos como guía conceptual; este
> README resume el estado real del proyecto.

---

## Estructura de scripts (`Assets/Scripts/`)

```
Boss/       Jefes y arena (BossAI, CaperucitaBoss, PeterPanBoss, BossArenaController, BossManager)
Core/       Vida, proyectil, RunManager y enums (Health, Projectile, AnimationState, AttackType)
Data/       ScriptableObjects (WorldConfig, BlockBank, MapChunk, EnemyStats, BossStats,
            EnemyArchetype, BossAbility)
Editor/     Herramienta de captura de chunks (Tools > Madness Dreams > Map Chunk Capture)
Enemies/    IA y combate de esbirros (EnemyAI, EnemyMovement, EnemyAttack, EnemyFOV, SlowEffect)
Gameplay/   Flujo de nivel (WorldLayout, WorldPortalController, Portal)
Generation/ Generación procedural (MapGenerator, TilemapStamper, ChunkContent, Spawner, CellRole)
HUD/        Interfaz de gameplay (HUDController y widgets, Pausa, Mapa, Menú DEV)
Pickups/    Recogibles (AbilityPickup)
Player/     Jugador (Controller, Combat, Abilities, Stats, Animation, DeathHandler)
UI/         Flujo de pantallas (GameFlowManager, ScreenFader, LoadingScreen, Settings, Menús)
```

## Escenas (`Assets/Scenes/`)

| Escena | Rol |
|---|---|
| `01_MainMenu` | Menú principal (primera del build) |
| `02_Loading` | Pantalla de carga |
| `03_Gameplay` | Nivel jugable |
| `04_Defeat` | Derrota (Reintentar / Menú / Salir) |
| `05_Settings` | Ajustes |
| `Greybox` | Escena de pruebas (mapa grande + jefe) |

## Cómo ejecutar

- **Juego completo:** abrir `01_MainMenu` → **Play** → Jugar.
- **Pruebas:** abrir `Greybox` → **Play**.

## Controles

| Tecla | Acción |
|---|---|
| `WASD` / flechas | Mover |
| `Space` | Melee |
| `K` | Ranged (hacia el mouse) |
| `1`–`4` | Habilidades de jefe |
| `Esc` | Pausa (Reanudar / Ajustes / Menú) |
| `M` / clic en el minimapa | Mapa grande |

---

## Notas de proyecto

- **Animación por color (greybox):** el `Animator` usa un parámetro int **`State`**
  (0 Idle, 1 Move, 2 MeleeAttack, 3 RangedAttack, 4 Death) y los clips animan
  `SpriteRenderer.m_Color`. El arte final solo reemplaza los clips/sprite.
- **Habilidades (LOTE 2):** implementadas y **desbloqueadas desde el inicio** para testear
  (`PlayerAbilities.startingAbilities`); además, el jefe **suelta un pickup** al morir.
- **Menú DEV:** botón `DEV` en el HUD para nivel, estadísticas base y bloqueo de habilidades.
- **Layers**: `Ground, Walls, Player, Enemies, PlayerAttack, EnemyAttack, Pickups, Hazards,
  BossTrigger` (índices 6–14) + tag `Player`.
- **Input Handling = Both**; el jugador usa el nuevo Input System con *fallback* legacy.
- **Cinemachine 3.1.7** para el seguimiento de cámara.

## Crear ScriptableObjects

Desde `Create > Madness Dreams > ...`: `Map Chunk`, `Block Bank`, `Enemy Stats`, `Boss Stats`,
`Enemy Archetype`, `World Config`, `Boss Ability`.

## Capturar un room a `MapChunkSO`

1. Pintá el room en un `Tilemap` limpio.
2. `Tools > Madness Dreams > Map Chunk Capture`.
3. Asigná `Source Tilemap` y `Target MapChunkSO` → **Capture**.

> La captura solo guarda `size` y `tiles`; `doorPositions`, `enemySpawns` y `role` se llenan a mano.
