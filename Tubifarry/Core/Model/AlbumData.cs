using NzbDrone.Core.Parser.Model;
using System.Globalization;
using Tubifarry.Core.Utilities;

namespace Tubifarry.Core.Model
{
    /// <summary>
    /// Contains combined information about an album, search parameters, and search results.
    /// </summary>
    public partial class AlbumData(string name, string downloadProtocol)
    {
        public string? Guid { get; set; }
        public string IndexerName { get; } = name;

        // Mixed
        public string AlbumId { get; set; } = string.Empty;

        // Properties from AlbumInfo
        public string AlbumName { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;
        public string InfoUrl { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public DateTime ReleaseDateTime { get; set; }
        public string ReleaseDatePrecision { get; set; } = string.Empty;
        public int TotalTracks { get; set; }
        public bool ExplicitContent { get; set; }
        public string CustomString { get; set; } = string.Empty;
        public string CoverResolution { get; set; } = string.Empty;

        // Properties from YoutubeSearchResults
        public int Bitrate { get; set; }

        public int BitDepth { get; set; }
        public long Duration { get; set; }

        // Soulseek
        public long? Size { get; set; }

        public int Priotity { get; set; }
        public List<string>? ExtraInfo { get; set; }

        public string DownloadProtocol { get; set; } = downloadProtocol;

        // Not used
        public AudioFormat Codec { get; set; } = AudioFormat.AAC;

        /// <summary>
        /// Converts AlbumData into a ReleaseInfo object.
        /// </summary>
        public ReleaseInfo ToReleaseInfo() => new()
        {
            Guid = Guid ?? $"{IndexerName}-{AlbumId}-{Codec}-{Bitrate}-{BitDepth}",
            Artist = ArtistName,
            Album = AlbumName,
            DownloadUrl = AlbumId,
            InfoUrl = InfoUrl,
            PublishDate = ReleaseDateTime == DateTime.MinValue ? DateTime.UtcNow : ReleaseDateTime,
            DownloadProtocol = DownloadProtocol,
            Title = ReleaseTitleFormatter.Format(this),
            Codec = Codec.ToString(),
            Resolution = CoverResolution,
            Source = CustomString,
            Container = Bitrate.ToString(),
            Size = Size ?? (Duration > 0 ? Duration : TotalTracks * 300) * Bitrate * 1000 / 8
        };

        /// <summary>
        /// Parses the release date based on the precision.
        /// </summary>
        public void ParseReleaseDate()
        {
            if (string.IsNullOrWhiteSpace(ReleaseDate) || string.IsNullOrWhiteSpace(ReleaseDatePrecision))
            {
                ReleaseDateTime = DateTime.MinValue;
                return;
            }

            ReleaseDateTime = ReleaseDatePrecision switch
            {
                "year" => new DateTime(int.Parse(ReleaseDate, CultureInfo.InvariantCulture), 1, 1),
                "month" => DateTime.ParseExact(ReleaseDate, "yyyy-MM", CultureInfo.InvariantCulture),
                "day" => DateTime.ParseExact(ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => throw new FormatException($"Unsupported release_date_precision: {ReleaseDatePrecision}"),
            };
        }
    }
}