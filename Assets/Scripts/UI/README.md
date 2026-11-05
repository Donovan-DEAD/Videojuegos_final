# Scripts/UI — Flujo de pantallas

`GameFlowManager` es un **singleton persistente** (se crea solo si no existe) que mantiene el estado de la
run (`SelectedCharacter`, `RunSeed`, `CurrentWorldIndex`, `PendingWorldIndex`, `NextScene`, `Settings`)
y navega entre escenas con **fundido**.

| Archivo | Rol |
|---|---|
| `GameFlowManager` | `StartRun`, `Retry`, `GoToMainMenu`, `GoToDefeat`, `OpenSettings`/`CloseSettings`, `LoadSceneWithLoading`, `PeekNextWorld`, `NotifyPlayerDied`, `NotifyRunCompleted`, `QuitGame`. |
| `ScreenFader` | Fundido a negro antes/después de cada carga (lo usa `TransitionTo`). |
| `LoadingScreen` | `02_Loading`: monta `NextScene` de forma **aditiva** mientras el interludio sigue visible; la barra (`Filled`) muestra carga + generación (`RunManager.GenerationProgress`) y la escena de carga se descarga cuando el mundo está listo. |
| `StoryInterludeController` | Panel de historia entre mundos. La barra muestra el tiempo de página por defecto o un **progreso externo de carga** si se le pasa un proveedor. |
| `MainMenuController` | Jugar / Ajustes / Salir + elección de protagonista. |
| `DefeatController` | Reintentar / Menú principal / Salir. |
| `SettingsController` | Ajustes (volúmenes/ pantalla completa) con evento `Closed` (se reusa como panel in-game). |
| `SettingsData` | Persistencia de ajustes en `PlayerPrefs`. |

## Flujo

```
MainMenu --(Jugar)--> Loading (aditivo) --> Gameplay --(jefe)--> Portal --> Loading --> Gameplay (mundo N+1)
                                        |--(muerte)--> Defeat --> Menu / Reintentar
     `Esc` en gameplay --> Pausa (Ajustes in-game, sin salir de la escena)
```

> En la transición, `02_Loading` inicia `LoadSceneAsync(03_Gameplay, Additive)` **antes** del
> interludio. La escena de gameplay se activa, genera el mundo por frames y reporta progreso; recién
> entonces se descarga `02_Loading`.
