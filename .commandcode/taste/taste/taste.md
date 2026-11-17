# Taste
- Prefers the agent to investigate the root cause first, then present a plan for the fix before writing any code — explicitly wants the plan displayed in the conversation (plan-before-implement workflow). Confidence: 0.7
- Prefers obsolete placeholder/debug visuals to be removed once the real art/asset replaces them, rather than left in the codebase. Confidence: 0.4
- When a CLI tool isn't discoverable, prefers a proper system-wide install via the distro package manager (e.g. `sudo pacman -S git-lfs`) over workarounds like symlinking into ~/.local/bin or editing PATH. Confidence: 0.5
- For Unity work, prefers editor-side operations (creating GameObjects, attaching scripts, building prefabs, wiring serialized references) be done through the Unity MCP bridge rather than by hand-editing asset/scene files. Confidence: 0.5
- Prefers to be spoken to in English (explicitly requested "Speak to me in English" after the agent replied in German). Confidence: 0.9
- Refers to in-game entities by their Spanish names (e.g. "lobo guardian", "leñador", "luciérnaga") even when writing instructions in English. Confidence: 0.6
