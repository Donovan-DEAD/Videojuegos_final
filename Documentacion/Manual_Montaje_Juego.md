# Madness Dreams — Manual de montaje completo

> Guía paso a paso para armar el juego desde cero en Unity: configuración de la escena,
> generación procedural, jugador, enemigos, jefes (Caperucita y Peter Pan) y el sistema de
> objetos/habilidades.
>
> **Alcance del demo:** el juego completo tiene muchos mundos de cuentos. Para la entrega de
> clase solo se montan **dos mundos**: el *Bosque de Caperucita* y el *Mar de Nunca Jamás*.
> El resto es visión del juego ideal, no parte del demo.
>
> **Estado (actualizado):** todos los scripts de `Assets/Scripts/` están implementados, **incluido
> el Lote 2** (objetos y habilidades). Se agregaron además: el **mapa procedural por cuartos de tamaño
> fijo por celdas con ambientación por tag** (`RoomLayout` → `MapGenerator` → `RoomContentSpawner`),
> props y barreras naturales, dificultad escalada por nivel, flujo de pantallas con fundidos, HUD,
> pausa/ajustes in-game, minimapa/mapa, menú DEV, animadores (estado por color), Cinemachine y el
> flujo de **portal → carga → siguiente mundo**. Ver
> [`Estado_Implementacion_Unity.md`](./Estado_Implementacion_Unity.md).

---

## Estado de implementación (resumen)

- **Scripts:** carpeta completa en `Assets/Scripts/` (Boss, Core, Data, Editor, Enemies, Gameplay,
  Generation, HUD, Pickups, Player, UI), **todos con documentación XML**. El **LOTE 2** ya existe:
  `BossAbilitySO`, `PlayerAbilities`, `AbilityPickup`, `SlowEffect`.
- **Escenas:** `01_MainMenu`, `02_Loading`, `03_Gameplay`, `04_Defeat`, `05_Settings` y `Greybox`
  (todas en Build Settings).
- **Mapa procedural:** `RoomLayout` (grafo conexo de cuartos de tamaño fijo por celdas, con **Spawn y
  Boss garantizados**), `MapGenerator` (estampado + pasos) y `RoomContentSpawner` (**barreras, props y
  enemigos por tag** desde `ZoneThemeSO`, con dificultad por `DifficultyProfileSO`).
- **HUD:** prefab `Assets/Prefabs/UI/HUD.prefab` (vida, ataques, hotbar de habilidades, barra del
  jefe, minimapa/mapa por cuartos, pausa+ajustes, menú DEV).
- **Assets de sample** ya creados: arquetipos de cuarto por tamaño×tag + boss 3×3, 6 props/barreras,
  `ZoneTheme_Forest/Sea`, `DifficultyProfile_Default`, enemigos del GDD, jefe, proyectiles, pickups,
  tile, banco y 2 `WorldConfigSO`.
- **Editor:** `Tools > Madness Dreams > Map Chunk Capture` (captura tiles, `sizeInCells`, tag y spawn
  points desde los hijos) y `Validate Worlds` (1 Spawn y 1 Boss por mundo).

> En este manual, las fases que dependen de código ya no requieren “programar”: solo montar/enlazar
> en el editor. Donde diga `[LOTE 2]`, el código ya está (ver §“Estado de implementación”).

---

## Índice de fases

| Fase | Qué hace | Se hace en |
|---|---|---|
| 0 | Preparar el proyecto y las carpetas | Editor |
| 1 | Layers y Tags | Editor |
| 2 | Animator Controller compartido | Editor |
| 3 | ScriptableObjects base (stats, arquetipos, mundos) | Editor |
| 4 | Rooms / chunks (`MapChunkSO`) | Editor |
| 5 | Escena base (Grid, Tilemap, cámara) | Editor |
| 6 | Generación del mundo | Editor |
| 7 | Jugador | Editor |
| 8 | Proyectiles | Editor |
| 9 | Enemigos | Editor |
| 10 | Jefes: Caperucita y Peter Pan | Editor |
| 11 | `[LOTE 2]` Objetos y habilidades | Editor |
| 12 | Flujo de mundos y cierre de partida | Editor |
| 13 | Checklist de prueba end-to-end | Play Mode |
| 14 | Problemas comunes | — |

**Orden de dependencias (importante):** no se puede crear un `WorldConfigSO` antes que su
`BlockBankSO`, ni este antes de sus `MapChunkSO`, ni un `EnemyArchetypeSO` antes de su
`EnemyStatsSO`. Respetá el orden de las fases.

---

## Fase 0 — Preparar el proyecto

1. Creá un proyecto **Unity 2D** (Unity 2021.3 LTS o superior; el demo se prueba en 2D).
2. Copiá la carpeta `Assets/Scripts/` completa dentro de `Assets/` del proyecto.
3. Verificá que queden estas subcarpetas:

```
Assets/Scripts/
├── Boss/         IBossController, CaperucitaBoss, PeterPanBoss, BossAI, BossArenaController, BossManager, BossSpawnPoint
├── Core/         Health, Projectile, RunManager, AnimationState, AttackType
├── Data/         ScriptableObjects (configuración y plantillas)
├── Editor/       MapChunkCaptureEditor (captura de cuartos + Validate Worlds)
├── Enemies/      EnemyAI, EnemyMovement, EnemyAttack, EnemyFOV, SlowEffect
├── Gameplay/     WorldLayout, WorldPortalController, Portal
├── Generation/   RoomLayout, MapGenerator, RoomContentSpawner, TilemapStamper, Spawner, CellRole
├── HUD/          HUDController y widgets, Pausa, Mapa, Menú DEV
├── Pickups/      AbilityPickup
├── Player/       PlayerController, PlayerCombat, PlayerAbilities, PlayerStats, PlayerAnimationController, PlayerDeathHandler
└── UI/           GameFlowManager, ScreenFader, LoadingScreen, SettingsController, Menús
```

4. Si Unity marca errores de compilación, revisá que `Assets/Scripts/Editor/` esté dentro de
   una carpeta llamada exactamente `Editor` (si no, `UnityEditor` no está disponible en builds).
5. Creá la carpeta `Assets/Prefabs/` para los prefabs y `Assets/ScriptableObjects/` para los assets.

> **Nota:** los `.cs` se editan en un IDE, pero **todos los `.asset` y los prefabs se crean en
> el editor de Unity**, no por código. Cada paso de este manual indica qué se hace en cada lado.

---

## Fase 1 — Layers y Tags

### 1.1 Crear las Physics Layers

`Edit > Project Settings > Tags and Layers` → sección **Layers**. En índices libres (a partir
del 6) creá, con estos nombres exactos:

| Layer | Para qué |
|---|---|
| `Ground` | Piso transitable |
| `Walls` | Colisiones del tilemap (muros) |
| `Player` | El jugador |
| `Enemies` | Enemigos y jefes |
| `PlayerAttack` | Proyectiles del jugador |
| `EnemyAttack` | Proyectiles de enemigos y jefes |
| `Pickups` | Objetos recogibles |
| `Hazards` | Trampas y daño ambiental |
| `BossTrigger` | Trigger que arranca la pelea del jefe |

### 1.2 Crear el Tag

`Edit > Project Settings > Tags and Layers` → **Tags** → confirmá que exista `Player`
(es un tag por defecto; si lo borraste, recreálo con ese nombre exacto).
Todo el código busca al jugador con `GameObject.FindGameObjectWithTag("Player")` y
`CompareTag("Player")`.

### 1.3 Configurar la matriz de colisión

`Edit > Project Settings > Physics 2D > Layer Collision Matrix`. Marcá como mínimo:

| A | B | ¿Colisionan? | Por qué |
|---|---|---|---|
| `Player` | `Ground` | Sí | Caminar |
| `Player` | `Walls` | Sí | No atravesar muros |
| `Player` | `Enemies` | Sí | Contacto con enemigos |
| `Player` | `EnemyAttack` | Sí | Recibir proyectiles |
| `Player` | `Pickups` | Sí | Recoger objetos |
| `Player` | `BossTrigger` | Sí | Disparar la pelea del jefe |
| `Enemies` | `Ground` / `Walls` | Sí | Navegación |
| `Enemies` | `PlayerAttack` | Sí | Recibir daño melee/ranged del jugador |
| `Enemies` | `EnemyAttack` | **No** | Los proyectiles enemigos no se pegan entre ellos |
| `EnemyAttack` | `Player` | Sí | Único objetivo de los proyectiles enemigos |
| `PlayerAttack` | `Enemies` | Sí | Único objetivo de los proyectiles del jugador |
| `PlayerAttack` | `Player` | **No** | El jugador no se dispara a sí mismo |
| `Walls` | `PlayerAttack` / `EnemyAttack` | **No** | Los proyectiles atraviesan muros (el daño ya se filtra por `targetMask`) |

> **Regla mental:** los proyectiles solo chocan contra su objetivo. El daño se filtra una
> segunda vez por `targetMask` en `Projectile` y `EnemyAttack`, así que la matriz es la
> primera barrera y el `LayerMask` la segunda.

---

## Fase 2 — Animator Controller compartido

Enemigos, jefes y jugador usan **el mismo esquema**: un **único parámetro `int` llamado
`State`**. Los estados lógicos (FSM) no son estados del Animator: el código traduce FSM →
animación.

1. En `Assets/Animations/` creá un `Animator Controller`.
2. Añadí un parámetro **int** llamado exactamente `State` (respetá mayúscula).
3. Creá 5 estados y asignales los clips **en este orden de valor**:

| Valor `State` | Nombre del estado | Cuándo se usa |
|---|---|---|
| 0 | `Idle` | quieto |
| 1 | `Move` | moviéndose |
| 2 | `MeleeAttack` | ataque cuerpo a cuerpo |
| 3 | `RangedAttack` | ataque a distancia |
| 4 | `Death` | muriendo |

4. **No hace falta ningún parámetro booleano ni trigger.** El código solo llama a
   `animator.SetInteger("State", valor)`.

> En `EnemyAI` y en los jefes el nombre del parámetro es un campo serializado
> (`stateParameter`, por defecto `"State"`), así que podés renombrarlo si querés — pero
> entonces cambialo también en el Inspector.

---

## Fase 3 — ScriptableObjects base

Todo se crea desde el menú **`Assets > Create > Madness Dreams > ...`**
(click derecho en la ventana Project → `Create > Madness Dreams`).

Creá **en este orden**, porque cada uno referencia al anterior:

### 3.1 `Enemy Stats` (`EnemyStatsSO`)

Plantilla de propiedades de un enemigo. Campos y valores por defecto:

| Campo | Default | Qué controla |
|---|---|---|
| `moveSpeed` | 2 | Velocidad de patrulla / regreso |
| `chaseSpeed` | 3.5 | Velocidad al perseguir |
| `maxHealth` | 100 | Vida máxima |
| `damage` | 10 | Daño por golpe o por proyectil |
| `attackRange` | 1.5 | Distancia a la que ataca |
| `attackCooldown` | 1.5 | Segundos entre ataques |
| `windupTime` | 0.3 | Anticipación antes de golpear (telegraph) |
| `attackType` | `Melee` | `Melee` o `Ranged` |
| `projectilePrefab` | — | **Solo Ranged**: qué proyectil dispara |
| `projectileSpeed` | 8 | **Solo Ranged**: velocidad del proyectil |
| `detectionRadius` | 8 | Radio máximo de persecución (leash) |
| `fovRadius` | 6 | Alcance de la visión |
| `fovAngle` | 120 | Cono de visión en grados (360 = omnidireccional) |
| `hearingRadius` | 4 | Radio en el que te "escucha" aunque no te vea |

> **Truco:** `fovAngle = 360` hace que el enemigo vea en todas direcciones (útil para jefes).

### 3.2 `Boss Stats` (`BossStatsSO`)

Hereda de `EnemyStatsSO` (todos los campos de arriba) y añade:

| Campo extra | Default | Qué controla |
|---|---|---|
| `phaseHealthThresholds` | `[0.66, 0.33]` | Ratios de vida (0..1, **ascendentes**) que disparan un cambio de fase |
| `phaseAttacks` | vacío | **Sin uso en el código actual** (reservado; podés dejarlo vacío) |

Cómo funcionan los umbrales: con `[0.66, 0.33]` → **fase 1** de 100% a 66%, **fase 2** de 66%
a 33%, **fase 3** por debajo de 33%.

### 3.3 `Enemy Archetype` (`EnemyArchetypeSO`)

Vincula un **prefab** con sus **stats**, su **tier** de poder y su peso.

| Campo | Qué poner |
|---|---|
| `prefab` | El prefab del enemigo (ver Fase 9) |
| `stats` | Su `EnemyStatsSO` |
| `tier` | Nivel de poder (1 = débil); la dificultad filtra por tier según el nivel del jugador |
| `weight` | Peso relativo dentro del banco del `ZoneThemeSO` |

Se usan en el `enemyBank` de cada `ZoneThemeSO`. **Cada enemigo distinto necesita su propio arquetipo.**

### 3.4 `Map Chunk` (`MapChunkSO`)

Ver **Fase 4** (se crea mejor después de tener un room pintado).

### 3.5 `Block Bank` (`BlockBankSO`)

| Campo | Qué poner |
|---|---|
| `normalChunks` | Lista de `MapChunkSO` **normales** (de todos los tamaños y tags) |
| `startRoom` | Arquetipo del **cuarto de Spawn** (1×1 recomendado) |
| `bossArena` | Un único `MapChunkSO` con la arena del jefe (3×3 recomendado) |

### 3.6 `World Config` (`WorldConfigSO`)

| Campo | Default | Qué controla |
|---|---|---|
| `seed` | 0 | Semilla del mapa (mismo número = mismo mapa) |
| `bank` | — | Su `BlockBankSO` |
| `width` / `height` | 14 | Tamaño de la rejilla, **en celdas** (grande = menos repetición) |
| `chunkSize` | 16 | Lado de la **celda** en tiles (los arquetipos deben ser múltiplos de esto) |
| `bossRegionSize` | 3 | (legacy) tamaño de la región del jefe |
| `sizeWeights` | 4,3,1,1 | Peso por clase de tamaño (1×1 / 2×1-1×2 / 2×2 / 3×3) |
| `openingWidth` | 4 | Ancho (en tiles) de los pasos entre cuartos |
| `fillRatio` | 1.0 | Cobertura de la rejilla (1 = todo es cuarto) |
| `themes` | — | Lista `tag → ZoneThemeSO` (qué bancos usa cada tag) |
| `difficulty` | — | `DifficultyProfileSO` (densidad y tier por nivel del jugador) |
| `bossPrefab` | — | Prefab del jefe del mundo |
| `boss` | — | Su `BossStatsSO` |

### 3.7 `[LOTE 2]` `Boss Ability` (`BossAbilitySO`)

Ver Fase 11.

---

## Fase 4 — Arquetipos de cuarto (`MapChunkSO`)

Un **arquetipo** es una plantilla de cuarto con **tamaño en celdas** (`sizeInCells`: 1×1, 2×1, 1×2,
2×2, 3×3), su `tag` temático, sus tiles, sus puntos de spawn de enemigos y sus regiones de props.

> **Regla de autoría:** un **único espacio abierto** con **anillo de muro de 1 tile** en el borde del
> footprint; el piso va **sin tile** (sólo los muros llevan `TileBase`).

### 4.1 Pintar el room en un Tilemap de trabajo

1. En la escena, creá un `Grid` temporal con un `Tilemap` hijo (tu "tilemap de trabajo").
2. Pintá el room que quieras (muros, piso, decoración) **en un rectángulo limpio**, sin
   huecos si podés.
3. Dejá el tilemap de trabajo **separado** del tilemap de juego: se usa solo para capturar.

### 4.2 Capturar el cuarto

1. Agregá **hijos vacíos** al tilemap (o hermanos) en las posiciones de aparición de enemigos,
   nombrados con el prefijo `EnemySpawn` (y `EnemySpawn1`, `EnemySpawn2`, …).
2. `Tools > Madness Dreams > Map Chunk Capture`.
3. **Source Tilemap** → tu tilemap de trabajo.
4. **Target MapChunkSO** → creá un `Map Chunk` y arrastralo acá.
5. **Cell size** → 16 (debe coincidir con el `chunkSize` del mundo).
6. **Tag** → la temática del cuarto (`Forest`, `Sea`, …).
7. Clic en **Capture**.

La herramienta calcula los límites **reales** de los tiles pintados (no `cellBounds`, que incluye
padding), aplana los tiles fila a fila, deriva **`sizeInCells`** y recoge los **`enemySpawns`** desde
los hijos con el prefijo.

> **Validaciones** (warnings): tamaño múltiplo de la celda, anillo de muro completo y presencia de
> spawn points.

### 4.3 Campos del `MapChunkSO`

| Campo | Qué poner |
|---|---|
| `size` | Tiles del footprint (se llenó solo) |
| `sizeInCells` | 1×1, 2×1, 1×2, 2×2 o 3×3 (se derivó solo) |
| `tag` | Temática (se llenó solo con el desplegable) |
| `role` | `Normal` para cuartos, `Boss` para la arena |
| `tiles` | Aplanado (índice = `y * size.x + x`) |
| `enemySpawns` | Lista de `{ localPosition, weight }` en tiles locales (se llenó solo desde los hijos) |
| `propRegions` | Rects locales donde esparcir props (opcional) |

### 4.4 Cuartos que necesitás como mínimo

- **Un arquetipo por clase de tamaño × tag** (1×1, 2×1, 1×2, 2×2, 3×3) para cada temática.
- **1 cuarto de Spawn** (`1×1`) en `BlockBankSO.startRoom`.
- **1 arena del jefe** (`3×3`) en `BlockBankSO.bossArena`.

> `RoomLayout` **garantiza** el cuarto de Spawn (centro) y el de Boss (el más lejano) en todos los
> mundos; el resto se empaqueta por peso. `RoomContentSpawner` coloca los enemigos según el **tier**
> permitido por el **nivel del jugador** y la densidad del `DifficultyProfileSO`.

---

## Fase 5 — Escena base

1. En la escena, creá un **`Grid`** (`GameObject > 2D Object > Tilemap > Rectangular`).
2. Renombrá su hijo a `Tilemap_Walls`.
3. En `Tilemap_Walls` añadí un **`Tilemap Collider 2D`** y poné **`Collider Type = Grid`**
   (importante: con `Sprite` genera un collider por tile y es mucho más caro).
4. Poné `Tilemap_Walls` en la **layer `Walls`**.
5. Añadí un **`Rigidbody 2D`** al `Grid` con **`Body Type = Static`** para que el tilemap
   colisione correctamente.
6. Añadí la **cámara**: `Main Camera` con `Projection = Orthographic`. El código usa
   `Camera.main` para apuntar los ataques a distancia del jugador. **Debe existir una cámara
   con el tag `MainCamera`** o los disparos del jugador usarán `transform.up`.

---

## Fase 6 — Generación del mundo

### 6.1 Objeto `Spawner`

1. Creá un GameObject vacío llamado `Spawner`.
2. Añadí el componente **`Spawner`**.

| Campo | Default | Qué poner |
|---|---|---|
| `maxTries` | 8 | Intentos de colocación antes de abortar |
| `clearanceRadius` | 0.5 | Radio libre que necesita cada spawn |
| `obstacleMask` | ~0 | Marcá **`Walls`** (y `Enemies` si no querés solapamientos) |

### 6.2 Objeto `World`

1. Creá un GameObject vacío llamado `World`.
2. Añadí **`MapGenerator`** y **`RoomContentSpawner`**.

**`MapGenerator`**

| Campo | Qué poner |
|---|---|
| `config` | Se sobreescribe desde `RunManager`; podés dejarlo vacío |
| `targetTilemap` | Arrastrá `Tilemap_Walls` |
| `contentSpawner` | Arrastrá el mismo GameObject `World` (tiene el `RoomContentSpawner`) |

**`RoomContentSpawner`**

| Campo | Qué poner |
|---|---|
| `mapGenerator` | Arrastrá el mismo GameObject `World` |
| `spawner` | Arrastrá el GameObject `Spawner` |

> **Probar sin jugar:** `MapGenerator` tiene `[ContextMenu("Generate")]`, así que desde el
> menú de tres puntos del componente podés hacer clic en **Generate** y ver el mapa armado
> sin entrar en Play. Necesita `config`, `bank` y `targetTilemap` asignados.

### 6.3 Cómo se genera (para entender qué estás configurando)

1. `RunManager` carga el `WorldConfigSO` del mundo 0 → `MapGenerator.Configure()` + `Generate()`.
2. `Generate()` limpia el tilemap y las listas, y crea `System.Random(seed)`.
3. **`RoomLayout.Build`**: reserva el cuarto de **Spawn** (centro) y el de **Boss** (el más lejano),
   empaqueta el resto por **clase de tamaño** con pesos, asigna **tag** por cuarto y calcula la
   adyacencia. El grafo es **totalmente conexo**.
4. **`StampRooms`**: estampa cada arquetipo en su celda de origen (sin rotación).
5. **`CarveConnections`**: por cada celda compartida limpia un paso en el borde (`openingWidth`).
6. **`SpawnContent`** → `RoomContentSpawner.SpawnAll()`: por cuarto resuelve el `ZoneThemeSO` de su
   `tag` y coloca **barreras** (validadas para no sellar el cuarto), **props** y **enemigos**.

> **El tamaño importa:** los `MapChunkSO` deben medir `sizeInCells × chunkSize` en tiles
> (p. ej. un 2×1 son 32×16 tiles con `chunkSize = 16`), si no los cuartos se solapan o quedan con huecos.

---

## Fase 7 — Jugador

### 7.1 Estructura del GameObject `Player`

1. `GameObject > 2D Object > Sprites > Capsule` (o tu sprite) → renombralo `Player`.
2. **Tag** = `Player`. **Layer** = `Player`.
3. Añadí **`Rigidbody 2D`**: `Gravity Scale = 0`, `Collision Detection = Continuous`,
   `Interpolate = Interpolate`, `Freeze Rotation Z` ✅.
4. Añadí un **`Circle Collider 2D`** (o `Capsule`) del tamaño del sprite.

### 7.2 Componentes

| Componente | Campos a configurar |
|---|---|
| **`PlayerController`** | `moveSpeed` = 5. Usa los ejes `Horizontal` / `Vertical` (WASD + flechas por defecto) |
| **`Health`** | `maxHealth` = 6 (poné la vida en "corazones") |
| **`PlayerCombat`** | ver 7.3 |
| **`PlayerAnimationController`** | `animator`, `controller`, `combat`, `health` (si los dejás vacíos se auto-resuelven con `GetComponent`) |
| **`Animator`** | Controller de la Fase 2 |

### 7.3 `PlayerCombat`

Creá dos GameObjects vacíos hijos del Player y usalos como puntos de origen:

- `MeleePoint` → a ~1 tile delante del jugador.
- `FirePoint` → en el centro del jugador.

| Campo | Default | Qué poner |
|---|---|---|
| `meleeDamage` | 10 | Daño del ataque melee |
| `meleeRange` | 1.5 | Radio del `OverlapCircle` del melee |
| `meleeCooldown` | 0.5 | Segundos entre melees |
| `meleeAttackDuration` | 0.3 | Cuánto dura la animación de melee |
| `meleeKey` | `Space` | Tecla de melee |
| `meleePoint` | — | Arrastrá `MeleePoint` |
| `rangedDamage` | 5 | Daño del proyectil |
| `rangedCooldown` | 0.8 | Segundos entre disparos |
| `rangedAttackDuration` | 0.3 | Duración de la animación de disparo |
| `rangedKey` | `K` | Tecla de ataque a distancia |
| `projectilePrefab` | — | Prefab del proyectil del jugador (Fase 8) |
| `projectileSpeed` | 8 | Velocidad del proyectil |
| `firePoint` | — | Arrastrá `FirePoint` |
| `enemyMask` | ~0 | Marcá **`Enemies`** |

> **Comportamiento:**
> - **Melee (`Space`)**: `Physics2D.OverlapCircleAll(meleePoint, meleeRange, enemyMask)` y
>   aplica `meleeDamage` a cada `IDamageable` encontrado.
> - **Ranged (`K`)**: instancia el proyectil hacia **la posición del mouse**
>   (`Camera.main.ScreenToWorldPoint`).
>
> **Ataque del jugador ≠ ataque del jefe:** el jugador usa `Space` y `K` fijos. Los jefes
> tienen sus propias mecánicas (Fase 10).

---

## Fase 8 — Proyectiles

El mismo script `Projectile` sirve para jugador, enemigos y jefes; lo que cambia es el prefab,
la layer y el `targetMask`.

1. Creá un `GameObject > 2D Object > Sprites > Circle` pequeño (una bolita) → `Projectile_Player`.
2. **Layer** = `PlayerAttack` (para el jugador) o `EnemyAttack` (para enemigos/jefes).
3. Añadí **`Rigidbody 2D`**: `Gravity Scale = 0`.
4. Añadí un **`Circle Collider 2D`** con **`Is Trigger`** ✅.
5. Añadí el componente **`Projectile`**.

| Campo | Default | Qué poner |
|---|---|---|
| `lifetime` | 5 | Segundos antes de autodestruirse |

> **Nada más se configura acá.** `Projectile` recibe daño, velocidad y `targetMask` por código
> desde quien lo dispara (`PlayerCombat`, `EnemyAttack` o `PeterPanBoss`).
>
> **Requisito de la matriz de colisión:** el proyectil solo debe chocar contra su objetivo.
> Para el del jugador, `PlayerAttack × Enemies`. Para el de enemigos, `EnemyAttack × Player`.

---

## Fase 9 — Enemigos

### 9.1 Estructura del prefab

1. Creá el sprite → `Enemy_Wolf` (o el que sea).
2. **Layer** = `Enemies`.
3. Añadí **`Rigidbody 2D`**: `Gravity Scale = 0`, `Freeze Rotation Z` ✅.
4. Añadí un **`Circle Collider 2D`**.
5. Añadí **`Health`**, **`EnemyMovement`**, **`EnemyAttack`**, **`EnemyFOV`**, **`EnemyAI`**
   y un **`Animator`** con el controller de la Fase 2.

> `EnemyAI` tiene `[RequireComponent]` de `EnemyMovement`, `EnemyAttack`, `EnemyFOV` y `Health`:
> si te falta alguno, Unity te lo agrega solo.

### 9.2 Configurar `EnemyAI`

| Campo | Default | Qué poner |
|---|---|---|
| `stats` | — | Su `EnemyStatsSO` |
| `animator` | — | El `Animator` del prefab |
| `stateParameter` | `State` | Nombre del parámetro int |
| `fovCheckInterval` | 0.1 | Cada cuánto recalcula si te ve (rendimiento) |
| `alertTime` | 0.3 | Pausa de "te vi" antes de perseguir |
| `lostPlayerTimeout` | 3 | Segundos fuera de rango antes de volver a casa |

> **La vida del prefab la pisa el código:** `EnemyAI.ApplyStats()` llama a
> `health.SetMaxHealth(stats.MaxHealth)`. O sea, el `maxHealth` que manda es el del
> `EnemyStatsSO`, no el del componente `Health` del prefab.

### 9.3 Configurar `EnemyAttack` y `EnemyFOV`

**`EnemyAttack`**

| Campo | Qué poner |
|---|---|
| `firePoint` | Un hijo vacío en la boca/arma del enemigo (origen del proyectil) |
| `targetMask` | Marcá **`Player`** |

**`EnemyFOV`**

| Campo | Qué poner |
|---|---|
| `obstacleMask` | Marcá **`Walls`** (para que los muros corten la línea de visión) |
| `eyes` | Un hijo vacío a la altura de los ojos (opcional; si está vacío usa `transform.position`) |

> ⚠ **La visión usa `transform.up` como "adelante".** Si tu enemigo debe mirar hacia el
> jugador para verte, rotá el sprite: el cono de visión (`fovAngle`) se calcula contra el eje
> **Y verde** del objeto. Para enemigos que ven en todas direcciones, poné `fovAngle = 360`.

### 9.4 Enemigos melee vs ranged

Solo cambia el `EnemyStatsSO`:

- **Melee**: `attackType = Melee`, `projectilePrefab` vacío. Golpea en `attackRange` con
  `windupTime` de anticipación.
- **Ranged**: `attackType = Ranged` + `projectilePrefab` asignado + `projectileSpeed`.
  Instancia el proyectil y lo lanza hacia el jugador.

### 9.5 Crear los `EnemyArchetypeSO`

Uno por enemigo: `prefab` + `stats` + `tier` + `weight`. Después van en el `enemyBank` de cada `ZoneThemeSO`.

### 9.6 Enemigos del demo (valores sugeridos)

**Bosque de Caperucita**

| Enemigo | Tipo | Vida | Vel. persecución | Daño | Rango | Detección |
|---|---|---|---|---|---|---|
| Lobo de la guardia | Melee | 30 | 4.5 | 8 | 1.4 | 9 |
| Zarza viviente | Melee | 70 | 1.5 | 12 | 1.6 | 5 |
| Leñador poseído | Melee | 45 | 2.8 | 16 | 2.2 | 7 |
| Luciérnaga maldita | Ranged | 25 | 2.5 | 6 | 6 | 7 |

**Mar de Nunca Jamás**

| Enemigo | Tipo | Vida | Vel. persecución | Daño | Rango | Detección |
|---|---|---|---|---|---|---|
| Grumete / niño perdido | Melee | 35 | 3.5 | 10 | 1.5 | 8 |
| Sirena | Ranged | 30 | 2.8 | 7 | 7 | 10 |
| Cocodrilo Tic-Tac | Melee | 80 | 5 | 14 | 1.8 | 9 |
| Hada de polvo | Ranged | 20 | 5.5 | 5 | 5 | 8 |

---

## Fase 10 — Jefes: Caperucita y Peter Pan

> **Regla de oro:** Caperucita y Peter Pan **no comparten IA ni movimiento**. Cada uno tiene
> su propio script con su propia máquina de estados. **No les pongas `EnemyAI` ni `BossAI`**:
> serían dos controladores peleando por el mismo `Rigidbody2D`.
>
> Lo único que comparten es: el `BossStatsSO` (datos), el esquema de animación (Fase 2) y la
> interfaz `IBossController` (contrato para que la arena los spawnee sin saber cuál es cuál).

### 10.1 Armar la arena en el chunk del jefe

En el `MapChunkSO` con `role = Boss` (o en una copia en escena), dentro de la escena:

1. En el room de la arena, creá un GameObject `BossArena`.
2. Añadí un **`Box Collider 2D`** con **`Is Trigger`** ✅ que cubra la entrada/piso de la arena.
   Ponele la **layer `BossTrigger`**.
3. Añadí el componente **`BossArenaController`**.
4. Creá un hijo vacío `BossSpawnPoint` en el centro de la arena → añadile **`BossSpawnPoint`**
   (dibuja un gizmo rojo; sirve para ver dónde aparece el jefe).
5. Creá los **colliders de las puertas de salida**: GameObjects con `Box Collider 2D` (no
   trigger) en cada salida, en la layer `Walls`. **Dejalos deshabilitados por defecto**.

**`BossArenaController`**

| Campo | Qué poner |
|---|---|
| `exitBlockers` | Arrastrá los colliders de las puertas (empezando **deshabilitados**) |
| `bossSpawnPoint` | El `BossSpawnPoint` de la arena |
| `bossPrefab` / `bossStats` | Se sobreescriben desde el `WorldConfigSO`; se pueden dejar vacíos |

> **Cómo funciona:** cuando el jugador entra al trigger (`OnTriggerEnter2D` + `CompareTag("Player")`),
> el script **activa** los `exitBlockers` (te encierra), instancia el jefe en el spawn point,
> le llama `ConfigureBoss(stats, player)` vía `IBossController` y se suscribe a su `Health.Died`.
> Al morir: apaga los blockers, dispara `BossDefeated` y avisa al `BossManager`.

### 10.2 Prefab de Caperucita (melee puro)

1. Sprite de Caperucita → nombre `Boss_Caperucita`.
2. **Layer** = `Enemies`.
3. **`Rigidbody 2D`**: `Gravity Scale = 0`, `Freeze Rotation Z` ✅.
4. **`Circle Collider 2D`**.
5. **`Health`** (la vida real la pone el `BossStatsSO`).
6. **`CaperucitaBoss`** ← el único controlador.
7. **`Animator`** con el controller de la Fase 2.
8. Hijo vacío `SlashPoint` a ~1 tile delante (origen del corte).

**Campos de `CaperucitaBoss`**

| Campo | Default | Qué controla |
|---|---|---|
| `stats` | — | Su `BossStatsSO` |
| `slashPoint` | — | Origen del `OverlapCircle` del corte |
| `playerMask` | ~0 | Marcá **`Player`** |
| `lungeSpeed` | 14 | Velocidad de la embestida |
| `lungeDuration` | 0.22 | Duración de la embestida (segundos) |
| `lungeCooldown` | 3.5 | Segundos entre embestidas |
| `lungeDamageMultiplier` | 1.5 | Daño de la embestida = `damage × esto` |
| `lungeHitRadius` | 1 | Radio de contacto durante la embestida |
| `lungeMinDistance` | 2.5 | No embiste si estás más cerca que esto |
| `lungeMaxDistance` | 7 | No embiste si estás más lejos que esto |
| `recoverTime` | 0.35 | Pausa después de atacar |
| `summonWindup` | 0.7 | Anticipación antes de invocar (el "aullido") |
| `minionPrefabs` | vacío | Prefabs de los lobos |
| `minionsPerSummon` | 2 | Lobos por invocación |
| `extraWolvesOnEnrage` | 2 | Lobos extra en la fase 3 |
| `minionSpread` | 2.5 | Radio alrededor del jefe donde aparecen |
| `enrageSpeedMultiplier` | 1.35 | Multiplicador de velocidad en fase 3 |
| `enrageCooldownMultiplier` | 0.6 | Multiplicador de cooldown de ataque en fase 3 |
| `enrageLungeCooldownMultiplier` | 0.5 | Multiplicador de cooldown de embestida en fase 3 |
| `lootPrefab` | — | `[LOTE 2]` Objeto que suelta al morir |
| `animator` | — | El `Animator` |
| `stateParameter` | `State` | Nombre del parámetro int |

**Comportamiento (FSM propia):**

| Estado | Qué hace |
|---|---|
| `Stalk` | Persigue al jugador; si está en rango y sin cooldown, pasa a `Windup`; si hay distancia de embestida, pasa a `Lunge` |
| `Windup` | Se queda quieto `windupTime` (telegraph) |
| `Slash` | Aplica `damage` en `attackRange` alrededor de `slashPoint` y pasa a `Recover` |
| `Lunge` | Se lanza en línea recta `lungeDuration`; **un solo golpe** a `damage × lungeDamageMultiplier`, luego `Recover` |
| `Recover` | Quieto `recoverTime`, vuelve a `Stalk` |
| `Summon` | Quieto `summonWindup` y luego instancia los lobos |
| `Dead` | Deshabilita su collider, suelta el `lootPrefab` y pone la animación `Death` |

**Fases:** 1 → solo melee (corte + embestida). 2 → invoca lobos. 3 → enrage (más rápido,
menos cooldown, más lobos).

### 10.3 Prefab de Peter Pan (ranged puro, nunca melee)

Mismo armado que Caperucita, pero con **`PeterPanBoss`** y un hijo `FirePoint` en el centro.
**No lleva `SlashPoint`.**

**Campos de `PeterPanBoss`**

| Campo | Default | Qué controla |
|---|---|---|
| `stats` | — | Su `BossStatsSO` |
| `projectilePrefab` | — | Prefab de la daga (si está vacío usa `stats.projectilePrefab`) |
| `firePoint` | — | Origen de las dagas |
| `playerMask` | ~0 | Marcá **`Player`** |
| `daggersPerPhase` | `[1, 3, 5]` | Dagas por abanico según la fase |
| `spreadAngle` | 30 | Apertura total del abanico, en grados |
| `preferredDistance` | 6 | Distancia que intenta mantener |
| `closeRange` | 3 | Si te acercás más que esto, hace `Reposition` |
| `strafeSpeed` | 3 | Velocidad al orbitar |
| `orbitFlipInterval` | 1.2 | Cada cuánto invierte el sentido de la órbita |
| `dashSpeed` | 16 | Velocidad del dash de reposición (el "vuelo") |
| `dashDuration` | 0.18 | Duración del dash |
| `dashCooldown` | 4 | Segundos entre dashes |
| `recoverTime` | 0.4 | Pausa después del abanico |
| `summonWindup` | 0.7 | Anticipación antes de invocar |
| `minionPrefabs` | vacío | Perdidos / hadas |
| `minionsPerSummon` | 2 | Esbirros por invocación |
| `extraMinionsOnEnrage` | 2 | Esbirros extra en fase 3 |
| `minionSpread` | 2.5 | Radio de aparición |
| `enrageSpeedMultiplier` | 1.25 | Velocidad en fase 3 |
| `enrageCooldownMultiplier` | 0.65 | Cooldown del abanico en fase 3 |
| `enrageDashCooldownMultiplier` | 0.55 | Cooldown del dash en fase 3 |
| `enrageSpreadMultiplier` | 1.35 | Apertura del abanico en fase 3 |
| `lootPrefab` | — | `[LOTE 2]` Objeto que suelta al morir |
| `animator` / `stateParameter` | — / `State` | Igual que Caperucita |

**Comportamiento (FSM propia, distinta a la de Caperucita):**

| Estado | Qué hace |
|---|---|
| `Kite` | Si estás muy cerca → `Reposition`. Si estás lejos → se acerca. Si estás en rango → **orbita** en tangente. Si estás dentro de `attackRange` y sin cooldown → `VolleyWindup` |
| `VolleyWindup` | Quieto `windupTime`, luego `FireVolley` y pasa a `Recover` |
| `Reposition` | Dash en línea recta `dashDuration` a `dashSpeed`, luego vuelve a `Kite` |
| `Recover` | Quieto `recoverTime`, vuelve a `Kite` |
| `Summon` | Quieto `summonWindup` y luego instancia esbirros |
| `Dead` | Igual que Caperucita |

> **El "vuelo"**: al ser un top-down no hay eje de altura real. La sensación de flotar se logra
> con `chaseSpeed` alto + `Reposition` frecuente + el sprite flotando.

### 10.4 Stats sugeridos para los dos jefes

**Caperucita (Bosque)** — `BossStatsSO`

| Campo | Valor | Por qué |
|---|---|---|
| `maxHealth` | 300 | Pelea larga con 3 fases |
| `moveSpeed` | 2 | Lenta de lejos |
| `chaseSpeed` | 3.5 | Cazadora |
| `damage` | 12 | Corte; la embestida pega 18 |
| `attackRange` | 1.8 | Melee corto |
| `attackCooldown` | 1.2 | Marca el ritmo |
| `windupTime` | 0.35 | Telegrafía clara |
| `attackType` | `Melee` | — |
| `detectionRadius` | 20 | Pelea de arena: que no se escape |
| `fovRadius` | 20 | Te ve de lejos |
| `fovAngle` | 360 | Te ve en todas direcciones |
| `hearingRadius` | 20 | — |
| `phaseHealthThresholds` | `[0.66, 0.33]` | 3 fases |

**Peter Pan (Mar)** — `BossStatsSO`

| Campo | Valor | Por qué |
|---|---|---|
| `maxHealth` | 220 | Más frágil pero escurridizo |
| `moveSpeed` | 4 | Se reposiciona |
| `chaseSpeed` | 5 | "Vuela" |
| `damage` | 8 | Muchas dagas, poco daño cada una |
| `attackRange` | 9 | Ranged largo |
| `attackCooldown` | 1.6 | Ritmo del abanico |
| `windupTime` | 0.45 | Telegrafía antes del abanico |
| `attackType` | `Ranged` | — |
| `projectilePrefab` | `Projectile_Dagger` | Ver Fase 8, layer `EnemyAttack` |
| `projectileSpeed` | 12 | Dagas rápidas |
| `detectionRadius` | 20 | Pelea de arena |
| `fovRadius` | 20 | Te ve de lejos |
| `fovAngle` | 360 | Te ve en todas direcciones |
| `phaseHealthThresholds` | `[0.66, 0.33]` | 3 fases |

### 10.5 Conectar los jefes al flujo de mundos

En la escena, creá un GameObject `GameManager` con:

1. **`BossManager`**

| Campo | Qué poner |
|---|---|
| `config` | Se sobreescribe desde `RunManager`; se puede dejar vacío |
| `arena` | Arrastrá el GameObject `BossArena` (el de `BossArenaController`) |

2. **`RunManager`**

| Campo | Qué poner |
|---|---|
| `worlds` | Lista **ordenada**: `[World0_BosqueCaperucita, World1_MarPeterPan]` |
| `mapGenerator` | El GameObject `World` |
| `bossManager` | El GameObject `GameManager` |

3. Cada **`WorldConfigSO`** debe apuntar a su `bossPrefab` y su `boss`:

| Mundo | `bossPrefab` | `boss` |
|---|---|---|
| Bosque | `Boss_Caperucita` | `CaperucitaStats` |
| Mar | `Boss_PeterPan` | `PeterPanStats` |

> **Flujo:** `RunManager.Start()` → `GenerateWorld(0)` → genera el mapa y llama a
> `bossManager.SetupBossFight()` → la arena queda lista. Al morir el jefe:
> `BossArenaController` → `BossManager` → `RunManager.OnBossDefeated()` →
> `GenerateWorld(1)` con el siguiente mundo.

---

## Fase 11 — `[LOTE 2]` Objetos y habilidades

> Esta fase **todavía no está implementada en el código**. Se incluye acá para que el manual
> quede completo cuando se implemente el Lote 2.

### 11.1 Crear los 4 `BossAbilitySO`

`Create > Madness Dreams > Boss Ability`. Campos:

| Bloque | Campo | Qué controla |
|---|---|---|
| Identidad | `type` | `WolfClaw`, `ThornShot`, `PeterDagger` o `FairyDust` |
| | `displayName` / `description` / `icon` | UI |
| Tiempos | `cooldown` | Segundos entre usos |
| | `animationDuration` | Duración de la animación |
| Daño | `damage` / `range` | Daño y radio (melee) |
| Proyectil | `projectilePrefab` / `projectileSpeed` | Proyectil |
| | `projectileCount` / `spreadAngle` | Abanico |
| Ralentización | `slowMultiplier` / `slowDuration` | Solo `ThornShot` |
| Dash | `dashDistance` / `dashDuration` | Solo `WolfClaw` y `FairyDust` |

**Assets a crear (recompensa de cada jefe):**

| Asset | `type` | Origen | Efecto |
|---|---|---|---|
| `Ability_WolfClaw` | `WolfClaw` | Caperucita | Embestida corta + golpe melee con más daño y alcance |
| `Ability_ThornShot` | `ThornShot` | Caperucita | Proyectil de espinas que **ralentiza** al enemigo |
| `Ability_PeterDagger` | `PeterDagger` | Peter Pan | Abanico de dagas rápidas |
| `Ability_FairyDust` | `FairyDust` | Peter Pan | Dash de evasión (polvo de hada) |

### 11.2 Prefab `AbilityPickup`

1. Sprite del objeto → `Pickup_WolfClaw`, etc.
2. **Layer** = `Pickups`.
3. **`Circle Collider 2D`** con **`Is Trigger`** ✅.
4. Componente **`AbilityPickup`**.

| Campo | Qué poner |
|---|---|
| `ability` | El `BossAbilitySO` que otorga |
| `destroyOnPickup` | ✅ |
| `bobAmplitude` / `bobSpeed` | Flotación visual |

> El pickup solo se destruye si `PlayerAbilities.Learn()` devolvió `true`: si ya tenías esa
> habilidad, el objeto queda en el piso.

### 11.3 Asignar el drop a cada jefe

En el prefab del jefe, campo **`lootPrefab`**:

| Jefe | `lootPrefab` |
|---|---|
| `Boss_Caperucita` | `Pickup_WolfClaw` (o `Pickup_ThornShot` si querés la variante ranged) |
| `Boss_PeterPan` | `Pickup_PeterDagger` (o `Pickup_FairyDust`) |

### 11.4 Añadir `PlayerAbilities` al jugador

En el GameObject `Player`, añadí el componente **`PlayerAbilities`**:

| Campo | Qué poner |
|---|---|
| `startingAbilities` | Dejalo vacío para probar el flujo de recogida (o cargá una para testear) |
| `maxAbilities` | 4 |
| `abilityKeys` | `Alpha1`, `Alpha2`, `Alpha3`, `Alpha4` |
| `enemyMask` | Marcá **`Enemies`** |
| `castPoint` / `meleePoint` | Hijos vacíos del Player |

**Teclas:** `1`…`4` ejecutan la habilidad aprendida en ese slot, en el orden en que las
recogiste. Cada slot tiene su propio cooldown.

> Las habilidades las ejecuta el jugador apuntando **hacia el mouse**, igual que el ataque
> a distancia con `K`.

---

## Fase 12 — Flujo de mundos y cierre de partida

1. En `RunManager.worlds`, cargá los dos mundos **en orden**.
2. Al morir el jefe del último mundo, `RunManager` loguea `"All worlds cleared. Victory!"`.
   Ahí enganchás el final del demo.
3. **[LOTE 3]** `delayBeforeNextWorld` (default `0`): segundos de espera antes de pasar al
   siguiente mundo, para que al jugador le dé tiempo de recoger el drop del jefe.
   **Con `0` el mapa cambia de inmediato y el drop se pierde.**
4. ⚠ **La semilla no cambia sola.** Cada `WorldConfigSO` tiene su `seed` fijo: el mapa del
   Bosque es siempre el mismo. Si querés que cambie entre partidas, usá semillas distintas
   por mundo (o aleatorizá el `seed` del `WorldConfigSO` antes de generar).

---

## Fase 13 — Checklist de prueba end-to-end

Antes de dar el demo por bueno, corré esto **en Play Mode**:

**Generación**
- [ ] Al dar Play, aparece un mapa completo con rooms y muros sólidos.
- [ ] El jugador colisiona con los muros y no los atraviesa.
- [ ] El room del jefe es visible, distinto y está en la zona más lejana al spawn.
- [ ] Los enemigos aparecen en los rooms y **no** atraviesan muros.
- [ ] Cambiando el `seed` del `WorldConfigSO` y volviendo a generar, el mapa cambia.

**Enemigos**
- [ ] Un enemigo te ve, se alerta ~0.3 s y te persigue.
- [ ] Los muros **cortan** la línea de visión (no te ve a través de ellos).
- [ ] El enemigo melee se detiene en `attackRange`, telegrafía y te pega.
- [ ] El enemigo ranged te dispara y su proyectil te hace daño.
- [ ] Un proyectil de enemigo **no** daña a otros enemigos.
- [ ] El proyectil del jugador (`K`) daña al enemigo al que apuntás.
- [ ] El melee del jugador (`Space`) daña a los enemigos en el radio.
- [ ] Al morir un enemigo, su animación cambia a `Death`.
- [ ] El HUD/consola muestra `HealthChanged` bajando.

**Arena y Caperucita**
- [ ] Al entrar a la arena, las puertas **se bloquean** y aparece Caperucita.
- [ ] Caperucita te persigue y te corta de cerca.
- [ ] Caperucita **embiste** de vez en cuando (y no pega dos veces en la misma embestida).
- [ ] Al bajar del 66% aparece la animación/delay de invocación y aparecen los lobos.
- [ ] Al bajar del 33% se la nota más rápida y con más lobos (enrage).
- [ ] Al morir: suelta el `lootPrefab`, se desbloquean las puertas y `RunManager` pasa al mundo 2.

**Peter Pan**
- [ ] Aparece en el Mar y **mantiene la distancia**: si te acercás, se aleja.
- [ ] Lanza **abanicos** de dagas (varias en distintas direcciones).
- [ ] Cuando lo acorralás, hace el **dash** de reposición (no se queda pegado).
- [ ] Al 66% invoca esbirros; al 33% el abanico se abre más y dash más seguido.
- [ ] **Nunca** hace un ataque melee.

**Habilidades `[LOTE 2]`**
- [ ] Al morir Caperucita, el pickup aparece en el piso.
- [ ] Si lo recogés, la tecla `1` ejecuta la habilidad de Caperucita.
- [ ] `ThornShot` **ralentiza** al enemigo alcanzado.
- [ ] `FairyDust` te da un dash y no te deja atrapado en muros.
- [ ] Si ya tenías la habilidad, el pickup **no** se consume.

**Independencia de jefes (requisito del proyecto)**
- [ ] Caperucita **nunca** dispara proyectiles.
- [ ] Peter Pan **nunca** ataca melee.
- [ ] Ninguno de los dos usa `EnemyAI` como controlador.

---

## Fase 14 — Problemas comunes

| Síntoma | Causa probable | Solución |
|---|---|---|
| El mapa sale vacío | `MapGenerator` sin `targetTilemap`, banco sin `normalChunks`, o falta el cuarto de Spawn/Boss | Revisá los tres y corré `Validate Worlds` |
| NullReference al dar Play | `RunManager` sin `mapGenerator` o sin `bossManager`, o `BossArenaController` sin `arena` | Completá las referencias de la Fase 10.5 |
| `"RunManager: no worlds configured"` | Lista `worlds` vacía | Cargá los `WorldConfigSO` |
| Los cuartos se solapan o quedan con huecos | El `MapChunkSO` no mide `sizeInCells × chunkSize` en tiles | Revisá `sizeInCells` y `chunkSize` |
| Los enemigos no aparecen | El cuarto no tiene `enemySpawns`, el `ZoneThemeSO` no tiene `enemyBank`, o el **tier** permitido por el nivel es 0 | Revisá la Fase 4 y el `ZoneThemeSO`/`DifficultyProfileSO` |
| No aparecen props ni barreras | El `ZoneThemeSO` del tag del cuarto no tiene `propBank`/`barrierBank`, o el mundo no tiene `themes` cableados | Cableá `WorldConfigSO.themes` |
| Las barreras no aparecen | La validación anti-sellado descarta las que bloquearían el cuarto | Normal: probá con footprints más chicos |
| Los enemigos no te ven | `fovAngle` angosto y el sprite no mira hacia el jugador (la visión usa `transform.up`) | Poné `fovAngle = 360`, o rotá el sprite |
| Los proyectiles atraviesan o no dañan | Layer/matriz de colisión mal, o `targetMask` sin el layer correcto | Revisá Fase 1.3 y el `targetMask`/`enemyMask` |
| Los disparos del jugador salen en dirección rara | No hay `Camera.main` (falta el tag `MainCamera`) | Etiquetá la cámara |
| El jefe no aparece al entrar a la arena | Trigger sin layer `BossTrigger`, sin `Is Trigger`, o el jugador sin tag `Player`, o `bossPrefab` vacío en el `WorldConfigSO` | Revisá la Fase 10.1 y 10.5 |
| El jefe no se mueve ni ataca | Falta el `BossStatsSO` en el `WorldConfigSO.boss` (el script avisa con un warning) | Asignalo |
| El jefe tiene la vida del prefab y no la del SO | Es lo esperado: `ConfigureBoss` → `SetMaxHealth(stats.MaxHealth)` pisa el prefab | Editá el `BossStatsSO` |
| El jefe nunca cambia de fase | `phaseHealthThresholds` mal ordenados (deben ser **ascendentes**) o `maxHealth` en 0 | Revisá el `BossStatsSO` |
| Caperucita se daña a sí misma | `playerMask` incluye la layer `Enemies` | Marcá solo `Player` |
| El jefe queda atrapado en las puertas | Los `exitBlockers` están **habilitados desde el arranque** | Dejalos deshabilitados por defecto |
| El jefe se queda pegado al jugador sin pelear | Pusiste `EnemyAI` o `BossAI` **además** de `CaperucitaBoss`/`PeterPanBoss` | Dejá **un solo** controlador |
| Al ganar el mundo 2 el juego salta de mundo | `BossManager.SetupBossFight()` se vuelve a suscribir a `BossDefeated` en cada `SetupBossFight()`, así que con **3 o más** mundos el evento se dispara varias veces | Limitación conocida del código actual; con los 2 mundos del demo no afecta |
| El drop del jefe desaparece apenas muere | `RunManager` pasa de mundo de inmediato | Subí `delayBeforeNextWorld` `[LOTE 3]`, o colocá el pickup en el piso de la arena |
| El jugador muerto sigue caminando | No hay lógica de muerte del jugador implementada | Fuera del alcance del demo; solo cambia la animación a `Death` |

---

## Apéndice A — Orden de ejecución (qué pasa primero)

```
RunManager.Start()
   └─ GenerateWorld(0)
        ├─ mapGenerator.Configure(world) + mapGenerator.Generate()
        │     ├─ targetTilemap.ClearAllTiles()
        │     ├─ RoomLayout.Build()     → Spawn + Boss reservados + empaquetado + adyacencia
        │     ├─ StampRooms()          → TilemapStamper.Stamp()
        │     ├─ CarveConnections()    → un paso por celda compartida
        │     └─ RoomContentSpawner.SpawnAll() → barreras/props/enemigos → Spawner.TrySpawn()
        └─ bossManager.Configure(world) + SetupBossFight() → arena.Setup(prefab, stats)

[el jugador explora y pelea]

BossArenaController.OnTriggerEnter2D(tag "Player")
   ├─ SetExitsBlocked(true)
   ├─ Instantiate(bossPrefab)
   ├─ boss.GetComponent<IBossController>().ConfigureBoss(stats, player)
   └─ bossHealth.Died += OnBossDefeated

[pelea del jefe, con sus fases]

Health.Died → BossArenaController.OnBossDefeated()
   ├─ SetExitsBlocked(false)
   └─ BossDefeated → BossManager → RunManager.OnBossDefeated()
        └─ GenerateWorld(index + 1)   [o "Victory!" si era el último]
```

## Apéndice B — Resumen de teclas del jugador

| Tecla | Acción |
|---|---|
| `WASD` / flechas | Mover |
| `Space` | Ataque melee |
| `K` | Ataque a distancia (hacia el mouse) |
| `1`…`4` | `[LOTE 2]` Habilidades aprendidas de los jefes |

## Apéndice C — Referencia rápida de enums

**`AnimationState`** (parámetro int `State` del Animator):

| Valor | Estado |
|---|---|
| 0 | `Idle` |
| 1 | `Move` |
| 2 | `MeleeAttack` |
| 3 | `RangedAttack` |
| 4 | `Death` |

**`AttackType`**: `Melee = 0`, `Ranged = 1`.

**`CellRole`**: `Empty = 0`, `Start = 1`, `Normal = 2`, `Boss = 3`.
