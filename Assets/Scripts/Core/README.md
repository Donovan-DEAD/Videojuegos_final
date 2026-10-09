# Scripts/Core — Núcleo compartido

Tipos transversales que usan todas las capas.

| Archivo | Rol |
|---|---|
| `Health` / `IDamageable` | Vida con `CurrentHealth`, `MaxHealth`, `TakeDamage`, y los eventos `HealthChanged` y `Died`. |
| `Projectile` | Proyectil que viaja, colisiona contra `targetMask` y puede aplicar `SlowEffect`. |
| `RunManager` | Configura y genera el mundo actual (`GenerateWorld`), guarda la lista de `WorldConfigSO` y avanza con `RequestAdvance()` (portal → siguiente mundo). |
| `AttackType` | Enum `Melee` / `Ranged`. |
| `AnimationState` | Enum de estados del `Animator` (0 Idle … 4 Death). |

## Notas

- `Health.Died` es el punto de enganche de **muerte**: lo escuchan `EnemyAI`, `CaperucitaBoss`,
  `PlayerDeathHandler` y `BossArenaController`.
- `RunManager` lee `GameFlowManager.PendingWorldIndex` al arrancar la escena, por lo que el avance
  de mundo se resuelve recargando `03_Gameplay`.
