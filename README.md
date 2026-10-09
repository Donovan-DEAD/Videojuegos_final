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
