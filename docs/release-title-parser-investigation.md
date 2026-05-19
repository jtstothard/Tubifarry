# Release Title Parser Investigation

- Date: 2026-05-19 UTC
- Scope: Offline probe of Tubifarry `AlbumData.ToReleaseInfo()` title output against Lidarr `Parser.ParseAlbumTitle`.
- Parser boundary: Each fixture constructs representative provider `AlbumData`, applies the same release-date parse step used by provider parsers when indicated, emits `ReleaseInfo.Title`, then calls Lidarr's real `Parser.ParseAlbumTitle`.
- Harness behavior: Default mode reports evidence and exits non-zero for unexpected fixture construction/report write failures. `--assert` additionally exits non-zero for missing required provider coverage, required fixture errors, parser-null required provider titles, missing-metadata placeholder/date regressions, and codec-only quality-boundary regressions.
- Last assertion run represented in this report: `True`; assertion verdict: `PASS`.

## Provider Coverage

| Provider | Baseline covered? | Missing-metadata covered? | Source parser boundary | Fixture status | Parser verdict | Placeholder verdict |
|---|---:|---:|---|---|---|---|
| Qobuz | Yes | Yes | `Tubifarry/Indexers/DABMusic/DABMusicParser.cs` | Constructed | OK | OK |
| Deezer | Yes | Yes | `Tubifarry/Indexers/Lucida/LucidaRequestParser.cs; Tubifarry/Indexers/Lucida/LucidaServiceHelper.cs` | Constructed | OK | OK |
| Amazon Music | Yes | Yes | `Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs` | Constructed | OK | OK |
| SubSonic | Yes | Yes | `Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs` | Constructed | OK | OK |
| YouTube | Yes | Yes | `Tubifarry/Indexers/YouTube/YoutubeParser.cs` | Constructed | OK | OK |

## Fixture Results

### Qobuz

- Source parser boundary: `Tubifarry/Indexers/DABMusic/DABMusicParser.cs`
- Scenario: `provider baseline`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `David Bowie`
  - AlbumName: `Blackstar`
  - ReleaseDate / Precision: `2016-01-08` / `day`
  - Codec / Bitrate / BitDepth: `FLAC` / `1411` / `24`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `7` / `0` / `444000000`
- Emitted `ReleaseInfo.Title`: `David Bowie - Blackstar (2016) [FLAC 24bit] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `David Bowie`
  - AlbumTitle: `Blackstar`
  - ReleaseDate: `2016`
  - Quality: `FLAC 24bit v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Deezer

- Source parser boundary: `Tubifarry/Indexers/Lucida/LucidaRequestParser.cs; Tubifarry/Indexers/Lucida/LucidaServiceHelper.cs`
- Scenario: `provider baseline`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Daft Punk`
  - AlbumName: `Discovery`
  - ReleaseDate / Precision: `2001-03-12` / `day`
  - Codec / Bitrate / BitDepth: `MP3` / `320` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `14` / `0` / `null`
- Emitted `ReleaseInfo.Title`: `Daft Punk - Discovery (2001) [MP3 320kbps] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Daft Punk`
  - AlbumTitle: `Discovery`
  - ReleaseDate: `2001`
  - Quality: `MP3-320 v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Amazon Music

- Source parser boundary: `Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs`
- Scenario: `provider baseline`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Taylor Swift`
  - AlbumName: `folklore (deluxe version)`
  - ReleaseDate / Precision: `2020-08-18` / `day`
  - Codec / Bitrate / BitDepth: `FLAC` / `1411` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `17` / `0` / `612000000`
- Emitted `ReleaseInfo.Title`: `Taylor Swift - folklore {deluxe version} (2020) [FLAC] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Taylor Swift`
  - AlbumTitle: `folklore {deluxe version`
  - ReleaseDate: `2020`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### SubSonic

- Source parser boundary: `Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs`
- Scenario: `provider baseline`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Daft Punk`
  - AlbumName: `Random Access Memories`
  - ReleaseDate / Precision: `2013` / `year`
  - Codec / Bitrate / BitDepth: `FLAC` / `1000` / `16`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `13` / `0` / `536000000`
- Emitted `ReleaseInfo.Title`: `Daft Punk - Random Access Memories (2013) [FLAC 16bit] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Daft Punk`
  - AlbumTitle: `Random Access Memories`
  - ReleaseDate: `2013`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### YouTube

- Source parser boundary: `Tubifarry/Indexers/YouTube/YoutubeParser.cs`
- Scenario: `provider baseline`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Kacey Musgraves`
  - AlbumName: `Golden Hour feat. Willow`
  - ReleaseDate / Precision: `2018` / `year`
  - Codec / Bitrate / BitDepth: `AAC` / `128` / `0`
  - ExplicitContent: `True`
  - TotalTracks / Duration / Size: `13` / `2720` / `null`
- Emitted `ReleaseInfo.Title`: `Kacey Musgraves - Golden Hour feat. Willow (2018) [Explicit] [AAC 128kbps] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Kacey Musgraves`
  - AlbumTitle: `Golden Hour feat  Willow`
  - ReleaseDate: `2018`
  - Quality: `AAC-VBR v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Qobuz (missing metadata)

- Source parser boundary: `Tubifarry/Indexers/DABMusic/DABMusicParser.cs`
- Scenario: `missing metadata`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Qobuz Fixture Artist`
  - AlbumName: `No Date Qobuz Album`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `FLAC` / `1411` / `24`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `9` / `0` / `444000000`
- Emitted `ReleaseInfo.Title`: `Qobuz Fixture Artist - No Date Qobuz Album [FLAC 24bit] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Qobuz Fixture Artist`
  - AlbumTitle: `No Date Qobuz Album`
  - ReleaseDate: `0`
  - Quality: `FLAC 24bit v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Deezer (missing metadata)

- Source parser boundary: `Tubifarry/Indexers/Lucida/LucidaRequestParser.cs; Tubifarry/Indexers/Lucida/LucidaServiceHelper.cs`
- Scenario: `missing metadata`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Deezer Fixture Artist`
  - AlbumName: `No Date Deezer Album`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `MP3` / `320` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `10` / `0` / `null`
- Emitted `ReleaseInfo.Title`: `Deezer Fixture Artist - No Date Deezer Album [MP3 320kbps] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Deezer Fixture Artist`
  - AlbumTitle: `No Date Deezer Album`
  - ReleaseDate: `0`
  - Quality: `MP3-320 v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Amazon Music (missing metadata)

- Source parser boundary: `Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs`
- Scenario: `missing metadata`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Amazon Fixture Artist`
  - AlbumName: `No Date Amazon Album`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `FLAC` / `1411` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `11` / `0` / `612000000`
- Emitted `ReleaseInfo.Title`: `Amazon Fixture Artist - No Date Amazon Album [FLAC] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Amazon Fixture Artist`
  - AlbumTitle: `No Date Amazon Album`
  - ReleaseDate: `0`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### SubSonic (missing metadata)

- Source parser boundary: `Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs`
- Scenario: `missing metadata`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `SubSonic Fixture Artist`
  - AlbumName: `No Date SubSonic Album`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `FLAC` / `1000` / `16`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `12` / `0` / `536000000`
- Emitted `ReleaseInfo.Title`: `SubSonic Fixture Artist - No Date SubSonic Album [FLAC 16bit] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `SubSonic Fixture Artist`
  - AlbumTitle: `No Date SubSonic Album`
  - ReleaseDate: `0`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### YouTube (missing metadata)

- Source parser boundary: `Tubifarry/Indexers/YouTube/YoutubeParser.cs`
- Scenario: `missing metadata`
- Required provider assertion fixture: `True`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `YouTube Fixture Artist`
  - AlbumName: `No Date YouTube Album`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `AAC` / `128` / `0`
  - ExplicitContent: `True`
  - TotalTracks / Duration / Size: `13` / `2720` / `null`
- Emitted `ReleaseInfo.Title`: `YouTube Fixture Artist - No Date YouTube Album [Explicit] [AAC 128kbps] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `YouTube Fixture Artist`
  - AlbumTitle: `No Date YouTube Album`
  - ReleaseDate: `0`
  - Quality: `AAC-VBR v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Boundary (non-featuring parentheses)

- Source parser boundary: `Tubifarry/Core/Model/AlbumData.cs`
- Scenario: `non-featuring parentheses`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `John Coltrane`
  - AlbumName: `Blue Train (Remastered) ft. Lee Morgan`
  - ReleaseDate / Precision: `1958` / `year`
  - Codec / Bitrate / BitDepth: `FLAC` / `1000` / `24`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `5` / `0` / `320000000`
- Emitted `ReleaseInfo.Title`: `John Coltrane - Blue Train {Remastered} ft. Lee Morgan (1958) [FLAC 24bit] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `John Coltrane`
  - AlbumTitle: `Blue Train {Remastered} ft  Lee Morgan`
  - ReleaseDate: `1958`
  - Quality: `FLAC 24bit v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Boundary Unknown Date (blank date and precision)

- Source parser boundary: `Tubifarry/Core/Model/AlbumData.cs`
- Scenario: `blank date and precision`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `True`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Formatter Seam`
  - AlbumName: `No Date Needed`
  - ReleaseDate / Precision: ` ` / ``
  - Codec / Bitrate / BitDepth: `FLAC` / `0` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `1` / `0` / `42000000`
- Emitted `ReleaseInfo.Title`: `Formatter Seam - No Date Needed [FLAC] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Formatter Seam`
  - AlbumTitle: `No Date Needed`
  - ReleaseDate: `0`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Boundary Empty Precision (date with empty precision)

- Source parser boundary: `Tubifarry/Core/Model/AlbumData.cs`
- Scenario: `date with empty precision`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `True`
- Codec-only quality assertion: `True`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Formatter Seam`
  - AlbumName: `Date Without Precision`
  - ReleaseDate / Precision: `2024` / ``
  - Codec / Bitrate / BitDepth: `FLAC` / `0` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `1` / `0` / `42000000`
- Emitted `ReleaseInfo.Title`: `Formatter Seam - Date Without Precision [FLAC] [WEB]`
- Placeholder regression verdict: `OK`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Formatter Seam`
  - AlbumTitle: `Date Without Precision`
  - ReleaseDate: `0`
  - Quality: `FLAC v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Boundary Missing Optional Quality (codec-only quality)

- Source parser boundary: `Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs`
- Scenario: `codec-only quality`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `True`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture inputs:
  - ArtistName: `Formatter Seam`
  - AlbumName: `Codec Only Quality`
  - ReleaseDate / Precision: `` / ``
  - Codec / Bitrate / BitDepth: `MP3` / `0` / `0`
  - ExplicitContent: `False`
  - TotalTracks / Duration / Size: `1` / `0` / `null`
- Emitted `ReleaseInfo.Title`: `Formatter Seam - Codec Only Quality [MP3] [WEB]`
- Placeholder regression verdict: `Not asserted`
- `Parser.ParseAlbumTitle` verdict: **OK**
- Parsed fields:
  - ArtistName: `Formatter Seam`
  - AlbumTitle: `Codec Only Quality`
  - ReleaseDate: `0`
  - Quality: `Unknown v1`
  - ReleaseGroup: `WEB`
  - ReleaseHash: ``
  - ReleaseVersion: ``

### Boundary Missing Artist (required artist validation)

- Source parser boundary: `Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs`
- Scenario: `required artist validation`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture construction: Failed: `ArgumentException: ArtistName is required to format a release title. (Parameter 'ArtistName')`
- Emitted `ReleaseInfo.Title`: Not emitted
- `Parser.ParseAlbumTitle` verdict: Not run
- Failure detail: Fixture could not be constructed or emitted, so no parser evidence exists for this fixture.

### Boundary Missing Album (required album validation)

- Source parser boundary: `Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs`
- Scenario: `required album validation`
- Required provider assertion fixture: `False`
- Missing-metadata placeholder assertion: `False`
- Codec-only quality assertion: `False`
- Provider parser calls `AlbumData.ParseReleaseDate()`: `True`
- Fixture construction: Failed: `ArgumentException: AlbumName is required to format a release title. (Parameter 'AlbumName')`
- Emitted `ReleaseInfo.Title`: Not emitted
- `Parser.ParseAlbumTitle` verdict: Not run
- Failure detail: Fixture could not be constructed or emitted, so no parser evidence exists for this fixture.

## Negative Evidence Checklist

### Required provider baseline fixtures not covered

- None.

### Required provider missing-metadata fixtures not covered

- None.

### Fixtures with unexpected construction errors

- None.

### Parser null results for required provider fixtures

- None.

### Missing-metadata placeholder/fake-date regressions

- None.

### Missing optional quality regressions

- None.


## Field Fidelity Notes

- Amazon Music: parsed album differs from fixture album (\`folklore {deluxe version\` vs \`folklore (deluxe version)\`).
- YouTube: parsed album differs from fixture album (\`Golden Hour feat  Willow\` vs \`Golden Hour feat. Willow\`).

## Recommendation for S02

The current `AlbumData` formatter emits titles that Lidarr's real parser accepts for all five required provider baseline and missing-metadata fixtures. Missing-date provider fixtures omit year/date placeholders and placeholder tokens while still parsing, so S02 has executable regression protection for R004. Use `dotnet run --project tools/ReleaseTitleProbe/ReleaseTitleProbe.csproj -- --assert --write-report docs/release-title-parser-investigation.md` as the slice verification command.

The additional `Boundary` fixture is not provider coverage. It is a formatter boundary check for non-featuring parentheses plus `ft.` normalization so formatter changes can avoid regressing album-title normalization behavior.
