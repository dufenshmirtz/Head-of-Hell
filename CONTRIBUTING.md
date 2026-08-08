# Contributing

This repository is still in active game development. Keep documentation cleanup, repository cleanup, asset changes, and gameplay changes separate whenever possible so the team can review each kind of change safely.

## Unity Version

Use Unity `2022.3.24f1` for development unless the team agrees to upgrade.

Open the Unity project from the `Head-of-Hell/` folder, not from the repository root.

## Branch And Merge Notes

- Use descriptive commit titles.
- Add commit descriptions when the title alone does not explain the change.
- Before merging a feature branch, update it with the latest main branch.
- Consult the project lead before merging into the main branch.
- Do not rewrite shared history unless everyone affected understands the consequences.
- Avoid mixing game logic changes with documentation or repository-cleanup commits.

## Coding Notes

These conventions are adapted from the original README's coding manifesto:

- Match the style of nearby code before introducing a new style.
- Function and method names should start with an uppercase letter when following the project's existing convention.
- Variable names should start with a lowercase letter and use camelCase for multiple words.
- Opening braces for functions should go below the function declaration when matching the existing style.
- Place comments above functions when they explain purpose or behavior.
- Use inline comments only when they clarify code that would otherwise be hard to read.
- Remove temporary test prints before committing.
- Use test-only print statements for local debugging and `Debug.Log`, `Debug.LogWarning`, or `Debug.LogError` for meaningful runtime diagnostics.
- Avoid vague counter names when a more descriptive name would make the code easier to understand.

## Repository Hygiene

Do not commit generated Unity folders or local editor state:

- `Library/`
- `Temp/`
- `Obj/`
- `Logs/`
- `UserSettings/`
- `.vs/`
- `.idea/`
- build outputs
- ML-Agents timer logs
- local analysis outputs

Generated ML-Agents result folders should normally stay out of Git. The exception is the curated `Head-of-Hell/Assets/results/hoh_run1/` run, which is intentionally kept for project history and portfolio context.

Curated trained agents under `Head-of-Hell/Assets/SuccessfulAgents/` are project assets. Do not remove or rename them during cleanup without confirming with the team. This includes `AgentSmith3.2.onnx`.

Large binary files, trained models, videos, audio files, and generated builds should be reviewed before committing. Use Git LFS when the team decides a large binary asset should remain in the repository.

## Audio, Art, And License Review

Some audio/music files are temporary placeholders for development and testing. Keep them available for the team, but do not treat them as cleared public-release assets until their licenses and credits are confirmed.

Before public release or CV promotion, update `CREDITS.md` with third-party assets, music, sound effects, fonts, model sources, and tool credits.
