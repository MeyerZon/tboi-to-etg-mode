# How to work on this mod

This repository is set up so that most of the engineering happens in Claude Code cloud sessions
and the only thing that has to happen on your own PC is playing the game. This page explains the
loop, the conventions, and the exact steps for testing a build in Enter the Gungeon.

## 1. The two environments

| | Cloud session (Claude) | Your PC |
|---|---|---|
| Compile the mod (`net35` DLL) | yes | yes (optional) |
| Run unit tests | yes | yes (optional) |
| Build the Thunderstore package | yes | yes (optional) |
| Run Enter the Gungeon and play-test | **no** | **yes** |
| Draw sprites, record audio | no | yes |

The game is proprietary and needs a display, so it never runs in the cloud. Everything else does.

## 2. The day-to-day loop

1. **Start a cloud session** on this repository. The `SessionStart` hook installs the .NET SDK and
   restores packages (about a minute). Pick **Auto** in the permission dropdown so builds and tests run
   without prompts while you are away.
2. **Give the task.** Reference requirement IDs from [`REQUIREMENTS.md`](REQUIREMENTS.md)
   (for example "implement CHR-5 and CHR-8, the Tears gun and the gun lock"). Decisions from section 9
   should be stated once; Claude records them in the requirements document.
3. **Claude works on a branch**, runs `dotnet build` and `dotnet test`, commits and pushes. Every push
   triggers the **Build** workflow, which uploads two artifacts: `IsaacMode-dll` and `IsaacMode-thunderstore`.
4. **You play-test.** Download the DLL artifact from the Actions run (section 4), drop it in the game,
   check the F2 console, play.
5. **Report back** in the session: what worked, what did not, and the relevant lines from
   `BepInEx/LogOutput.log`. Claude fixes and pushes again.
6. **Open a PR** when the milestone is done. CI must be green. Merge into `main`.

## 3. Branches, commits, PRs

- `main` is always buildable. Work happens on short-lived branches, one milestone or feature per branch.
- Commit messages say what changed and why, and name the requirement IDs they implement.
- A PR describes the change, lists how it was validated (build, tests, in-game check), and notes any
  requirement that was changed or deferred. CI must pass before merging.
- Version bumps touch three places together: `ModVersion` in `Directory.Build.props`,
  `Plugin.VERSION` in `src/IsaacMode/Plugin.cs`, and `version_number` in `thunderstore/manifest.json`.
  `scripts/package.sh` refuses to package if the first and third disagree. Add a `CHANGELOG.md` entry.

## 4. Getting a build to test

**From CI (no tools needed on your PC)**

1. Open the repository on GitHub, go to **Actions**, open the latest **Build** run for the branch.
2. Under **Artifacts**, download `IsaacMode-dll` (a zip containing `IsaacMode.dll`). Artifacts are
   kept for 90 days and require being signed in to GitHub.

**Building locally** (Windows, macOS or Linux)

Install the .NET 8 SDK, then:

```bash
dotnet build IsaacMode.sln -c Release
# -> src/IsaacMode/bin/Release/net35/IsaacMode.dll
```

To have every local build copied straight into the game, create `local.props` next to the solution
(it is git-ignored) from `local.props.example` and set `EtgPluginsDir` to your
`<Enter the Gungeon>/BepInEx/plugins` folder.

## 5. Installing the build in the game

One-time setup:

1. Install a mod manager (r2modman, Gale or Thunderstore Mod Manager) and select Enter the Gungeon,
   or install `BepInExPack_EtG` manually into the game folder.
2. Install **Mod the Gungeon API** and **Alexandria** from Thunderstore. They are the mod's dependencies.
3. Launch the game once modded so BepInEx creates its folders, then quit.

Each test:

1. Put `IsaacMode.dll` in `<profile or game folder>/BepInEx/plugins/IsaacMode/`.
   With r2modman the profile folder is under **Settings → Browse profile folder**.
2. Start the game. Press **F2** to open the console; you should see `Isaac Mode vX.Y.Z loaded`.
   **F3** shows the log, **F1** the plugin list.
3. If something is wrong, the full log is `BepInEx/LogOutput.log`. Paste the lines around the first
   `[Error]` or `Exception` into the session.

Useful console commands while testing (the F2 console): `character <name>` switches Gungeoneer in the
Breach, `give <prefix:item_name>` grants items, `spawn_chest` spawns chests. The mod will add an `isaac`
command group for mode toggles and cheats (UI-3 in the requirements).

## 6. Releasing to Thunderstore

1. Bump the version in the three places listed in section 3 and update `CHANGELOG.md`.
2. Run `scripts/package.sh` (locally or let CI do it) and take `artifacts/IsaacMode-<version>.zip`,
   or download the `IsaacMode-thunderstore` artifact from the CI run.
3. Replace `thunderstore/icon.png` with real 256x256 art before the first public release.
4. Upload the zip at https://thunderstore.io/c/enter-the-gungeon/create/ under your team.
   Dependencies are already declared in `manifest.json`.

Releases are manual on purpose. If you later want Claude to publish, add a Thunderstore API token as an
environment secret and ask for a release workflow.

## 7. Adding assets

- Everything under `src/IsaacMode/Resources/` with the extensions `.png .txt .json .jtk2d .bnk` is
  embedded in the DLL and addressed from code as `IsaacMode/Resources/<folder>/<file>`.
- Character animations go in `Resources/Characters/Isaac/newspritesetup/<clip>/` as numbered PNG
  frames, one folder per clip named exactly as Alexandria expects (list in
  [`research/api-patterns.md`](research/api-patterns.md), section 1.3). In Aseprite, tag each clip and
  export frames per tag into those folders.
- Item icons: up to 30x30 px, 16 px = 1 world unit, avoid pure black on the outline.
- Sounds: WAV files turned into a `.bnk` with `gen-gungeon-audio-bank.py` from pcrain's
  gungeon-modding-tools (Python, no Wwise needed).
- Sprites extracted from the owner's copy of The Binding of Isaac are allowed and credited; **no TBOI
  music or sound effects, ever**. See [`REQUIREMENTS.md`](REQUIREMENTS.md) section 8 and
  [`research/crossover-precedents.md`](research/crossover-precedents.md) for why.

## 8. Where things live

| Path | Purpose |
|---|---|
| `src/IsaacMode/Plugin.cs` | BepInEx entry point; game init happens in `GMStart` |
| `src/IsaacMode/Core/` | Pure TBOI rule math, no game types, unit tested |
| `src/IsaacMode/Resources/` | Embedded sprites, character data, sound banks |
| `tests/IsaacMode.Tests/` | xunit tests (.NET 8) over the `Core/` sources |
| `thunderstore/` | Package manifest and icon |
| `scripts/package.sh` | Release build and Thunderstore zip |
| `.github/workflows/build.yml` | CI: build, test, package, artifacts |
| `.claude/` | Cloud session hook and permission rules |
| `docs/REQUIREMENTS.md` | What the mod must do; the source of truth for scope |
| `docs/research/` | Source-linked notes behind the requirements |
| `CLAUDE.md` | Conventions the coding sessions follow |

## 9. Tips for working with the cloud sessions

- Say which requirement IDs a task covers and what "done" looks like (for example "F2 shows the load
  message and Isaac cannot pick up a gun from a chest").
- Paste log excerpts rather than describing errors.
- Decisions you make in chat should end up in `REQUIREMENTS.md` section 9; ask for that explicitly if
  Claude did not do it.
- Ask for a PR when you want a reviewable unit; otherwise work accumulates on the branch and every push
  still produces a testable DLL.
- If a session reports a tool or network problem, the fix is usually in the environment settings
  (cloud environment menu in the session title bar, then Edit): setup script, network access, secrets.
