# Changelog

## Unreleased

- Project skeleton: SDK-style `net35` mod project, unit tests, CI, Thunderstore packaging.
- Requirements document and research notes under `docs/`.
- Isaac registered as a Gungeoneer on the Bullet base with a generated placeholder sprite set (CHR-1 to CHR-4, CHR-9).
- Tears weapon: hidden, infinite, undroppable gun with fire rate, damage, range and speed from the TBOI base stats (CHR-5, CHR-6, STA-1, STA-3, STA-4).
- F2 console group `isaac` with `mode` and `stats` (UI-3).
- Build hardening: NuGet package source mapping, GitHub Actions pinned to commit SHAs.
- Isaac cannot pick up or be handed other guns, cannot dodge roll, and never holds blanks; armor loss no longer fires a blank for him (CHR-8, MOD-13, HLT-5).
- Tears follow the TBOI arc (level flight, then a fall at the end of their range), scale with damage and knock back by shot speed (CHR-7).
