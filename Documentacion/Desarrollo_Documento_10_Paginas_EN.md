# Madness Dreams — Ten-Page Document Development

> Content ready to drop into pages 2, 3, and 6, aligned with what is already
> implemented in `Assets/Scripts/` (enemy FSM, phased bosses, worlds in `RunManager`).

> **Demo scope note:** the full game features multiple fairy-tale worlds and enemies. For the
> class demo, only two worlds are implemented: **Red Riding Hood's Forest** and
> **Neverland Sea**. This does not mean the ideal game has only those two levels; the rest of
> the worlds (ice, castle, etc.) are part of the complete vision.

---

## Page 2 — Story and Gameplay

### Story

**Beginning.** Hansel and Gretel get lost in the forest. While searching for the way back, a
cold light drags them into a broken dream: a superior being —*the Puppeteer* (working name)—
kidnaps one of the siblings and leaves the other inside a realm of distorted fairy tales.
The player chooses whether to control Hansel or Gretel; the other is the captive.

**Middle.** Each world is a corrupted version of a classic tale, ruled by a protagonist the
Puppeteer keeps under its control. To advance, the player must cross the world, survive its
thematic enemies, and defeat the guardian (boss). Each victory frees a fragment of the
sibling's memory and tears away a piece of the guardian's power, which the player keeps
permanently. The full game continues this pattern across many fairy-tale worlds; the demo
covers the first two.

**Ending (full game).** After traveling through the remaining corrupted worlds, the surviving
sibling finally reaches the Puppeteer. Having recovered the stolen power and the memories of
the other sibling, the protagonist faces the superior being and defeats it, breaking the
control over the fairy-tale heroes and rescuing the family. They return to their own world.

**Demo ending / cliffhanger.** For the class demo, the journey stops after defeating
Red Riding Hood and Peter Pan: the sibling is still captive and the trail points deeper into
new tales, leaving the defeat of the Puppeteer as the goal of the complete game.

### Setting, characters, and conflict

- **Setting:** deformed fairy-tale worlds, procedurally generated with a seed. The full game
  spans many worlds; the demo shows two (forest and sea).
- **Characters:** the protagonist (Hansel or Gretel), the captive sibling, the Puppeteer, and
  the corrupted fairy-tale heroes (Red Riding Hood, Peter Pan, and many more in the full game).
- **Conflict:** rescue family without destroying the fairy-tale heroes who are being
  manipulated. The player "frees," not kills. In the end, one of the siblings confronts and
  defeats the superior being.

### Gameplay (flow in stages)

1. Choose a protagonist and start in **Red Riding Hood's Forest**.
2. Explore seed-generated rooms (the map changes every run).
3. Fight enemies whose appearance and mechanics change depending on the world.
4. Defeat the world's boss to unlock the exit.
5. Obtain a persistent **weapon or ability upgrade** from the boss.
6. The seed changes; progress carries over between worlds.
7. Advance to **Neverland Sea** (end of the demo).
8. In the full game, keep advancing through more fairy-tale worlds.
9. Rescue the sibling and defeat the Puppeteer.

---

## Page 3 — Game Flow (growth and progression)

### How the player grows

The player grows by **defeating bosses**, not by leveling up from accumulated experience.
Each defeated guardian grants an upgrade that is saved for the rest of the run. This ties
progression to the story: each upgrade is a fragment of the sibling's memory that is
recovered and, at the same time, a tool to go further. This progression chain continues
across every world in the full game, not only the two demo worlds.

### Proposed systems

| System | Function |
|---|---|
| **Tale fragments** | Progression resource dropped by bosses. They work as "memory currency." |
| **Boss loot** | A unique weapon or ability upgrade per world (persistent). |
| **Hearts / health** | Base player health, expandable with upgrades. |
| **Melee and ranged weapons** | The player already has `Space` (melee) and `K` (ranged); upgrades modify damage, range, projectile, or add mobility. |
| **Procedural seed** | Changes when a boss is defeated; progress is kept between worlds. |

### What the player gains

- **After Red Riding Hood (Forest):** *Wolf's Claw* (improved melee attack: more damage and a
  short dash) or *Thorn Basket* (projectile that slows enemies).
- **After Peter Pan (Sea):** *Peter's Dagger* (projectile with bounce or higher speed) or
  *Fairy Dust* (evasion ability / short dash).
- **In later worlds:** new weapons, abilities, and mobility tied to each fairy-tale guardian.
- **In general:** more health, more damage, new projectile patterns and mobility, plus
  cosmetic unlockables.

---

## Page 6 — Game World

### Environments

#### 1. Red Riding Hood's Forest (world 1 — demo)

- **Description:** dark forest with crimson fog, twisted trees, paths of dead leaves, and
  ruined cabins. Visual theme: red, black, and gray.
- **Mood:** fairy tale turned nightmare; the feeling of being watched.
- **Story connection:** it is the first tale the Puppeteer distorts; here the player
  understands that fairy-tale heroes are being used as puppets.
- **Thematic enemies:**
  - *Guard wolf:* melee, fast chase, large detection radius.
  - *Living bramble:* melee, slow, high health.
  - *Possessed woodcutter:* melee, high damage, medium range.
  - *Cursed firefly:* ranged, pollen projectile.
- **Boss:** **Corrupted Red Riding Hood.**

#### 2. Neverland Sea (world 2 — demo)

- **Description:** twilight ocean with floating islets, shipwrecked pirate ships,
  bioluminescent waters, and remnants of a starry night sky. Visual theme: night blue,
  cyan, and gold.
- **Mood:** childhood wonder broken by abandonment; melancholy and twisted adventure.
- **Story connection:** it contrasts with the closed forest through the openness of the sea;
  here the player learns not to trust the figure of the "hero who never grows up."
- **Thematic enemies:**
  - *Cabin boy / lost boy:* melee, medium damage.
  - *Mermaid:* ranged, water projectile, high detection radius.
  - *Tick-Tock crocodile:* melee, fast chase, high health.
  - *Dust fairy:* ranged, small and fast.
- **Boss:** **Corrupted Peter Pan.**

#### Future worlds (full game)

- Additional fairy-tale worlds planned beyond the demo, each with its own mood, thematic
  enemies, and corrupted guardian (e.g., an ice world, a castle, and others). They follow the
  same structure and reinforce the same progression.

### Connection between worlds

**Linear navigation for now** (list of `WorldConfigSO` in `RunManager`), with a possible
**hub** of tales later. The first two worlds reinforce the contrast:
**closed, oppressive forest → open, desolate sea**, and later worlds continue the sequence in
the full game.

### Navigation diagram

```
Hub / start (optional)
        |
        v
[1] Red Riding Hood's Forest ── boss: Red Riding Hood ──> persistent upgrade
        |                                                (seed changes)
        v
[2] Neverland Sea ───────────── boss: Peter Pan ───────> persistent upgrade
        |                                                (seed changes)
        v
        |  (end of the demo)
        v
Future worlds (ice, castle, etc.)                          [full game]
        |
        v
Rescue the sibling + defeat the Puppeteer                  [full game ending]
```

---

## Concrete design of the two bosses (mapped to the current system)

The system already supports `Melee`, `Ranged`, phases (`phaseHealthThresholds`), and minion
summoning (`minionPrefabs` / `minionsPerSummon`). The two demo bosses fit without touching
code, and the same pattern applies to future bosses.

### Red Riding Hood (Forest)

| Phase | Health | Behavior |
|---|---|---|
| 1 | 100%–66% | Melee with a knife (`attackType = Melee`), short range, short dash. |
| 2 | 66%–33% | Summons wolves (minions) and alternates ranged attacks throwing baskets/thorns. |
| 3 | <33% | Enrage: raises `chaseSpeed`, `damage`, and number of summoned wolves. |

- **Suggested stats:** high `maxHealth`, short `attackRange` in phase 1, `projectilePrefab`
  of thorns for phase 2/3, wide `fovAngle` (huntress).
- **Reward:** Wolf's Claw (melee) or Thorn Basket (ranged).

### Peter Pan (Sea)

| Phase | Health | Behavior |
|---|---|---|
| 1 | 100%–66% | Ranged daggers (`attackType = Ranged`), high `chaseSpeed` to simulate flight. |
| 2 | 66%–33% | Summons cabin boys/fairies and keeps the ranged attack. |
| 3 | <33% | Summons his **shadow** (minion) and fires projectiles in a fan pattern. |

- **Suggested stats:** medium-high `maxHealth`, high `chaseSpeed` (mobility), high
  `projectileSpeed`, wide `detectionRadius`.
- **Reward:** Peter's Dagger (ranged) or Fairy Dust (dash/evasion).

> Peter Pan's "flight" is solved visually in a top-down game: raise `chaseSpeed` and let the
> sprite float, without needing a real vertical axis.
