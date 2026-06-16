# MIMESIS - BetterJump

> 🛟 **Need help or found a bug?** Get support at [support.doodesch.de](https://support.doodesch.de).


> Makes jumping in Mimesis actually useful - the stock jump is near-useless for getting around, so BetterJump retunes it (stronger configurable jump plus a brief forced-airborne window) to clear gaps and obstacles reliably.

![Version](https://img.shields.io/badge/version-1.4.0-blue)
![Game](https://img.shields.io/badge/game-MIMESIS-purple)
![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.3+-green)
![Status](https://img.shields.io/badge/status-working-brightgreen)

---

## Features

- Retunes your local player's jump so it is actually useful for map traversal (the default jump is near-useless).
- Configurable upward jump velocity (default `5.2` units/second) - raise it for higher, stronger jumps.
- Configurable "force unground" window after takeoff that keeps you airborne briefly so the game does not instantly re-detect the ground, improving jump responsiveness (default `0.08s`).
- Affects only your own avatar and only triggers when you jump from the ground - other players are unaffected.
- Single `Enabled` toggle to switch the mod on/off without uninstalling, plus an optional `DebugLogs` switch for troubleshooting.
- Implemented via Harmony patches on the game's existing jump and ground-check logic - no input rebinding, uses your normal jump key.

## Requirements

| Component | Version |
|---|---|
| MIMESIS | 0.3.0 (current Steam build) |
| MelonLoader | 0.7.3+ |
| MimicAPI | Required - [NeoMimicry/MimicAPI](https://github.com/NeoMimicry/MimicAPI) |

> BetterJump reads private jump/ground state through MimicAPI's reflection helpers and will not function without it. A Thunderstore mod manager resolves it automatically as `NeoMimicry-MimicAPI-0.3.0`.

## Installation

- **Recommended:** install via a Thunderstore mod manager (r2modman or Gale) from the [package page](https://github.com/DooDesch/Mimesis-BetterJump) - it pulls in MelonLoader and MimicAPI for you.
- **Manual:**
  1. Download `BetterJump.dll` from the [Releases page](../../releases).
  2. Make sure `MimicAPI.dll` is also present in your `Mods/` folder.
  3. Drop both DLLs into `MIMESIS/Mods/`.
  4. Launch the game once to generate the configuration file at `UserData/MelonPreferences.cfg`.

## Configuration

Stored in `UserData/MelonPreferences.cfg` under the `BetterJump` category.

| Option | Description | Default | Values/Range |
|---|---|---|---|
| `Enabled` | Enable BetterJump functionality. When disabled, the mod does not modify jump behavior. | `true` | `true` / `false` |
| `JumpVelocity` | Upward speed applied when a jump starts (units/second). Higher values result in stronger jumps. | `5.2` | Any value `>= 0` (negative values are clamped to `0` at runtime) |
| `ForceUngroundTime` | Seconds to keep the avatar airborne before the next ground check can succeed. Prevents the game from immediately detecting the ground after a jump, improving responsiveness. | `0.08` | Any value `>= 0.01` (clamped to a minimum of `0.01s`; the effective hold is at least `0.02s` at jump time) |
| `DebugLogs` | Enable detailed debug logging for troubleshooting jump behavior. | `false` | `true` / `false` |

## Usage

There is no keybind - just use your normal in-game jump input. BetterJump hooks the game's jump and ground-check logic, so jumping simply feels stronger and more responsive.

- Raise `JumpVelocity` for higher jumps; adjust `ForceUngroundTime` if jumps feel cut short.
- All tuning is done in `UserData/MelonPreferences.cfg` (category `BetterJump`); changes take effect on game (re)launch.
- Effects apply only to your own avatar.

## Compatibility

Built for Mimesis 0.3.0 / MelonLoader 0.7.3. The effect is purely client-side and local-avatar only, so it is safe in multiplayer - it only changes your own jump and never touches other players.

## Building (developers)

```
dotnet build -c Release
```

Targets `netstandard2.1`. References the game DLLs and MelonLoader from the shared workspace `lib/` set, and links MimicAPI via `ProjectReference` (`Private=false`) declared as a MelonLoader optional dependency. The PostBuild step copies `BetterJump.dll` into the game's `Mods/` folder.

## Credits / License

Created by **DooDesch**. Released under the **MIT License** (Copyright (c) 2025 DooDesch). Source and releases: [github.com/DooDesch/Mimesis-BetterJump](https://github.com/DooDesch/Mimesis-BetterJump).