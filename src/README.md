# Hindsight Source Code

The folders and files for this project are as follows:

- `HindsightGame/` - Unity 6 project for the game (proof-of-concept vertical
  slice: title screen, level select, and the Eye Drops walkthrough). See
  [`HindsightGame/README.md`](HindsightGame/README.md) for setup, architecture,
  tests, and builds.
  - `Assets/_Project/Scripts/Core/` - engine-independent game rules (C#).
  - `Assets/_Project/Scripts/Runtime/` - Unity components: menus, procedure
    runner, step views, UI.
  - `Assets/_Project/Scripts/Editor/` - editor tooling (import settings,
    scaffolding, validation, build scripts).
  - `Assets/_Project/Tests/` - EditMode and PlayMode automated tests.
  - `Tools/ArtGen/` - generator for the original placeholder artwork.
- `TraceToDesign.md` - traceability between code and design modules.
