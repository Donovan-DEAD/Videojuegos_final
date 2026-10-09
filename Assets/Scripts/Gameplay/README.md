# Scripts/Gameplay — Nivel y avance

| Archivo | Rol |
|---|---|
| `WorldLayout` | Al evento `MapGenerator.Generated` coloca al **jugador en el centro del cuarto de Spawn** y la **arena en el centro del cuarto del jefe**. |
| `WorldPortalController` | Mantiene el portal oculto y lo revela cuando muere el jefe. Escucha `BossManager.BossDefeated`; si no hay manager, engancha el `Health` del jefe presente en la escena. |
| `Portal` | Trigger: al tocarlo el jugador llama a `RunManager.RequestAdvance()` una sola vez. |

## Flujo

```
MapGenerator.Generate() → Generated → WorldLayout (jugador + arena)
jefe muere → WorldPortalController → Portal activo → jugador entra → RequestAdvance()
```

Ver: [../Boss/README.md](../Boss/README.md) · [../Generation/README.md](../Generation/README.md)
