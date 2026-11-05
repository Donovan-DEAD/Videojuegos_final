# Scripts/Core — Núcleo compartido

Tipos transversales que usan todas las capas.

| Archivo | Rol |
|---|---|
| `Health` / `IDamageable` | Vida con `CurrentHealth`, `MaxHealth`, `TakeDamage`, y los eventos `HealthChanged` y `Died`. |
| `Projectile` | Proyectil que viaja, colisiona contra `targetMask` y puede aplicar `SlowEffect`. |
| `RunManager` | Configura el mundo (`PrepareWorld`), lo genera de forma incremental en `Start` (`GenerationProgress`, `IsWorldReady`, `WorldReady`), guarda la lista de `WorldConfigSO` y avanza con `RequestAdvance()` (portal → siguiente mundo, usando el interludio del mundo que se entra). |
| `AttackType` | Enum `Melee` / `Ranged`. |
| `AnimationState` | Enum de estados del `Animator` (0 Idle … 4 Death). |

## Notas

- `Health.Died` es el punto de enganche de **muerte**: lo escuchan `EnemyAI`, `CaperucitaBoss`,
  `PlayerDeathHandler` y `BossArenaController`.
- `RunManager` lee `GameFlowManager.PendingWorldIndex` al arrancar la escena, por lo que el avance
  de mundo se resuelve recargando `03_Gameplay`.
- `RunManager.Awake` activa su propia escena antes de generar. La escena de gameplay se carga de forma
  **aditiva** desde `02_Loading`; sin ese cambio, los props/enemigos instanciados caían en la pantalla
  de carga y se destruían al descargarla.
