# Docuto AI Crawler (PoC)

This project is a small .NET console app that crawls a codebase, asks an LLM which files matter for architecture understanding, and generates Markdown documentation into a `docs/` folder inside the target project.

It is designed as a proof‑of‑concept for automated architectural documentation using OpenAI chat models.

## How It Works
1. Loads `projectPath` and `openAiKey` from `appsettings.json`.
2. Enumerates files in the target project (with ignore/whitelist support).
3. Uses an LLM to select the files worth documenting.
4. Generates Markdown documentation for each selected file.
5. Writes docs to `docs/<original_path>.md` in the target project.

## Requirements
- .NET SDK (target framework: `net10.0`)
- OpenAI API key

## Setup
1. Copy the example config:
```bash
cp Docuto.Agent/appsettings.json.example Docuto.Agent/appsettings.json
```

2. Edit `Docuto.Agent/appsettings.json`:
```json
{
  "openAiKey": "YOUR_OPENAI_API_KEY",
  "projectPath": "/absolute/path/to/your/project",
  "model": "gpt-5.1"
}
```

You can also use `appsettings.local.json` for local overrides that are git-ignored.

3.  **Command Line Arguments (Optional)**
You can override any setting via the command line, which is useful for CI/CD:
```bash
dotnet run --project Docuto.Agent -- projectPath "/path/to/proj" openAiKey "sk-..."
```

## Run
```bash
dotnet run --project Docuto.Agent
```

## Output
Documentation is written to:
```
<projectPath>/docs/<original_file_path>.md
```

## File Selection Rules
The crawler supports optional file lists in the target project:
- `.docwhitelist` (highest priority): explicit list of files to consider.
- `.docignore`: files to skip if no whitelist is present.

Rules in these files support:
- Exact file paths
- Folder prefixes ending in `/`
- Simple glob patterns like `*.cs`

If neither file exists, the crawler uses a default ignore list for common build and dependency folders (e.g. `bin`, `obj`, `.git`, `node_modules`).

## Notes
- The LLM chooses a subset of files to document; it will not necessarily document every file.
- The model is configurable via `model` in `appsettings.json` (defaults to `gpt-5.1` if omitted).
