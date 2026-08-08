# D.I.E.N.A.M.O.

D.I.E.N.A.M.O. is a work-in-progress 2D Unity arena fighter by XLR8. The project focuses on fast local matches, character-specific abilities, custom match rules, player profiles, replays, telemetry, and experimental AI opponents trained with Unity ML-Agents.

This repository is still named `Head-of-Hell` in several folders because the game was developed under that working title. The intended public game name is `D.I.E.N.A.M.O.`

## Current Status

This game is in active development. Balance, menus, art, audio, AI behavior, project settings, and internal naming are not final.

The repository is being cleaned up for portfolio/CV review, so the documentation is intentionally honest about unfinished areas while highlighting the systems that already exist.

## Project Highlights

- Local 2D arena combat with movement, jumping, quick attacks, heavy attacks, blocking, parrying, charge attacks, projectiles, and character-specific abilities.
- PvP support for `1v1`, `1v1v1`, and `2v2` match setups.
- PvE setup with `Scripted Bot` and `ML Agent` opponent options across `Easy`, `Medium`, and `Hard` difficulty choices.
- Ten playable character identities in the current codebase: Steelager, Vander, Rager, Skipler, Fin, Lazy Bigus, Lithra, Chiback, Lupen, and Visvia.
- Custom rulesets for rounds, starting health, powerups, hidden health, player speed, disabled actions, portals, developer tools, and Chan Chan mode.
- Player profile creation, profile selection, profile analysis data, and menu stats display.
- Replay recording, replay browsing, and replay playback based on captured input frames.
- Match telemetry written to local JSON sessions for later analysis.
- Unity ML-Agents training/runtime work, including curated trained ONNX agents in `Assets/SuccessfulAgents/`.

## Repository Layout

```text
.
|-- README.md
|-- CREDITS.md
|-- CONTRIBUTING.md
|-- .gitattributes
`-- Head-of-Hell/
    |-- .gitignore
    |-- Assets/
    |   |-- Scenes/
    |   |-- Scripts/
    |   |-- CustomizationScripts/
    |   |-- SuccessfulAgents/
    |   `-- results/hoh_run1/
    |-- Packages/
    `-- ProjectSettings/
```

The Unity project folder is `Head-of-Hell/`. Open that folder in Unity Hub, not the repository root.

## Unity Setup

1. Install Unity `2022.3.24f1`.
2. Open the `Head-of-Hell/` folder with Unity Hub.
3. Let Unity restore packages from `Head-of-Hell/Packages/manifest.json`.
4. Start from `Assets/Scenes/HoHMainMenu.unity` for the main menu flow.

The configured build scenes are:

- `Assets/Scenes/HoHMainMenu.unity`
- `Assets/Scenes/GamePlayScene.unity`
- `Assets/Scenes/Intro.unity`
- `Assets/Scenes/TutorialScene.unity`

## Tech Stack

- Unity `2022.3.24f1`
- Universal Render Pipeline `14.0.10`
- Unity Input System `1.7.0`
- Unity ML-Agents `2.0.2`
- TextMeshPro
- Git LFS for selected large media formats (`.mp3`, `.mp4`)

## Gameplay Overview

The current flow supports local PvP and PvE play. PvP can be played as two-player duels, three-player free-for-all, or two-versus-two teams. PvE lets a player choose between a scripted bot and an ML-Agent-based bot, then select a difficulty.

Match setup includes character selection, stage selection, and optional custom rulesets. Custom rulesets can change the number of rounds, starting health, powerup behavior, movement speed, available actions, portal count, hidden-health behavior, and special Chan Chan behavior.

Controls are assigned through player and character setup in the Unity scenes. The tutorial reads the active key bindings when prompting the player.

## AI And Training Notes

The repository includes two kinds of AI-related material:

- Runtime/experimental AI code under `Assets/Scripts/RL/`.
- Curated trained ONNX agents under `Assets/SuccessfulAgents/`, including `AgentSmith3.2.onnx`.

Generated ML-Agents training outputs are intentionally not treated like normal source files. The preserved `Assets/results/hoh_run1/` folder is the curated training run kept for project history and portfolio context. Other generated result runs should stay out of Git unless the team deliberately decides otherwise.

## Development Notes

- This is a Unity project in progress, so generated folders such as `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, build outputs, and ML-Agents timer logs should stay out of Git.
- Large binary assets, trained models, builds, videos, and audio files should be reviewed before committing. Git LFS is already configured for `.mp3` and `.mp4`; additional binary formats can be added later if needed.
- Some internal names still use `HoH`, `Head-of-Hell`, or `DefaultCompany`. Those can be aligned with `D.I.E.N.A.M.O.` when the team is ready to touch Unity settings and serialized scene/project data.
- Some scripts and folders are experimental, obsolete, or temporary. They should be audited carefully before deletion because active development is still ongoing.

## Before Linking This Repository In A CV

- Add screenshots, gameplay clips, or a short demo build once the current gameplay state is stable.
- Finish the third-party asset, music, sound-effect, font, and model-license audit.
- Decide whether the project will stay proprietary or receive an explicit open-source license.
- Expand `CREDITS.md` with more specific implementation credits when the team is ready.
- Align visible project metadata with `D.I.E.N.A.M.O.` when it is safe to touch Unity settings.
- Keep cleanup changes separate from gameplay changes so reviewers can understand the development history.

## Credits And License

Team credits are listed in `CREDITS.md`.

No open-source license has been selected yet. Unless a `LICENSE` file is added, the code, assets, models, audio, and other project materials are not licensed for reuse.
