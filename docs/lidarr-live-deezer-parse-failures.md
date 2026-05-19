# Live Lidarr Deezer Parse Failures

Captured on 2026-05-20 from the local Lidarr API (`v3.1.2.4913`, branch `plugins`) using `GET /api/v1/release?albumId=...` after temporarily narrowing interactive search to Deezer only.

## Confirmed live failures

### 1) Reneé Rapp - Lucky (from Now You See Me: Now You Don’t)
- Album ID: `232448`
- Release count from Deezer: `3`
- All three variants were rejected with the same parse reason.

#### Rejected titles
1. `Reneé Rapp - Lucky (from Now You See Me: Now You Don’t) (2025) [FLAC] [WEB]`
2. `Reneé Rapp - Lucky (from Now You See Me: Now You Don’t) (2025) [MP3 320] [WEB]`
3. `Reneé Rapp - Lucky (from Now You See Me: Now You Don’t) (2025) [MP3 128] [WEB]`

#### Parsed fields returned by Lidarr
- `artistName`: `Reneé Rapp`
- `albumTitle`: `Lucky`

#### Rejections
- `Unable to parse albums from release name`

#### Interpretation
Lidarr parses the prefix album as `Lucky`, but the monitored album is `Lucky (from Now You See Me: Now You Don’t)`. The parenthetical suffix appears to be treated as extra info instead of part of the album title, so the album lookup fails after parse.

---

### 2) Halsey - Lilith (Diablo IV anthem)
- Album ID: `233337`
- Release count from Deezer: `3`
- All three variants were rejected with the same parse reason.

#### Rejected titles
1. `Halsey - Lilith (feat. SUGA) (Diablo IV Anthem) (2023) [Explicit] [FLAC] [WEB]`
2. `Halsey - Lilith (feat. SUGA) (Diablo IV Anthem) (2023) [Explicit] [MP3 320] [WEB]`
3. `Halsey - Lilith (feat. SUGA) (Diablo IV Anthem) (2023) [Explicit] [MP3 128] [WEB]`

#### Parsed fields returned by Lidarr
- `artistName`: `Halsey`
- `albumTitle`: `Lilith`

#### Rejections
- `Unable to parse albums from release name`

#### Interpretation
This is the same failure shape, but worse: the title includes both `(feat. SUGA)` and `(Diablo IV Anthem)`. Lidarr keeps only `Lilith` as album title, which is insufficient to match the monitored album `Lilith (Diablo IV anthem)`.

---

## Confirmed live non-failures from the same Deezer API path

These demonstrate that parentheses alone are not the issue.

### Pass example: Animal Byproducts - Groundwork
`Animal Byproducts - Groundwork (2024) [FLAC] [WEB]`
- Rejection reason was **not** parse-related:
  - `Existing files meets cutoff: FLAC`
- `downloadAllowed: true`

### Pass example shape: Kim Petras / Wet Leg / Miya Folick candidates
The following albums returned Deezer releases with `releaseCount: 3` and **no** parse rejections in the sampled run:
- `Need For Speed (loki Remix)`
- `Alone (Major Lazer remix)`
- `Alone (James Hype remix)`
- `mangetout (The Dare remix)`
- `ROACH (live and acoustic)`
- `Colors (Ian Asher remix)`

## Working hypothesis

The current live failure pattern is **not** “any title with parentheses”. It is closer to:
- album titles where a **meaningful album suffix** is inside parentheses and must be preserved for matching, and/or
- titles with **multiple parenthetical groups**, especially when one is a feature marker and another is album metadata.

The rejection path is consistent with `DownloadDecisionMaker`:
- parse succeeds enough to populate `artistName`
- parse produces a shortened `albumTitle`
- mapped album lookup returns no albums
- Lidarr returns `Unable to parse albums from release name`

### Code-level cause in Lidarr

The parser regexes in `NzbDrone.Core/Parser/Parser.cs` use a minimal `album` capture before the first `(` or `[` in the common `Artist - Album (Year)` forms.

Relevant patterns:
- `Artist - Album (Year) Strict`
- `Artist - Album (Year)`
- `Artist - Album [something] or Artist - Album (something)`

Because the `album` group is captured as `.+?` before `\W*(?:\(|\[)`, titles like these are reduced during parse:
- `Reneé Rapp - Lucky (from Now You See Me: Now You Don’t) (2025) [FLAC] [WEB]` -> album `Lucky`
- `Halsey - Lilith (feat. SUGA) (Diablo IV Anthem) (2023) [Explicit] [FLAC] [WEB]` -> album `Lilith`

After that, `ParsingService.GetAlbums()` first tries exact title match, then inexact matching through `AlbumService.FindByTitleInexact()`. That service also explicitly tries `RemoveBracketsAndContents()` and `RemoveAfterDash()` variants, which further bias matching toward the stripped base title rather than the full parenthetical album name.

This explains why the artist and year survive, but the album match still fails and `DownloadDecisionMaker` raises `Unable to parse albums from release name`.

### What current Tubifarry formatter would emit for the same album names

`ReleaseTitleFormatter.NormalizeAlbumName()` rewrites non-feature parentheses to braces, which avoids Lidarr's `stop album capture at first (` behavior for the affected shapes.

Expected Lucida/Tubifarry outputs for the same album names:
- `Reneé Rapp - Lucky {from Now You See Me: Now You Don’t} (2025) [MP3 320kbps] [WEB]`
- `Halsey - Lilith (feat. SUGA) {Diablo IV Anthem} (2023) [Explicit] [MP3 320kbps] [WEB]`

That does **not** prove the full end-to-end flow yet because the current repo build is blocked by unrelated compile issues, but it does show the current Tubifarry formatter is aiming at the exact parser weakness observed in live Deezer failures.

### Tubifarry path coverage check

The current indexer-layer release creation paths all converge on `AlbumData.ToReleaseInfo()`, which in turn uses `ReleaseTitleFormatter.Format(this)`.

Confirmed call sites include:
- `Tubifarry/Indexers/Lucida/LucidaRequestParser.cs`
- `Tubifarry/Indexers/DABMusic/DABMusicParser.cs`
- `Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs`
- `Tubifarry/Indexers/YouTube/YoutubeParser.cs`
- `Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs`
- `Tubifarry/Indexers/Spotify/SpotifyParser.cs`
- `Tubifarry/Indexers/Soulseek/SlskdIndexerParser.cs`
- `Tubifarry/Core/Utilities/IndexerParserHelper.cs`

That means there is no obvious active indexer path in Tubifarry that still bypasses `ReleaseTitleFormatter` when constructing `ReleaseInfo.Title`.

## Operational notes
- Interactive search was temporarily narrowed to Deezer by disabling `enableInteractiveSearch` for Slskd and Bandcamp, then restored immediately after capture.
- No persistent config changes were left behind.
