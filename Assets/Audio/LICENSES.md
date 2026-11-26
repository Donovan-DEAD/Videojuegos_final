# Music licenses

All five tracks shipped in `Assets/Audio/Music/` require attribution. Keep this file
with the project and reproduce the attribution in the game's credits screen.

Tracks were downloaded from their original pages on 2026-11-26 and verified as real
audio with `ffprobe` (MPEG audio, 44.1–48 kHz, stereo). No track was loop-edited or
transcoded; the MP3 files are used as delivered, so no "changes" note is required.

---

## Main menu — `MainMenu.mp3`

- **Title:** Gone Beyond
- **Creator:** Kevin MacLeod (incompetech.com)
- **Source:** https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100752
- **License:** Creative Commons Attribution 4.0 International (CC BY 4.0)
- **License URL:** https://creativecommons.org/licenses/by/4.0/
- **Changes:** none

## Red Riding Hood world — `RedRidingHood_World.mp3`

- **Title:** Willow and the Light
- **Creator:** Kevin MacLeod (incompetech.com)
- **Source:** https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100591
- **License:** CC BY 4.0
- **License URL:** https://creativecommons.org/licenses/by/4.0/
- **Changes:** none

## Red Riding Hood boss — `RedRidingHood_Boss.mp3`

- **Title:** Pop Goes the Weasel
- **Creator:** Kevin MacLeod (incompetech.com)
- **Source:** https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100724
- **License:** CC BY 4.0
- **License URL:** https://creativecommons.org/licenses/by/4.0/
- **Changes:** none

## Peter Pan world — `PeterPan_World.mp3`

- **Title:** Air Prelude
- **Creator:** Kevin MacLeod (incompetech.com)
- **Source:** https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100337
- **License:** CC BY 4.0
- **License URL:** https://creativecommons.org/licenses/by/4.0/
- **Changes:** none

> **Substitution note.** The originally preferred cue for this slot, "Spacial Winds"
> (USUAN1100203), returned HTTP 404 for every spelling of its title and its ISRC page
> carries no title in the served HTML, so it could not be obtained. Per the integration
> brief ("if a preferred track is unavailable, report it and audition the listed
> alternatives"), the listed CC BY 4.0 alternative "Air Prelude" was used instead.

## Peter Pan boss — `PeterPan_Boss.mp3`

- **Title:** Blackmoor Tides (Epic Pirate Battle Theme)
- **Creator:** Matthew Pablo
- **Source:** https://opengameart.org/content/blackmoor-tides-epic-pirate-battle-theme
- **License:** Creative Commons Attribution 3.0 Unported (CC BY 3.0)
- **License URL:** https://creativecommons.org/licenses/by/3.0/
- **Attribution instructions (from the source page):** http://www.matthewpablo.com/service
- **Changes:** none

---

## Suggested credits line

```
Music: "Gone Beyond", "Willow and the Light", "Pop Goes the Weasel", "Air Prelude"
by Kevin MacLeod (incompetech.com) — CC BY 4.0.
"Blackmoor Tides" by Matthew Pablo — CC BY 3.0.
```

---

## Not shipped — audition alternatives

The integration brief asked for ten alternatives to be kept outside `Assets/`. Four
were downloaded successfully during setup and are held outside the project in the
extraction folder; they are **not** in the build and carry no obligation unless used:

| Track | Section | License | Status |
|---|---|---|---|
| Determined Pursuit | Red Riding Hood boss | CC0 | downloaded, not shipped |
| Enemy Ship Approaching | Peter Pan world | CC0 | downloaded, not shipped |
| Pirate Indenture (v1, v2) | Peter Pan boss | CC0 | downloaded, not shipped |

The remaining six alternatives were listed as CC BY 4.0 on incompetech and were not
downloaded (the same client-side URL problem). If any is adopted later, add an entry
here before shipping.

## Verification status

- Audio integrity: **verified** with `ffprobe` on every file.
- Licenses: **verified against the live source pages** on 2026-11-26 — incompetech
  states "Creative Commons: By Attribution 4.0 License"; the OpenGameArt page for
  Blackmoor Tides shows CC-BY 3.0.
- Playback in-engine: see the integration report — this file makes no claim about
  runtime playback.
