using System.Text.RegularExpressions;
using NzbDrone.Core.Parser.Model;
using Tubifarry.Core.Model;

namespace Tubifarry.Core.Utilities
{
    /// <summary>
    /// Formats Tubifarry album metadata into Lidarr parser-compatible release titles.
    /// </summary>
    public static partial class ReleaseTitleFormatter
    {
        /// <summary>
        /// Constructs a title string for the album in a format optimized for Lidarr parsing.
        /// </summary>
        /// <param name="albumData">Album metadata to format.</param>
        /// <returns>A formatted title string.</returns>
        /// <exception cref="ArgumentException">Thrown when required artist or album data is missing.</exception>
        public static string Format(AlbumData albumData)
        {
            ArgumentNullException.ThrowIfNull(albumData);
            ValidateRequiredText(albumData.ArtistName, nameof(albumData.ArtistName));
            ValidateRequiredText(albumData.AlbumName, nameof(albumData.AlbumName));

            string normalizedAlbumName = NormalizeAlbumName(albumData.AlbumName);

            string title = $"{albumData.ArtistName} - {normalizedAlbumName}";

            if (albumData.ReleaseDateTime != DateTime.MinValue)
                title += $" ({albumData.ReleaseDateTime.Year})";

            if (albumData.ExplicitContent)
                title += " [Explicit]";

            title += FormatQuality(albumData);

            if (albumData.ExtraInfo?.Count > 0)
                title += string.Concat(albumData.ExtraInfo
                    .Where(info => !string.IsNullOrWhiteSpace(info))
                    .Select(info => $" [{info}]"));

            title += " [WEB]";
            return title;
        }

        private static void ValidateRequiredText(string? value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{propertyName} is required to format a release title.", propertyName);
        }

        private static string FormatQuality(AlbumData albumData)
        {
            int calculatedBitrate = albumData.Bitrate;
            if (calculatedBitrate <= 0 && albumData.Size.HasValue && albumData.Duration > 0)
                calculatedBitrate = (int)(albumData.Size.Value * 8 / (albumData.Duration * 1000));

            if (AudioFormatHelper.IsLossyFormat(albumData.Codec) && calculatedBitrate > 0)
                return $" [{albumData.Codec} {calculatedBitrate}kbps]";

            if (!AudioFormatHelper.IsLossyFormat(albumData.Codec) && albumData.BitDepth > 0)
                return $" [{albumData.Codec} {albumData.BitDepth}bit]";

            return $" [{albumData.Codec}]";
        }

        /// <summary>
        /// Normalizes the album name to handle featuring artists and other parentheses.
        /// </summary>
        /// <param name="albumName">The album name to normalize.</param>
        /// <returns>The normalized album name.</returns>
        private static string NormalizeAlbumName(string albumName)
        {
            if (FeatRegex().IsMatch(albumName))
            {
                Match match = FeatRegex().Match(albumName);
                string featuringArtist = albumName[(match.Index + match.Length)..].Trim();

                albumName = $"{albumName[..match.Index].Trim()} (feat. {featuringArtist})";
            }

            return FeatReplaceRegex().Replace(albumName, match => $"{{{match.Value.Trim('(', ')')}}}");
        }

        [GeneratedRegex(@"(?i)\b(feat\.|ft\.|featuring)\b", RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex FeatRegex();

        [GeneratedRegex(@"\((?!feat\.)[^)]*\)")]
        private static partial Regex FeatReplaceRegex();
    }
}
