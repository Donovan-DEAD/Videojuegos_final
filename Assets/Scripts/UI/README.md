# Scripts/UI — Flujo de pantallas

`GameFlowManager` es un **singleton persistente** (se crea solo si no existe) que mantiene el estado de la
run (`SelectedCharacter`, `RunSeed`, `CurrentWorldIndex`, `PendingWorldIndex`, `NextScene`, `Settings`)
y navega entre escenas con **fundido**.

| Archivo | Rol |
|---|---|
| `GameFlowManager` | `StartRun`, `Retry`, `GoToMainMenu`, `GoToDefeat`, `OpenSettings`/`CloseSettings`, `LoadSceneWithLoading`, `NotifyPlayerDied`, `NotifyRunCompleted`, `QuitGame`. |
| `ScreenFader` | Fundido a negro antes/después de cada carga (lo usa `TransitionTo`). |
| `LoadingScreen` | `02_Loading`: carga asíncrona de `NextScene` con barra y %. |
| `MainMenuController` | Jugar / Ajustes / Salir + elección de protagonista. |
| `DefeatController` | Reintentar / Menú principal / Salir. |
| `SettingsController` | Ajustes (volúmenes/ pantalla completa) con evento `Closed` (se reusa como panel in-game). |
| `SettingsData` | Persistencia de ajustes en `PlayerPrefs`. |

## Flujo

```
MainMenu --(Jugar)--> Loading --> Gameplay --(jefe)--> Portal --> Loading --> Gameplay (mundo N+1)
                                   |--(muerte)--> Defeat --> Menu / Reintentar
     `Esc` en gameplay --> Pausa (Ajustes in-game, sin salir de la escena)
```
