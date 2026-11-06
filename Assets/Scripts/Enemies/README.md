# Scripts/Enemies — Esbirros

IA de los enemigos comunes (los jefes viven en `Scripts/Boss`).

| Archivo | Rol |
|---|---|
| `EnemyAI` | FSM `Patrol → Alert → Chase → Attack → Return → Hide → Dead`. Actualiza el `Animator` (int `State`), escala por `EnemyStatsSO` y dispara `Died`. Al morir desactiva colliders, reproduce el estado `Death` y destruye el objeto tras `deathLingerTime`. El estado `Hide` retira al enemigo a su terreno de escondite y lo mantiene quieto un tiempo. |
| `EnemyMovement` | Aplica velocidad con `SpeedMultiplier`; restringe el paso según `EnemyHabitat` (Ground/Water/Flying) y expone `LastMoveBlocked`/`IsOnWallCell`. |
| `EnemyAttack` | Ejecuta melee/ranged según `AttackType` y expone `IsWindingUp`, `AttackType`. |
| `EnemyFOV` | Cono de visión + línea de vista y radio de audición; `VisionThroughWalls` opcional. |
| `TerrainSpawnUtility` | Compatibilidad hábitat/terreno y búsqueda de posiciones para spawns e invocaciones. |
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

## Terreno y hábitats

- `EnemyHabitat` (flags): `Ground`, `Water`, `Flying`. `EnemyStatsSO` define `habitat`, `passThroughWalls`
  y `visionThroughWalls`; `EnemyArchetypeSO.Habitat` los lee para el spawner.
- `EnemyMovement` consulta `MapGenerator.GetTerrain`: Ground solo `Floor`, Water solo `Water`,
  Ground|Water ambos, Flying cualquiera. Si la dirección directa está bloqueada, desliza en X/Y; si no,
  se detiene y marca `LastMoveBlocked`.
- `EnemyAI` aplica `Collider2D.excludeLayers = Walls` a los voladores (siguen en `Enemies` y reciben daño);
  `EnemyFOV` puede ignorar muros con `VisionThroughWalls` (apagado por defecto).
- `TerrainSpawnUtility` reutiliza la compatibilidad para el spawner y las invocaciones de jefes.

## Comportamiento por personaje (autómata)

La FSM agrega **`Hide`** y dos temporizadores:

- `hideTimer`: si `hidesPeriodically`, entra en `Hide` cada `[min,max]` segundos.
- `outOfWaterTimer`: si `maxTimeOutOfWater > 0` (Cocodrilo), se acumula mientras pisa suelo; al vencer
  vuelve al agua.
- `Hide`: va a la celda más cercana de `hideTerrain` (`Water`/`Wall`), se queda `hideDuration` s y luego
  vuelve a `Chase` (si ve al jugador) o `Patrol`. Si `hideWhileNotVisible`, oculta borde/etiqueta.

| Enemigo | hide | hideTerrain | outOfWater | intervalo |
|---|---|---|---|---|
| Mermaid | sí | Water | 0 | 5–8 s |
| Crocodile | sí | Water | 4 s | 6–10 s |
| DustFairy / Firefly | sí | Wall | 0 | 7–12 s |
| Ground (Wolf/Bramble/Woodcutter/CabinBoy) | no | — | 0 | — |



**Identidad:** cada `EnemyStatsSO` (`Assets/ScriptableObjects/Enemies/`) define `displayName` e
`identityColor`; `EnemyAI` crea/aplica `EnemyIdentity` en `Awake`, así que todos los enemigos instanciados
(incluidos los invocados por jefes) quedan identificados sin tocar los prefabs.
