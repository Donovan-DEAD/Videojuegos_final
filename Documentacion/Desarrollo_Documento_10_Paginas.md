# Madness Dreams — Desarrollo del documento de 10 páginas

> Contenido listo para volcar en las páginas 2, 3 y 6, alineado con lo que ya está
> implementado en `Assets/Scripts/` (FSM de enemigos, jefes por fases, mundos en `RunManager`).

---

## Page 2 — Historia y Gameplay

### Historia

**Inicio.** Hansel y Gretel se pierden en el bosque. Mientras buscan el camino de vuelta, una
luz fría los arrastra hacia un sueño roto: un ser superior —*el Titiritero* (nombre de trabajo)—
secuestra a uno de los hermanos y deja al otro dentro de un reino de cuentos distorsionados.
El jugador elige si controla a Hansel o a Gretel; el otro es el cautivo.

**Nudo.** Cada mundo es una versión corrompida de un cuento clásico, gobernada por un
protagonista que el Titiritero mantiene bajo su control. Para avanzar, el jugador debe cruzar
el mundo, sobrevivir a sus enemigos temáticos y derrotar al guardián (jefe). Cada victoria
libera un fragmento de la memoria del hermano y arranca una parte del poder del guardián,
que el jugador conserva de forma permanente.

**Desenlace / cliffhanger.** Tras derrotar a Caperucita y a Peter Pan, el protagonista descubre
que el Titiritero se alimenta del propio combate: cada pelea lo hace más fuerte. El hermano
sigue cautivo y el rastro se adentra en mundos todavía más profundos. El final queda abierto
hacia nuevos cuentos.

### Escenario, personajes y conflicto

- **Escenario:** mundos de cuentos deformados, generados proceduralmente con una semilla.
- **Personajes:** el protagonista (Hansel o Gretel), el hermano cautivo, el Titiritero y los
  guardianes corrompidos (Caperucita, Peter Pan y futuros).
- **Conflicto:** rescatar a la familia sin destruir a los héroes de cuento que están siendo
  manipulados. El jugador "libera", no asesina.

### Gameplay (flujo en etapas)

1. Elegir protagonista y empezar en el **Bosque de Caperucita**.
2. Explorar salas generadas con semilla (el mapa cambia en cada partida).
3. Combatir enemigos cuya apariencia y mecánica cambian según el mundo.
4. Derrotar al jefe del mundo para desbloquear la salida.
5. Obtener una **mejora de arma o habilidad** persistente del jefe.
6. La semilla cambia; el progreso se conserva entre mundos.
7. Avanzar al **Mar de Nunca Jamás** y, más adelante, a nuevos mundos.
8. Rescatar al hermano y derrotar al Titiritero.

---

## Page 3 — Game Flow (crecimiento y progresión)

### Cómo crece el jugador

El jugador crece **derrotando jefes**, no subiendo de nivel por acumular experiencia. Cada
guardián vencido entrega una mejora que queda guardada para el resto de la partida. Esto
une la progresión con la historia: cada mejora es un fragmento de memoria del hermano que
se recupera y, al mismo tiempo, una herramienta para llegar más lejos.

### Sistemas propuestos

| Sistema | Función |
|---|---|
| **Fragmentos de cuento** | Recurso de progresión soltado por jefes. Funcionan como "moneda de memoria". |
| **Botín de jefe** | Mejora de arma o habilidad única por mundo (persistente). |
| **Corazones / vida** | Vida base del jugador, ampliable con mejoras. |
| **Armas melee y a distancia** | El jugador ya tiene `Space` (melee) y `K` (ranged); las mejoras modifican daño, alcance, proyectil o añaden movilidad. |
| **Semilla procedural** | Cambia al vencer a un jefe; el progreso se mantiene entre mundos. |

### Qué gana el jugador

- **Tras Caperucita (Bosque):** *Garra del Lobo* (mejor ataque melee: más daño y un
  embestida corta) o *Cesta de Espinas* (proyectil que ralentiza enemigos).
- **Tras Peter Pan (Mar):** *Daga de Peter* (proyectil con rebote o mayor velocidad) o
  *Polvo de Hada* (habilidad de evasión / dash corto).
- **En general:** más vida, más daño, nuevos patrones de proyectil y movilidad, además de
  desbloqueables de apariencia.

---

## Page 6 — Game World

### Entornos

#### 1. El Bosque de Caperucita (mundo 1)

- **Descripción:** bosque oscuro de niebla carmesí, árboles retorcidos, caminos de hojas
  muertas y cabañas derruidas. Tema visual: rojo, negro y gris.
- **Mood:** cuento de hadas convertido en pesadilla; sensación de ser observado.
- **Conexión con la historia:** es el primer cuento que el Titiritero distorsiona; aquí el
  jugador entiende que los héroes de los cuentos están siendo usados como marionetas.
- **Enemigos temáticos:**
  - *Lobo de la guardia:* melee, persecución rápida, gran radio de detección.
  - *Zarza viviente:* melee, lenta, mucha vida.
  - *Leñador poseído:* melee, daño alto, rango medio.
  - *Luciérnaga maldita:* ranged, proyectil de polen.
- **Jefe:** **Caperucita Roja corrompida.**

#### 2. El Mar de Nunca Jamás (mundo 2)

- **Descripción:** océano crepuscular con islotes flotantes, barcos pirata encallados,
  aguas bioluminiscentes y restos de un cielo nocturno estrellado. Tema visual: azul noche,
  cian y dorado.
- **Mood:** maravilla infantil rota por el abandono; melancolía y aventura torcida.
- **Conexión con la historia:** contrasta con el bosque cerrado mediante la apertura del mar;
  aquí el jugador aprende que no puede confiar en la figura del "héroe que no crece".
- **Enemigos temáticos:**
  - *Grumete / niño perdido:* melee, daño medio.
  - *Sirena:* ranged, proyectil de agua, alto radio de detección.
  - *Cocodrilo Tic-Tac:* melee, persecución rápida, mucha vida.
  - *Hada de polvo:* ranged, pequeña y veloz.
- **Jefe:** **Peter Pan corrompido.**

### Conexión entre mundos

Navegación **lineal por ahora** (lista de `WorldConfigSO` en `RunManager`), con un posible
**hub** de cuentos más adelante. El orden de los dos primeros mundos refuerza el contraste:
**bosque cerrado y opresivo → mar abierto y desolado**.

### Diagrama de navegación

```
Hub / inicio (opcional)
        |
        v
[1] Bosque de Caperucita ── jefe: Caperucita ──> mejora persistente
        |                                        (semilla cambia)
        v
[2] Mar de Nunca Jamás ──── jefe: Peter Pan ───> mejora persistente
        |                                        (semilla cambia)
        v
Mundos futuros (hielo, castillo, etc.)
        |
        v
Rescate del hermano + derrota del Titiritero
```

---

## Diseño concreto de los dos jefes (mapeado al sistema actual)

El sistema ya soporta `Melee`, `Ranged`, fases (`phaseHealthThresholds`) e invocación de
esbirros (`minionPrefabs` / `minionsPerSummon`). Los dos jefes encajan sin tocar código.

### Caperucita Roja (Bosque)

| Fase | Vida | Comportamiento |
|---|---|---|
| 1 | 100%–66% | Melee con cuchillo (`attackType = Melee`), rango corto, embestida corta. |
| 2 | 66%–33% | Invoca lobos (esbirros) y alterna ranged lanzando cestas/espinas. |
| 3 | <33% | Enrage: sube `chaseSpeed`, `damage` y número de lobos invocados. |

- **Stats sugeridos:** `maxHealth` alto, `attackRange` corto en fase 1, `projectilePrefab` de
  espinas para fase 2/3, `fovAngle` amplio (cazadora).
- **Recompensa:** Garra del Lobo (melee) o Cesta de Espinas (ranged).

### Peter Pan (Mar)

| Fase | Vida | Comportamiento |
|---|---|---|
| 1 | 100%–66% | Ranged lanzando dagas (`attackType = Ranged`), `chaseSpeed` alto para simular vuelo. |
| 2 | 66%–33% | Invoca grumetes/hadas y mantiene el ataque ranged. |
| 3 | <33% | Invoca su **sombra** (esbirro) y lanza proyectiles en abanico. |

- **Stats sugeridos:** `maxHealth` medio-alto, `chaseSpeed` alto (movilidad), `projectileSpeed`
  alto, `detectionRadius` amplio.
- **Recompensa:** Daga de Peter (ranged) o Polvo de Hada (dash/evasión).

> El "vuelo" de Peter Pan se resuelve visualmente en un juego top-down: subir `chaseSpeed` y
> dejar que el sprite flote, sin necesidad de un eje de altura real.
