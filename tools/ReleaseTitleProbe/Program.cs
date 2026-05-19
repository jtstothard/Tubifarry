using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Tubifarry.Core.Model;
using Tubifarry.Core.Utilities;

namespace ReleaseTitleProbe;

internal static partial class Program
{
    private static readonly string[] RequiredProviders =
    [
        "Qobuz",
        "Deezer",
        "Amazon Music",
        "SubSonic",
        "YouTube"
    ];

    private static readonly string[] PlaceholderTokens =
    [
        "Unknown",
        "0000"
    ];

    private static int Main(string[] args)
    {
        ProbeOptions options;
        try
        {
            options = ParseOptions(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        List<ProbeResult> results = RunProbe().ToList();
        List<AssertionFailure> assertionFailures = EvaluateAssertions(results).ToList();

        PrintResults(results);

        if (options.Assert)
        {
            PrintAssertions(assertionFailures);
        }

        if (!string.IsNullOrWhiteSpace(options.ReportPath))
        {
            try
            {
                WriteReport(options.ReportPath, results, assertionFailures, options.Assert);
                Console.WriteLine();
                Console.WriteLine($"Wrote release title parser investigation report: {options.ReportPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to write report '{options.ReportPath}': {ex.GetType().Name}: {ex.Message}");
                return 1;
            }
        }

        if (options.Assert && assertionFailures.Count > 0)
        {
            return 1;
        }

        return results.Any(result => result.UnexpectedFixtureError is not null) ? 1 : 0;
    }

    private static ProbeOptions ParseOptions(string[] args)
    {
        string? reportPath = null;
        bool assert = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--assert":
                    assert = true;
                    break;
                case "--write-report":
                    if (i + 1 >= args.Length || string.IsNullOrWhiteSpace(args[i + 1]))
                    {
                        throw new ArgumentException("--write-report requires a non-empty path argument.");
                    }

                    reportPath = args[++i];
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return new ProbeOptions(assert, reportPath);
    }

    private static IEnumerable<ProbeResult> RunProbe()
    {
        foreach (Fixture fixture in Fixtures())
        {
            ProbeResult result;
            try
            {
                AlbumData albumData = fixture.CreateAlbumData();
                FixtureInput input = FixtureInput.From(albumData, fixture.ProviderParserCallsParseReleaseDate);

                if (fixture.ProviderParserCallsParseReleaseDate)
                {
                    albumData.ParseReleaseDate();
                }

                ReleaseInfo release = albumData.ToReleaseInfo();
                ParsedAlbumInfo? parsed = Parser.ParseAlbumTitle(release.Title);

                result = new ProbeResult(fixture, input, release.Title, parsed, null);
            }
            catch (Exception ex)
            {
                result = new ProbeResult(fixture, null, null, null, $"{ex.GetType().Name}: {ex.Message}");
            }

            yield return result;
        }
    }

    private static IEnumerable<Fixture> Fixtures()
    {
        yield return RequiredProviderFixture(
            Provider: "Qobuz",
            SourceParserFile: "Tubifarry/Indexers/DABMusic/DABMusicParser.cs",
            CreateAlbumData: () => new AlbumData("DABMusic", "QobuzDownloadProtocol")
            {
                AlbumId = "https://www.qobuz.com/us-en/album/blackstar-david-bowie/0825646507389",
                AlbumName = "Blackstar",
                ArtistName = "David Bowie",
                InfoUrl = "https://www.qobuz.com/us-en/album/blackstar-david-bowie/0825646507389",
                TotalTracks = 7,
                ReleaseDate = "2016-01-08",
                ReleaseDatePrecision = "day",
                CustomString = "https://static.qobuz.com/images/covers/89/73/0825646507389_600.jpg",
                Codec = AudioFormat.FLAC,
                Bitrate = 1411,
                BitDepth = 24,
                Size = 444_000_000
            });

        yield return RequiredProviderFixture(
            Provider: "Deezer",
            SourceParserFile: "Tubifarry/Indexers/Lucida/LucidaRequestParser.cs; Tubifarry/Indexers/Lucida/LucidaServiceHelper.cs",
            CreateAlbumData: () => new AlbumData("Lucida", "LucidaDownloadProtocol")
            {
                AlbumId = "https://www.deezer.com/album/302127",
                AlbumName = "Discovery",
                ArtistName = "Daft Punk",
                InfoUrl = "https://lucida.example/?url=https://www.deezer.com/album/302127",
                TotalTracks = 14,
                ReleaseDate = "2001-03-12",
                ReleaseDatePrecision = "day",
                CustomString = "album",
                Codec = AudioFormat.MP3,
                Bitrate = 320,
                BitDepth = 0
            });

        yield return RequiredProviderFixture(
            Provider: "Amazon Music",
            SourceParserFile: "Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs",
            CreateAlbumData: () => new AlbumData("TripleTriple", "AmazonMusicDownloadProtocol")
            {
                AlbumId = "album/B08J4H2H13",
                AlbumName = "folklore (deluxe version)",
                ArtistName = "Taylor Swift",
                InfoUrl = "https://music.amazon.com/albums/B08J4H2H13",
                TotalTracks = 17,
                ReleaseDate = "2020-08-18",
                ReleaseDatePrecision = "day",
                CustomString = "https://m.media-amazon.com/images/I/example.jpg",
                Codec = AudioFormat.FLAC,
                Bitrate = 1411,
                BitDepth = 0,
                Size = 612_000_000
            });

        yield return RequiredProviderFixture(
            Provider: "SubSonic",
            SourceParserFile: "Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs",
            CreateAlbumData: () => new AlbumData("SubSonic", "SubSonicDownloadProtocol")
            {
                AlbumId = "https://subsonic.example/album/alb-001",
                AlbumName = "Random Access Memories",
                ArtistName = "Daft Punk",
                InfoUrl = "https://subsonic.example/rest/browse?type=album&id=alb-001",
                TotalTracks = 13,
                ReleaseDate = "2013",
                ReleaseDatePrecision = "year",
                CustomString = "cover-art-id",
                Codec = AudioFormat.FLAC,
                Bitrate = 1000,
                BitDepth = 16,
                Size = 536_000_000
            });

        yield return RequiredProviderFixture(
            Provider: "YouTube",
            SourceParserFile: "Tubifarry/Indexers/YouTube/YoutubeParser.cs",
            CreateAlbumData: () => new AlbumData("Youtube", "YoutubeDownloadProtocol")
            {
                AlbumId = "OLAK5uy_example",
                AlbumName = "Golden Hour feat. Willow",
                ArtistName = "Kacey Musgraves",
                InfoUrl = "https://music.youtube.com/playlist?list=OLAK5uy_example",
                TotalTracks = 13,
                ReleaseDate = "2018",
                ReleaseDatePrecision = "year",
                CustomString = "https://yt3.ggpht.com/example=s544-c",
                CoverResolution = "544x544",
                ExplicitContent = true,
                Codec = AudioFormat.AAC,
                Bitrate = 128,
                BitDepth = 0,
                Duration = 2_720
            });

        yield return MissingMetadataFixture(
            Provider: "Qobuz",
            SourceParserFile: "Tubifarry/Indexers/DABMusic/DABMusicParser.cs",
            CreateAlbumData: () => new AlbumData("DABMusic", "QobuzDownloadProtocol")
            {
                AlbumId = "qobuz/missing-date",
                AlbumName = "No Date Qobuz Album",
                ArtistName = "Qobuz Fixture Artist",
                InfoUrl = "https://qobuz.example/missing-date",
                TotalTracks = 9,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "missing-date",
                Codec = AudioFormat.FLAC,
                Bitrate = 1411,
                BitDepth = 24,
                Size = 444_000_000
            });

        yield return MissingMetadataFixture(
            Provider: "Deezer",
            SourceParserFile: "Tubifarry/Indexers/Lucida/LucidaRequestParser.cs; Tubifarry/Indexers/Lucida/LucidaServiceHelper.cs",
            CreateAlbumData: () => new AlbumData("Lucida", "LucidaDownloadProtocol")
            {
                AlbumId = "deezer/missing-date",
                AlbumName = "No Date Deezer Album",
                ArtistName = "Deezer Fixture Artist",
                InfoUrl = "https://deezer.example/missing-date",
                TotalTracks = 10,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "album",
                Codec = AudioFormat.MP3,
                Bitrate = 320,
                BitDepth = 0
            });

        yield return MissingMetadataFixture(
            Provider: "Amazon Music",
            SourceParserFile: "Tubifarry/Indexers/TripleTriple/TripleTripleParser.cs",
            CreateAlbumData: () => new AlbumData("TripleTriple", "AmazonMusicDownloadProtocol")
            {
                AlbumId = "amazon/missing-date",
                AlbumName = "No Date Amazon Album",
                ArtistName = "Amazon Fixture Artist",
                InfoUrl = "https://music.amazon.example/missing-date",
                TotalTracks = 11,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "https://m.media-amazon.com/images/I/example.jpg",
                Codec = AudioFormat.FLAC,
                Bitrate = 1411,
                BitDepth = 0,
                Size = 612_000_000
            });

        yield return MissingMetadataFixture(
            Provider: "SubSonic",
            SourceParserFile: "Tubifarry/Indexers/SubSonic/SubSonicIndexerParser.cs",
            CreateAlbumData: () => new AlbumData("SubSonic", "SubSonicDownloadProtocol")
            {
                AlbumId = "subsonic/missing-date",
                AlbumName = "No Date SubSonic Album",
                ArtistName = "SubSonic Fixture Artist",
                InfoUrl = "https://subsonic.example/rest/browse?type=album&id=missing-date",
                TotalTracks = 12,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "cover-art-id",
                Codec = AudioFormat.FLAC,
                Bitrate = 1000,
                BitDepth = 16,
                Size = 536_000_000
            });

        yield return MissingMetadataFixture(
            Provider: "YouTube",
            SourceParserFile: "Tubifarry/Indexers/YouTube/YoutubeParser.cs",
            CreateAlbumData: () => new AlbumData("Youtube", "YoutubeDownloadProtocol")
            {
                AlbumId = "youtube/missing-date",
                AlbumName = "No Date YouTube Album",
                ArtistName = "YouTube Fixture Artist",
                InfoUrl = "https://music.youtube.com/playlist?list=missing-date",
                TotalTracks = 13,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "https://yt3.ggpht.com/example=s544-c",
                CoverResolution = "544x544",
                ExplicitContent = true,
                Codec = AudioFormat.AAC,
                Bitrate = 128,
                BitDepth = 0,
                Duration = 2_720
            });

        yield return new Fixture(
            Provider: "Boundary",
            Scenario: "non-featuring parentheses",
            SourceParserFile: "Tubifarry/Core/Model/AlbumData.cs",
            ProviderParserCallsParseReleaseDate: true,
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/non-featuring-parentheses",
                AlbumName = "Blue Train (Remastered) ft. Lee Morgan",
                ArtistName = "John Coltrane",
                InfoUrl = "https://example.invalid/boundary/non-featuring-parentheses",
                TotalTracks = 5,
                ReleaseDate = "1958",
                ReleaseDatePrecision = "year",
                CustomString = "boundary",
                Codec = AudioFormat.FLAC,
                Bitrate = 1000,
                BitDepth = 24,
                Size = 320_000_000
            });

        yield return new Fixture(
            Provider: "Boundary Unknown Date",
            Scenario: "blank date and precision",
            SourceParserFile: "Tubifarry/Core/Model/AlbumData.cs",
            ProviderParserCallsParseReleaseDate: true,
            IsMissingMetadata: true,
            AssertNoPlaceholderTokens: true,
            AssertCodecOnlyQuality: true,
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/unknown-date",
                AlbumName = "No Date Needed",
                ArtistName = "Formatter Seam",
                InfoUrl = "https://example.invalid/boundary/unknown-date",
                TotalTracks = 1,
                ReleaseDate = " ",
                ReleaseDatePrecision = string.Empty,
                CustomString = "boundary",
                Codec = AudioFormat.FLAC,
                Bitrate = 0,
                BitDepth = 0,
                Size = 42_000_000
            });

        yield return new Fixture(
            Provider: "Boundary Empty Precision",
            Scenario: "date with empty precision",
            SourceParserFile: "Tubifarry/Core/Model/AlbumData.cs",
            ProviderParserCallsParseReleaseDate: true,
            IsMissingMetadata: true,
            AssertNoPlaceholderTokens: true,
            AssertCodecOnlyQuality: true,
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/empty-precision",
                AlbumName = "Date Without Precision",
                ArtistName = "Formatter Seam",
                InfoUrl = "https://example.invalid/boundary/empty-precision",
                TotalTracks = 1,
                ReleaseDate = "2024",
                ReleaseDatePrecision = string.Empty,
                CustomString = "boundary",
                Codec = AudioFormat.FLAC,
                Bitrate = 0,
                BitDepth = 0,
                Size = 42_000_000
            });

        yield return new Fixture(
            Provider: "Boundary Missing Optional Quality",
            Scenario: "codec-only quality",
            SourceParserFile: "Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs",
            ProviderParserCallsParseReleaseDate: true,
            AssertCodecOnlyQuality: true,
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/missing-optional-quality",
                AlbumName = "Codec Only Quality",
                ArtistName = "Formatter Seam",
                InfoUrl = "https://example.invalid/boundary/missing-optional-quality",
                TotalTracks = 1,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "boundary",
                Codec = AudioFormat.MP3,
                Bitrate = 0,
                BitDepth = 0,
                Size = null,
                Duration = 0
            });

        yield return new Fixture(
            Provider: "Boundary Missing Artist",
            Scenario: "required artist validation",
            SourceParserFile: "Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs",
            ProviderParserCallsParseReleaseDate: true,
            ExpectedFixtureErrorContains: "ArtistName is required",
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/missing-artist",
                AlbumName = "Missing Required Artist",
                ArtistName = string.Empty,
                InfoUrl = "https://example.invalid/boundary/missing-artist",
                TotalTracks = 1,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "boundary",
                Codec = AudioFormat.FLAC,
                Bitrate = 0,
                BitDepth = 0,
                Size = 42_000_000
            });

        yield return new Fixture(
            Provider: "Boundary Missing Album",
            Scenario: "required album validation",
            SourceParserFile: "Tubifarry/Core/Utilities/ReleaseTitleFormatter.cs",
            ProviderParserCallsParseReleaseDate: true,
            ExpectedFixtureErrorContains: "AlbumName is required",
            CreateAlbumData: () => new AlbumData("ProbeBoundary", "QobuzDownloadProtocol")
            {
                AlbumId = "boundary/missing-album",
                AlbumName = string.Empty,
                ArtistName = "Formatter Seam",
                InfoUrl = "https://example.invalid/boundary/missing-album",
                TotalTracks = 1,
                ReleaseDate = string.Empty,
                ReleaseDatePrecision = string.Empty,
                CustomString = "boundary",
                Codec = AudioFormat.FLAC,
                Bitrate = 0,
                BitDepth = 0,
                Size = 42_000_000
            });
    }

    private static Fixture RequiredProviderFixture(string Provider, string SourceParserFile, Func<AlbumData> CreateAlbumData) =>
        new(
            Provider: Provider,
            Scenario: "provider baseline",
            SourceParserFile: SourceParserFile,
            ProviderParserCallsParseReleaseDate: true,
            CreateAlbumData: CreateAlbumData,
            IsRequiredProviderFixture: true);

    private static Fixture MissingMetadataFixture(string Provider, string SourceParserFile, Func<AlbumData> CreateAlbumData) =>
        new(
            Provider: Provider,
            Scenario: "missing metadata",
            SourceParserFile: SourceParserFile,
            ProviderParserCallsParseReleaseDate: true,
            CreateAlbumData: CreateAlbumData,
            IsRequiredProviderFixture: true,
            IsMissingMetadata: true,
            AssertNoPlaceholderTokens: true);

    private static IEnumerable<AssertionFailure> EvaluateAssertions(IReadOnlyCollection<ProbeResult> results)
    {
        foreach (string provider in RequiredProviders)
        {
            if (!results.Any(result => result.Fixture.IsRequiredProviderFixture && result.Fixture.Provider == provider && result.Fixture.Scenario == "provider baseline"))
            {
                yield return new AssertionFailure(provider, "coverage", "Required provider baseline fixture is missing.");
            }

            if (!results.Any(result => result.Fixture.IsRequiredProviderFixture && result.Fixture.Provider == provider && result.Fixture.IsMissingMetadata))
            {
                yield return new AssertionFailure(provider, "coverage", "Required provider missing-metadata fixture is missing.");
            }
        }

        foreach (ProbeResult result in results.Where(result => result.Fixture.IsRequiredProviderFixture))
        {
            if (result.FixtureError is not null)
            {
                yield return new AssertionFailure(result.DisplayName, "fixture", $"Required provider fixture error: {result.FixtureError}");
                continue;
            }

            if (result.Parsed is null)
            {
                yield return new AssertionFailure(result.DisplayName, "parser", $"Parser.ParseAlbumTitle returned null for: {result.EmittedTitle}");
            }
        }

        foreach (ProbeResult result in results.Where(result => result.Fixture.AssertNoPlaceholderTokens))
        {
            if (result.FixtureError is not null)
            {
                continue;
            }

            foreach (string regression in PlaceholderRegressions(result.EmittedTitle ?? string.Empty))
            {
                yield return new AssertionFailure(result.DisplayName, "placeholder", regression);
            }
        }

        foreach (ProbeResult result in results.Where(result => result.Fixture.AssertCodecOnlyQuality))
        {
            if (result.FixtureError is not null || result.Input is null)
            {
                continue;
            }

            string expectedQuality = $"[{result.Input.Codec}]";
            string emittedTitle = result.EmittedTitle ?? string.Empty;
            if (!emittedTitle.Contains(expectedQuality, StringComparison.Ordinal))
            {
                yield return new AssertionFailure(result.DisplayName, "quality", $"Expected codec-only quality token {expectedQuality} in: {emittedTitle}");
            }

            if (QualityDetailRegex().IsMatch(emittedTitle))
            {
                yield return new AssertionFailure(result.DisplayName, "quality", $"Missing optional quality fields produced fabricated bitrate/bit-depth details in: {emittedTitle}");
            }
        }
    }

    private static IEnumerable<string> PlaceholderRegressions(string emittedTitle)
    {
        foreach (string token in PlaceholderTokens)
        {
            if (emittedTitle.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                yield return $"Emitted title contains placeholder token '{token}': {emittedTitle}";
            }
        }

        if (emittedTitle.Contains($"({DateTime.UtcNow.Year})", StringComparison.Ordinal))
        {
            yield return $"Missing-date fixture emitted the current year: {emittedTitle}";
        }

        if (YearParenthesesRegex().IsMatch(emittedTitle))
        {
            yield return $"Missing-date fixture emitted a release year/date token: {emittedTitle}";
        }
    }

    private static void PrintResults(IReadOnlyCollection<ProbeResult> results)
    {
        Console.WriteLine("Tubifarry release title probe");
        Console.WriteLine("Fixture                       | Source parser                                      | Emitted title                                                                                          | Parser | Artist              | Album                         | Year | Quality");
        Console.WriteLine(new string('-', 230));

        foreach (ProbeResult result in results)
        {
            PrintRow(
                result.DisplayName,
                result.Fixture.SourceParserFile,
                result.EmittedTitle ?? $"fixture error: {result.FixtureError}",
                result.Verdict,
                result.Parsed?.ArtistName,
                result.Parsed?.AlbumTitle,
                result.Parsed?.ReleaseDate,
                result.Parsed?.Quality?.ToString());
        }
    }

    private static void PrintAssertions(IReadOnlyCollection<AssertionFailure> assertionFailures)
    {
        Console.WriteLine();
        Console.WriteLine("Assertion mode");
        if (assertionFailures.Count == 0)
        {
            Console.WriteLine("PASS: required provider coverage, parser verdicts, missing-metadata placeholders, and codec-only quality boundaries all passed.");
            return;
        }

        Console.WriteLine("FAIL:");
        foreach (AssertionFailure failure in assertionFailures)
        {
            Console.WriteLine($"- [{failure.Category}] {failure.Fixture}: {failure.Message}");
        }
    }

    private static void WriteReport(string reportPath, IReadOnlyCollection<ProbeResult> results, IReadOnlyCollection<AssertionFailure> assertionFailures, bool assertionMode)
    {
        string? directory = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(reportPath, BuildReport(results, assertionFailures, assertionMode), Encoding.UTF8);
    }

    private static string BuildReport(IReadOnlyCollection<ProbeResult> results, IReadOnlyCollection<AssertionFailure> assertionFailures, bool assertionMode)
    {
        StringBuilder report = new();
        List<string> coveredProviders = results
            .Where(result => result.Fixture.IsRequiredProviderFixture && result.Fixture.Scenario == "provider baseline")
            .Select(result => result.Fixture.Provider)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(provider => Array.IndexOf(RequiredProviders, provider))
            .ToList();
        List<string> missingProviders = RequiredProviders.Except(coveredProviders, StringComparer.Ordinal).ToList();
        List<string> missingMetadataProviders = RequiredProviders
            .Where(provider => !results.Any(result => result.Fixture.IsRequiredProviderFixture && result.Fixture.Provider == provider && result.Fixture.IsMissingMetadata))
            .ToList();
        List<ProbeResult> providerResults = results.Where(result => result.Fixture.IsRequiredProviderFixture).ToList();
        List<ProbeResult> nullResults = providerResults.Where(result => result.FixtureError is null && result.Parsed is null).ToList();
        List<ProbeResult> fixtureErrors = results.Where(result => result.UnexpectedFixtureError is not null).ToList();
        List<string> placeholderRegressions = results
            .Where(result => result.Fixture.AssertNoPlaceholderTokens && result.FixtureError is null)
            .SelectMany(result => PlaceholderRegressions(result.EmittedTitle ?? string.Empty).Select(regression => $"{result.DisplayName}: {regression}"))
            .ToList();
        List<string> qualityBoundaryRegressions = assertionFailures
            .Where(failure => failure.Category == "quality")
            .Select(failure => $"{failure.Fixture}: {failure.Message}")
            .ToList();

        report.AppendLine("# Release Title Parser Investigation");
        report.AppendLine();
        report.AppendLine($"- Date: {DateTime.UtcNow:yyyy-MM-dd} UTC");
        report.AppendLine("- Scope: Offline probe of Tubifarry `AlbumData.ToReleaseInfo()` title output against Lidarr `Parser.ParseAlbumTitle`.");
        report.AppendLine("- Parser boundary: Each fixture constructs representative provider `AlbumData`, applies the same release-date parse step used by provider parsers when indicated, emits `ReleaseInfo.Title`, then calls Lidarr's real `Parser.ParseAlbumTitle`.");
        report.AppendLine("- Harness behavior: Default mode reports evidence and exits non-zero for unexpected fixture construction/report write failures. `--assert` additionally exits non-zero for missing required provider coverage, required fixture errors, parser-null required provider titles, missing-metadata placeholder/date regressions, and codec-only quality-boundary regressions.");
        report.AppendLine($"- Last assertion run represented in this report: `{assertionMode}`; assertion verdict: `{(assertionFailures.Count == 0 ? "PASS" : "FAIL")}`.");
        report.AppendLine();

        report.AppendLine("## Provider Coverage");
        report.AppendLine();
        report.AppendLine("| Provider | Baseline covered? | Missing-metadata covered? | Source parser boundary | Fixture status | Parser verdict | Placeholder verdict |");
        report.AppendLine("|---|---:|---:|---|---|---|---|");
        foreach (string provider in RequiredProviders)
        {
            List<ProbeResult> providerFixtures = providerResults.Where(item => item.Fixture.Provider == provider).ToList();
            ProbeResult? baseline = providerFixtures.FirstOrDefault(item => item.Fixture.Scenario == "provider baseline");
            bool hasMissingMetadata = providerFixtures.Any(item => item.Fixture.IsMissingMetadata);
            if (baseline is null)
            {
                report.AppendLine($"| {Escape(provider)} | No | {(hasMissingMetadata ? "Yes" : "No")} | - | Missing fixture | Not run | Not run |");
                continue;
            }

            string fixtureStatus = providerFixtures.All(result => result.FixtureError is null)
                ? "Constructed"
                : string.Join("; ", providerFixtures.Where(result => result.FixtureError is not null).Select(result => $"{result.DisplayName}: {result.FixtureError}"));
            string parserVerdict = providerFixtures.All(result => result.FixtureError is null && result.Parsed is not null) ? "OK" : "FAIL";
            string placeholderVerdict = providerFixtures
                .Where(result => result.Fixture.AssertNoPlaceholderTokens && result.FixtureError is null)
                .SelectMany(result => PlaceholderRegressions(result.EmittedTitle ?? string.Empty))
                .Any()
                ? "FAIL"
                : "OK";
            report.AppendLine($"| {Escape(provider)} | Yes | {(hasMissingMetadata ? "Yes" : "No")} | `{Escape(baseline.Fixture.SourceParserFile)}` | {Escape(fixtureStatus)} | {Escape(parserVerdict)} | {Escape(placeholderVerdict)} |");
        }
        report.AppendLine();

        report.AppendLine("## Fixture Results");
        report.AppendLine();
        foreach (ProbeResult result in results)
        {
            report.AppendLine($"### {Escape(result.DisplayName)}");
            report.AppendLine();
            report.AppendLine($"- Source parser boundary: `{Escape(result.Fixture.SourceParserFile)}`");
            report.AppendLine($"- Scenario: `{Escape(result.Fixture.Scenario)}`");
            report.AppendLine($"- Required provider assertion fixture: `{result.Fixture.IsRequiredProviderFixture}`");
            report.AppendLine($"- Missing-metadata placeholder assertion: `{result.Fixture.AssertNoPlaceholderTokens}`");
            report.AppendLine($"- Codec-only quality assertion: `{result.Fixture.AssertCodecOnlyQuality}`");
            report.AppendLine($"- Provider parser calls `AlbumData.ParseReleaseDate()`: `{result.Fixture.ProviderParserCallsParseReleaseDate}`");

            if (result.Input is not null)
            {
                report.AppendLine("- Fixture inputs:");
                report.AppendLine($"  - ArtistName: `{Escape(result.Input.ArtistName)}`");
                report.AppendLine($"  - AlbumName: `{Escape(result.Input.AlbumName)}`");
                report.AppendLine($"  - ReleaseDate / Precision: `{Escape(result.Input.ReleaseDate)}` / `{Escape(result.Input.ReleaseDatePrecision)}`");
                report.AppendLine($"  - Codec / Bitrate / BitDepth: `{Escape(result.Input.Codec)}` / `{result.Input.Bitrate}` / `{result.Input.BitDepth}`");
                report.AppendLine($"  - ExplicitContent: `{result.Input.ExplicitContent}`");
                report.AppendLine($"  - TotalTracks / Duration / Size: `{result.Input.TotalTracks}` / `{result.Input.Duration}` / `{FormatNullable(result.Input.Size)}`");
            }

            if (result.FixtureError is not null)
            {
                report.AppendLine($"- Fixture construction: Failed: `{Escape(result.FixtureError)}`");
                report.AppendLine("- Emitted `ReleaseInfo.Title`: Not emitted");
                report.AppendLine("- `Parser.ParseAlbumTitle` verdict: Not run");
                report.AppendLine("- Failure detail: Fixture could not be constructed or emitted, so no parser evidence exists for this fixture.");
                report.AppendLine();
                continue;
            }

            report.AppendLine($"- Emitted `ReleaseInfo.Title`: `{Escape(result.EmittedTitle ?? string.Empty)}`");
            string placeholderVerdict = result.Fixture.AssertNoPlaceholderTokens
                ? PlaceholderRegressions(result.EmittedTitle ?? string.Empty).Any() ? "FAIL" : "OK"
                : "Not asserted";
            report.AppendLine($"- Placeholder regression verdict: `{placeholderVerdict}`");
            report.AppendLine($"- `Parser.ParseAlbumTitle` verdict: **{Escape(result.Verdict)}**");

            if (result.Parsed is null)
            {
                report.AppendLine("- Failure detail: Lidarr parser returned `null` for the emitted title.");
            }
            else
            {
                report.AppendLine("- Parsed fields:");
                report.AppendLine($"  - ArtistName: `{Escape(result.Parsed.ArtistName ?? string.Empty)}`");
                report.AppendLine($"  - AlbumTitle: `{Escape(result.Parsed.AlbumTitle ?? string.Empty)}`");
                report.AppendLine($"  - ReleaseDate: `{Escape(result.Parsed.ReleaseDate ?? string.Empty)}`");
                report.AppendLine($"  - Quality: `{Escape(result.Parsed.Quality?.ToString() ?? string.Empty)}`");
                report.AppendLine($"  - ReleaseGroup: `{Escape(result.Parsed.ReleaseGroup ?? string.Empty)}`");
                report.AppendLine($"  - ReleaseHash: `{Escape(result.Parsed.ReleaseHash ?? string.Empty)}`");
                report.AppendLine($"  - ReleaseVersion: `{Escape(result.Parsed.ReleaseVersion ?? string.Empty)}`");
            }

            report.AppendLine();
        }

        report.AppendLine("## Negative Evidence Checklist");
        report.AppendLine();
        AppendList(report, "Required provider baseline fixtures not covered", missingProviders);
        AppendList(report, "Required provider missing-metadata fixtures not covered", missingMetadataProviders);
        AppendList(report, "Fixtures with unexpected construction errors", fixtureErrors.Select(result => $"{result.DisplayName}: {result.UnexpectedFixtureError}"));
        AppendList(report, "Parser null results for required provider fixtures", nullResults.Select(result => $"{result.DisplayName}: {result.EmittedTitle}"));
        AppendList(report, "Missing-metadata placeholder/fake-date regressions", placeholderRegressions);
        AppendList(report, "Missing optional quality regressions", qualityBoundaryRegressions);
        report.AppendLine();

        List<string> fieldFidelityNotes = providerResults
            .Where(result => result.Input is not null && result.Parsed is not null)
            .SelectMany(GetFieldFidelityNotes)
            .ToList();
        report.AppendLine("## Field Fidelity Notes");
        report.AppendLine();
        if (fieldFidelityNotes.Count == 0)
        {
            report.AppendLine("- None. Parsed provider fields matched the fixture inputs exactly for all parser-success fixtures.");
        }
        else
        {
            foreach (string note in fieldFidelityNotes)
            {
                report.AppendLine($"- {Escape(note)}");
            }
        }
        report.AppendLine();

        report.AppendLine("## Recommendation for S02");
        report.AppendLine();
        if (assertionFailures.Count > 0)
        {
            report.AppendLine("Do not complete S02 yet. The assertion harness found required coverage, parser, placeholder, or quality-boundary failures listed above.");
        }
        else if (fixtureErrors.Count > 0 || missingProviders.Count > 0 || missingMetadataProviders.Count > 0)
        {
            report.AppendLine("Do not change production formatting yet. First repair fixture/provider evidence gaps so S02 is based on complete data rather than inferred parser compatibility.");
        }
        else if (providerResults.All(result => result.Parsed is not null))
        {
            report.AppendLine("The current `AlbumData` formatter emits titles that Lidarr's real parser accepts for all five required provider baseline and missing-metadata fixtures. Missing-date provider fixtures omit year/date placeholders and placeholder tokens while still parsing, so S02 has executable regression protection for R004. Use `dotnet run --project tools/ReleaseTitleProbe/ReleaseTitleProbe.csproj -- --assert --write-report docs/release-title-parser-investigation.md` as the slice verification command.");
        }
        else
        {
            report.AppendLine("At least one required provider fixture emits a title that Lidarr's real parser returns `null` for. S02 should treat those as formatter compatibility problems for the affected title shape only, while leaving successfully parsed provider-field mappings intact. Use the null-result titles above as failing regression fixtures and the successful titles as guardrails.");
        }

        if (results.Any(result => result.Fixture.Provider == "Boundary"))
        {
            report.AppendLine();
            report.AppendLine("The additional `Boundary` fixture is not provider coverage. It is a formatter boundary check for non-featuring parentheses plus `ft.` normalization so formatter changes can avoid regressing album-title normalization behavior.");
        }

        return report.ToString();
    }

    private static void AppendList(StringBuilder report, string heading, IEnumerable<string> values)
    {
        List<string> materialized = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        report.AppendLine($"### {heading}");
        report.AppendLine();
        if (materialized.Count == 0)
        {
            report.AppendLine("- None.");
        }
        else
        {
            foreach (string value in materialized)
            {
                report.AppendLine($"- {Escape(value)}");
            }
        }

        report.AppendLine();
    }

    private static IEnumerable<string> GetFieldFidelityNotes(ProbeResult result)
    {
        if (result.Input is null || result.Parsed is null)
        {
            yield break;
        }

        if (!string.Equals(result.Input.ArtistName, result.Parsed.ArtistName, StringComparison.Ordinal))
        {
            yield return $"{result.DisplayName}: parsed artist differs from fixture artist (`{result.Parsed.ArtistName}` vs `{result.Input.ArtistName}`).";
        }

        if (!string.Equals(result.Input.AlbumName, result.Parsed.AlbumTitle, StringComparison.Ordinal))
        {
            yield return $"{result.DisplayName}: parsed album differs from fixture album (`{result.Parsed.AlbumTitle}` vs `{result.Input.AlbumName}`).";
        }

        string expectedYear = result.Input.ReleaseDate.Length >= 4 && !string.IsNullOrWhiteSpace(result.Input.ReleaseDatePrecision)
            ? result.Input.ReleaseDate[..4]
            : "0";
        if (!string.Equals(expectedYear, result.Parsed.ReleaseDate, StringComparison.Ordinal))
        {
            yield return $"{result.DisplayName}: parsed release year differs from fixture release year (`{result.Parsed.ReleaseDate}` vs `{expectedYear}`).";
        }
    }

    private static void PrintRow(
        string provider,
        string sourceParserFile,
        string emittedTitle,
        string parserVerdict,
        string? artist,
        string? album,
        string? year,
        string? quality)
    {
        Console.WriteLine(
            string.Join(" | ",
                Truncate(provider, 29).PadRight(29),
                Truncate(sourceParserFile, 50).PadRight(50),
                Truncate(emittedTitle, 102).PadRight(102),
                parserVerdict.PadRight(6),
                Truncate(artist ?? "-", 19).PadRight(19),
                Truncate(album ?? "-", 29).PadRight(29),
                Truncate(year ?? "-", 4).PadRight(4),
                quality ?? "-"));
    }

    private static string Escape(string value) => value.Replace("|", "\\|").Replace("`", "\\`");

    private static string FormatNullable(long? value) => value.HasValue ? value.Value.ToString() : "null";

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, Math.Max(0, maxLength - 1)), "…");

    [GeneratedRegex(@"\(\d{4}\)")]
    private static partial Regex YearParenthesesRegex();

    [GeneratedRegex(@"\[(?:[^\]]*\b(?:\d+kbps|\d+bit)\b[^\]]*)\]", RegexOptions.IgnoreCase, "de-DE")]
    private static partial Regex QualityDetailRegex();

    private sealed record ProbeOptions(bool Assert, string? ReportPath);

    private sealed record AssertionFailure(string Fixture, string Category, string Message);

    private sealed record Fixture(
        string Provider,
        string Scenario,
        string SourceParserFile,
        bool ProviderParserCallsParseReleaseDate,
        Func<AlbumData> CreateAlbumData,
        string? ExpectedFixtureErrorContains = null,
        bool IsRequiredProviderFixture = false,
        bool IsMissingMetadata = false,
        bool AssertNoPlaceholderTokens = false,
        bool AssertCodecOnlyQuality = false);

    private sealed record ProbeResult(
        Fixture Fixture,
        FixtureInput? Input,
        string? EmittedTitle,
        ParsedAlbumInfo? Parsed,
        string? FixtureError)
    {
        public bool IsExpectedFixtureError => FixtureError is not null
            && Fixture.ExpectedFixtureErrorContains is not null
            && FixtureError.Contains(Fixture.ExpectedFixtureErrorContains, StringComparison.Ordinal);

        public string? UnexpectedFixtureError => FixtureError is not null && !IsExpectedFixtureError ? FixtureError : null;

        public string Verdict => FixtureError is not null
            ? IsExpectedFixtureError ? "EXPECTED ERROR" : "ERROR"
            : Parsed is null ? "FAIL" : "OK";

        public string DisplayName => Fixture.Scenario == "provider baseline"
            ? Fixture.Provider
            : $"{Fixture.Provider} ({Fixture.Scenario})";
    }

    private sealed record FixtureInput(
        string AlbumId,
        string AlbumName,
        string ArtistName,
        string ReleaseDate,
        string ReleaseDatePrecision,
        string Codec,
        int Bitrate,
        int BitDepth,
        int TotalTracks,
        long Duration,
        long? Size,
        bool ExplicitContent)
    {
        public static FixtureInput From(AlbumData albumData, bool providerParserCallsParseReleaseDate) => new(
            albumData.AlbumId,
            albumData.AlbumName,
            albumData.ArtistName,
            albumData.ReleaseDate,
            providerParserCallsParseReleaseDate ? albumData.ReleaseDatePrecision : $"{albumData.ReleaseDatePrecision} (not parsed)",
            albumData.Codec.ToString(),
            albumData.Bitrate,
            albumData.BitDepth,
            albumData.TotalTracks,
            albumData.Duration,
            albumData.Size,
            albumData.ExplicitContent);
    }
}
