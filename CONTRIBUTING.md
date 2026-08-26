# Contributing

This repository is in active game development. Keep documentation, repository cleanup, asset changes, and gameplay changes separate whenever possible.

## Unity Version

Use Unity `2022.3.24f1` for development unless the team agrees to upgrade.

Open the Unity project from the `Head-of-Hell/` folder, not from the repository root.

## Branch And Merge Notes

- Use descriptive commit titles.
- Add commit descriptions when the title alone does not explain the change.
- Update feature branches with the latest default branch before merging.
- Consult the project lead before merging into the default branch.
- Do not rewrite shared history unless everyone affected understands the consequences.
- Keep game logic changes separate from documentation or repository-cleanup commits.

## Coding Notes

These conventions are adapted from the original README's coding manifesto:

- Match the style of nearby code before introducing a new style.
- Function and method names should start with an uppercase letter when following the project's existing convention.
- Variable names should start with a lowercase letter and use camelCase for multiple words.
- Opening braces for functions should go below the function declaration when matching the existing style.
- Place comments above functions when they explain purpose or behavior.
- Use inline comments only when they clarify code that would otherwise be hard to read.
- Remove local test prints before committing.
- Use test-only print statements for local debugging and `Debug.Log`, `Debug.LogWarning`, or `Debug.LogError` for meaningful runtime diagnostics.
- Avoid vague counter names when a more descriptive name would make the code easier to understand.

## Repository Hygiene

Generated Unity folders and local editor state are excluded from version control:

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

The selected ML-Agents training run is stored at `Head-of-Hell/Assets/results/hoh_run1/`.

Curated trained agents are stored under `Head-of-Hell/Assets/SuccessfulAgents/`, including `AgentSmith3.2.onnx`.

Git LFS is used for selected large binary assets and media files.
