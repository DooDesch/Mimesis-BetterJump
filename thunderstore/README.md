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
- Uses your normal jump key - no input rebinding.

## Requirements

| Component | Version |
|---|---|
| MIMESIS | 0.3.0 (current Steam build) |
| MelonLoader | 0.7.3+ |
| MimicAPI | Required - [NeoMimicry/MimicAPI](https://github.com/NeoMimicry/MimicAPI) |

BetterJump reads private jump and ground state through MimicAPI and will not function without it. Installing through a mod manager pulls in MimicAPI (`NeoMimicry-MimicAPI-0.3.0`) and MelonLoader automatically.

## Installation

- **Recommended:** install with a Thunderstore mod manager (r2modman or Gale). It installs MelonLoader and MimicAPI for you - just click Install.
- **Manual:**
  1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader) 0.7.3+ into MIMESIS.
  2. Place both `BetterJump.dll` and `MimicAPI.dll` into `MIMESIS/Mods/`.
  3. Launch the game once to generate the configuration file at `UserData/MelonPreferences.cfg`.

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
- Changes take effect on game (re)launch.
- Effects apply only to your own avatar, so the mod is safe in multiplayer - it never affects other players.

Source and license (MIT, Copyright (c) 2025 DooDesch): [github.com/DooDesch/Mimesis-BetterJump](https://github.com/DooDesch/Mimesis-BetterJump).