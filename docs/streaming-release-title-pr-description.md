# PR Description — Streaming release title parsing

## Title

`Fixed: Streaming release title parsing`

## Body

## Summary

Improves streaming indexer release title compatibility with Lidarr by centralizing title formatting in `ReleaseTitleFormatter` and removing provider-side placeholder metadata fallbacks.

Generated titles now follow a parser-friendly `Artist - Album (Year) [Quality] [WEB]` shape, while omitting missing optional metadata instead of inventing placeholder values.

Adds Tubifarry-owned verification that runs generated Qobuz, Deezer, Amazon Music, SubSonic, and YouTube titles through Lidarr's real `Parser.ParseAlbumTitle()`.

Also documents a confirmed live Lidarr edge case for Deezer titles with meaningful parenthetical album suffixes, such as soundtrack/disambiguation text.

## Tested

- `CI=true dotnet run -p:NuGetAudit=false -p:TreatWarningsAsErrors=false --project tools/ReleaseTitleProbe/ReleaseTitleProbe.csproj -- --assert --write-report docs/release-title-parser-investigation.md`
- `CI=true dotnet test -p:NuGetAudit=false -p:TreatWarningsAsErrors=false tests/Tubifarry.Test/Tubifarry.Test.csproj --filter ReleaseTitleParserCompatibilityFixture`
- `curl/GET /api/v1/release?albumId=...` against a live Lidarr instance to capture real Deezer parse-rejected titles and compare them with current formatter output

## Notes

- No MusicBrainz/MBID file-tagging changes
- No authored Lidarr parser changes
- Live Deezer failures observed in Lidarr are documented in `docs/lidarr-live-deezer-parse-failures.md`
