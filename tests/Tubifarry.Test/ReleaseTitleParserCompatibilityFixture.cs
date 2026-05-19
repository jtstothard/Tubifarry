using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Tubifarry.Core.Model;
using Tubifarry.Core.Utilities;

namespace Tubifarry.Test;

[TestFixture]
public partial class ReleaseTitleParserCompatibilityFixture
{
    [TestCaseSource(nameof(ProviderBaselineFixtures))]
    public void Provider_release_titles_parse_through_lidarr_parser(Fixture fixture)
    {
        AlbumData albumData = fixture.CreateAlbumData();

        albumData.ParseReleaseDate();
        ReleaseInfo release = albumData.ToReleaseInfo();
        ParsedAlbumInfo? parsed = Parser.ParseAlbumTitle(release.Title);

        parsed.Should().NotBeNull($"Lidarr should parse the emitted {fixture.Provider} title: {release.Title}");
    }

    [TestCaseSource(nameof(MissingMetadataFixtures))]
    public void Missing_metadata_release_titles_do_not_fabricate_placeholders_or_dates_and_still_parse(Fixture fixture)
    {
        AlbumData albumData = fixture.CreateAlbumData();

        albumData.ParseReleaseDate();
        ReleaseInfo release = albumData.ToReleaseInfo();

        release.Title.Should().NotContain("Unknown", $"{fixture.Provider} missing metadata must not fabricate placeholder text");
        release.Title.Should().NotContain("0000", $"{fixture.Provider} missing metadata must not fabricate zero dates");
        release.Title.Should().NotContain($"({DateTime.UtcNow.Year})", $"{fixture.Provider} missing metadata must not fall back to the current UTC year");
        YearParenthesesRegex().IsMatch(release.Title).Should().BeFalse($"{fixture.Provider} missing metadata must not emit any parenthesized four-digit release year in: {release.Title}");

        ParsedAlbumInfo? parsed = Parser.ParseAlbumTitle(release.Title);
        parsed.Should().NotBeNull($"Lidarr should parse the emitted {fixture.Provider} missing-metadata title: {release.Title}");
    }

    private static IEnumerable<Fixture> ProviderBaselineFixtures()
    {
        yield return new Fixture(
            Provider: "Qobuz",
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

        yield return new Fixture(
            Provider: "Deezer",
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

        yield return new Fixture(
            Provider: "Amazon Music",
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

        yield return new Fixture(
            Provider: "SubSonic",
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

        yield return new Fixture(
            Provider: "YouTube",
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
    }

    private static IEnumerable<Fixture> MissingMetadataFixtures()
    {
        yield return new Fixture(
            Provider: "Qobuz",
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

        yield return new Fixture(
            Provider: "Deezer",
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

        yield return new Fixture(
            Provider: "Amazon Music",
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

        yield return new Fixture(
            Provider: "SubSonic",
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

        yield return new Fixture(
            Provider: "YouTube",
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
    }

    [GeneratedRegex(@"\(\d{4}\)")]
    private static partial Regex YearParenthesesRegex();

    public sealed record Fixture(string Provider, Func<AlbumData> CreateAlbumData)
    {
        public override string ToString() => Provider;
    }
}
