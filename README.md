# Madness Dreams — Proyecto Unity

Roguelite 2D top-down (estilo *Binding of Isaac* / *Cult of the Lamb*): mundos de cuentos
distorsionados, generación procedural por semilla, jefes por fases y progresión persistente.

> **Unity 6.0.3 LTS** (`6000.3.19f1`) · URP 2D · **Input System** (con fallback legacy) · **Cinemachine 3.1.7**.
> Todo el arte actual es **greybox** (primitivas coloreadas): la lógica está completa y el arte final se
> reemplaza sin tocar código.

## Cómo ejecutar

1. Abrir el proyecto con Unity **6000.3.19f1**.
2. Abrir la escena `Assets/Scenes/01_MainMenu.unity` y dar **Play** → **Jugar**.
3. Para pruebas rápidas: abrir `Assets/Scenes/Greybox.unity` y dar **Play** (usar el botón **DEV** del HUD).

Todas las escenas están en **Build Settings** (MainMenu = índice 0).

## Escenas

| Escena | Rol |
|---|---|
| `01_MainMenu` | Menú principal (primera del build) |
| `02_Loading` | Pantalla de carga |
| `03_Gameplay` | Nivel jugable (mapa procedural + HUD) |
| `04_Defeat` | Derrota (Reintentar / Menú / Salir) |
| `05_Settings` | Ajustes |
| `Greybox` | Escena de pruebas (mapa manual + jefe) |

## Controles

| Tecla | Acción |
|---|---|
| `WASD` / flechas | Mover |
| `Space` | Ataque melee |
| `K` | Ataque a distancia (hacia el mouse) |
| `1`–`4` | Habilidades de jefe |
| `Esc` | Pausa (Reanudar / Ajustes / Menú) |
| `M` / clic en el minimapa | Mapa grande |

## Estructura

```
Assets/
├── Art/Greybox/        Sprites placeholders (Square, Circle)
├── Animations/         Animator Controllers (Player / Enemy) — estado por color
├── Prefabs/            Player, enemigos, jefe, proyectiles, pickups, props, HUD
├── Scenes/             01_..05_ + Greybox
├── ScriptableObjects/  Datos (mundos, rooms, temas, dificultad, stats, habilidades)
├── Scripts/            Código (ver Assets/Scripts/README.md)
└── Tiles/              Tile_Wall
```

## Configuración del proyecto

- **Layers** (6–14): `Ground, Walls, Player, Enemies, PlayerAttack, EnemyAttack, Pickups, Hazards, BossTrigger`. Tag `Player`.
- **Matriz 2D:** los proyectiles sólo colisionan con su objetivo; `Enemies × EnemyAttack`, `PlayerAttack × Player` y `Walls × (PlayerAttack/EnemyAttack)` desactivados.
- **Input Handling = Both**; el jugador usa el nuevo Input System con fallback legacy.
- **Cinemachine:** `CinemachineBrain` en la Main Camera + `CM Camera` (`CinemachineFollow`).

## Documentación

- `Assets/Scripts/README.md` — arquitectura del código y una README por carpeta.
- `Documentacion/` — Game Design Document, manual de montaje, flujo de componentes y
  `Estado_Implementacion_Unity.md` (estado real de la implementación).

## Packs de props temáticos (`Assets/Art/Props/Packs/`)

Generados desde `Downloads/Props` con `Tools > Madness Dreams > Props > Build Prop Packs`
(`PropPackBuilder`). Cada tema tiene prefabs y `PropDefinitionSO` divididos en **Props**
(decorativos: arbustos, rocas, hongos) y **Barriers** (árboles y estructuras que bloquean el paso).

| Tema | Tag | Contenido |
|---|---|---|
| Forest | `RoomTag.Forest` | Árboles clásicos, moss, flores, frutales, arbustos, helechos, hongos, ents, rocas de pasto |
| Sea | `RoomTag.Sea` | Palmas, sauces, árboles de esferas de luz, árboles blancos, rocas de agua (`allowWater`) |
| Cave | `RoomTag.Cave` | Árboles luminosos, árboles retorcidos, rocas oscuras |
| Ruins | `RoomTag.Ruins` | Árboles rotos y quemados, ídolos de árbol, escombros, rocas |
| Winter | `RoomTag.Snow` | Árboles de nieve, pinos navideños, arbustos nevados, cactus, rocas de nieve |

- PNGs: `Assets/Art/Props/Packs/<Tema>/<Barriers|Props>/`
- Prefabs: `Assets/Prefabs/Props/Packs/<Tema>/<Barriers|Props>/`
- `PropDefinitionSO`: `Assets/ScriptableObjects/Props/Packs/<Tema>/<Barriers|Props>/`
- Temas: `Assets/ScriptableObjects/Themes/ZoneTheme_<Tema>.asset`

`World_Sea` usa los 5 temas con pesos Sea 4, Forest 3, Cave 1, Ruins 1, Winter 1 (cada cuarto
elige su tema por peso), por lo que los props aparecen en la partida sin pasos extra.
Catálogo completo: `Assets/Art/Props/Packs/Vaciado_Props.md`.
Los **Barriers** llevan `BoxCollider2D` en layer `Walls` (igual que las barriers de granja), así
el jugador no puede atravesarlos.
Todos los props se dibujan por encima del jugador y enemigos (`z = -0.5` + `sortingOrder 4`;
jugador = 3, enemigos = 2), ocultando las piernas al pasar por detrás de arbustos; los
proyectiles usan orden 5 para verse sobre los props.

## Organización de props (`Assets/Art/Props`, `Assets/Prefabs/Props`, `Assets/ScriptableObjects/Props`)

- `Farm/` — props de granja originales (spritesheets `Plants.png` / `Supplies.png`).
- `Packs/<Tema>/<Barriers|Props>/` — packs temáticos generados (Forest, Sea, Cave, Ruins, Winter).
- `Sample/` — props greybox del mundo `World_Sample_A` (Objects, ObjectsClean, ObjectsSplit, World_test).

Los assets sin uso se mueven a `_UnusedBackup/` (fuera de `Assets/`).
