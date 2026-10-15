# Taste

## Workflow

- Prefers the agent to investigate the root cause first, then present a plan for the fix before writing any code — explicitly wants the plan displayed in the conversation (plan-before-implement workflow). Confidence: 0.7

## Tooling

- When a CLI tool isn't discoverable, prefers a proper system-wide install via the distro package manager (e.g. `sudo pacman -S git-lfs`) over workarounds like symlinking into ~/.local/bin or editing PATH. Confidence: 0.5
- For Unity work, prefers editor-side operations (creating GameObjects, attaching scripts, building prefabs, wiring serialized references) be done through the Unity MCP bridge rather than by hand-editing asset/scene files. Confidence: 0.5
