# Changelog

All notable changes to BetterJump are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/), and this
project adheres to Semantic Versioning.

## [1.5.1] - 2026-06-17

### Changed
- Started maintaining a full changelog that is now published on GitHub, Thunderstore
  and Nexus. No gameplay changes compared to 1.5.0.

## [1.5.0] - 2026-06-17

### Fixed
- Jumping works again after the Mimesis 0.3.0 update: a jump actually changes your
  height once more. The game's new ground handling was cancelling the upward impulse
  the instant you left the ground.
- You can no longer re-trigger a jump in mid-air to climb upwards. A jump is only
  re-armed after you have actually landed.

### Added
- New `AirGravityScale` preference for tunable airtime. Reduced gravity during the
  rise gives a floaty, higher arc that blends back to normal gravity near the apex
  (so there is no hovering at the top), while the descent keeps normal gravity for
  crisp landings.

### Changed
- Tuned jump defaults: `JumpVelocity` 4.8, `AirGravityScale` 0.7.

## [1.4.0] - 2026-06-15

### Fixed
- Compatibility with the Mimesis 0.3.0 game update (updated the MimicAPI reference
  for the new game build).

### Changed
- Refreshed the README and project documentation.

## [1.3.0] - 2025-11-15

### Added
- MimicAPI integration. Jump internals are now accessed through MimicAPI's reflection
  helpers for better compatibility across game updates.
- Optional debug-logging preference for troubleshooting jump behaviour.

## [1.2.0] - 2025-11-15

### Fixed
- Release packaging and Thunderstore upload fixes; the post-build step now only runs
  on Windows.

## [1.1.0] - 2025-11-15

### Added
- Automated build-and-release pipeline and Thunderstore packaging.

## [1.0.0] - 2025-11-15

### Added
- Initial release. Makes jumping in Mimesis actually useful so you can navigate the
  maps.
