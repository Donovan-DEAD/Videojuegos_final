# Scripts/Player — Jugador

| Archivo | Rol |
|---|---|
| `PlayerController` | Movimiento (nuevo Input System con fallback legacy), `ControlEnabled` y `SetMoveSpeed`. |
| `PlayerCombat` | Melee (`Space`) y ranged (`K` hacia el mouse); expone `MeleeCooldownNormalized`, `RangedCooldownNormalized` y `SetDamageMultiplier`. |
| `PlayerAbilities` | 4 slots (`1`–`4`): `Learn`, `UseAbility`, `GetCooldownNormalized`, `IsSlotLocked`/`SetSlotLocked`. |
| `PlayerStats` | Nivel (1..20) + estadísticas **base** y crecimiento por nivel; aplica a `PlayerController`, `PlayerCombat` y `Health`. |
| `PlayerAnimationController` | Actualiza el `Animator` (int `State`) y expone `CurrentState`. |
| `PlayerDeathHandler` | `Health.Died` → `GameFlowManager.NotifyPlayerDied()` → pantalla de Derrota. |

## Estados de animación (`Animator` int `State`)

| Valor | Estado |
|---|---|
| 0 | Idle |
| 1 | Move |
| 2 | MeleeAttack |
| 3 | RangedAttack |
| 4 | Death |

Reproducir: `Assets/Animations/Player/PlayerAnimator.controller` (el color representa el estado).

**Habilidades** (`BossAbilitySO`): WolfClaw (dash + golpe), ThornShot (proyectil + slow),
PeterDagger (abanico), FairyDust (dash). Se aprenden con `AbilityPickup` (ver `../Pickups`).

**Stats editables en runtime** desde el menú **DEV** del HUD (nivel, stats base y bloqueo de habilidades).
