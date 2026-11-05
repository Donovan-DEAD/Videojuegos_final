# Scripts/HUD — Interfaz de gameplay

`HUDController` enlaza al jugador, al `MapGenerator` y al `BossArenaController`; cada widget se actualiza
solo. Todo el HUD vive en el prefab **`Assets/Prefabs/UI/HUD.prefab`** (Canvas overlay 1920×1080).

| Archivo | Rol |
|---|---|
| `HUDController` | Punto de entrada: enlaza jugador/mapa/arena y escucha `BossSpawned`/`BossDefeated`. |
| `HealthBarWidget` | Barra + texto de vida, desde `Health.HealthChanged`. |
| `AttackCooldownWidget` | Sweeps de cooldown de melee y ranged. |
| `AbilityHotbarWidget` | 4 slots: ícono, cooldown y **bloqueo** (el ícono desaparece y queda una sombra con “X”). |
| `BossHealthBarWidget` | Barra del jefe (aparece al iniciar el combate). |
| `MinimapWidget` | Minimapa: un rectángulo por cuarto + marcador continuo del jugador. |
| `MapOverlayWidget` | Mapa grande (`M` o clic en el minimapa). |
| `MapGrid` | Helper: `BuildRooms` (un rectángulo por cuarto) y `Build` (grilla por celda, fallback sin `MapGenerator`). |
| `PauseController` | Pausa (`Esc`, `Time.timeScale = 0`) con Ajustes in-game y Menú. Ignora `Esc` durante las intro/outro y la pantalla de carga. |
| `DevMenuController` | Menú **DEV**: nivel, stats base y bloqueo de habilidades. |
| `TaleFragmentWidget` | Contador de Fragmentos de Cuento (placeholder). |

> El HUD es **overlay**: no aparece en capturas de cámara.

Ver: [../UI/README.md](../UI/README.md)
