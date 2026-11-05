# Scripts/Enemies — Esbirros

IA de los enemigos comunes (los jefes viven en `Scripts/Boss`).

| Archivo | Rol |
|---|---|
| `EnemyAI` | FSM `Patrol → Alert → Chase → Attack → Return → Dead`. Actualiza el `Animator` (int `State`), escala por `EnemyStatsSO` y dispara `Died`. Al morir desactiva colliders, reproduce el estado `Death` y destruye el objeto tras `deathLingerTime`. |
| `EnemyMovement` | Aplica velocidad; expone `SpeedMultiplier` para debuffs. |
| `EnemyAttack` | Ejecuta melee/ranged según `AttackType` y expone `IsWindingUp`, `AttackType`. |
| `EnemyFOV` | Cono de visión + línea de vista y radio de audición. |
| `SlowEffect` | Multiplicador temporal de velocidad (lo aplica el proyectil de espinas). |
| `EnemyIdentity` | Identificación greybox: borde del color del arquetipo (hijo, fuera del Animator) + etiqueta flotante con `EnemyStatsSO.displayName`. Se oculta al morir. |

## Estados de animación (`Animator` int `State`)

| Valor | Estado |
|---|---|
| 0 | Idle |
| 1 | Move |
| 2 | MeleeAttack |
| 3 | RangedAttack |
| 4 | Death |

Reproducir: `Assets/Animations/Enemy/EnemyAnimator.controller`. Cada clip anima `SpriteRenderer.m_Color`
(el color **es** el estado hasta tener arte final).

**Prefabs:** `Assets/Prefabs/Enemies/` (los 8 tipos del GDD) + `Projectile_Enemy`.

**Identidad:** cada `EnemyStatsSO` (`Assets/ScriptableObjects/Enemies/`) define `displayName` e
`identityColor`; `EnemyAI` crea/aplica `EnemyIdentity` en `Awake`, así que todos los enemigos instanciados
(incluidos los invocados por jefes) quedan identificados sin tocar los prefabs.
