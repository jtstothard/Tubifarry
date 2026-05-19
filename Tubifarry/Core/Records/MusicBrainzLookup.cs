namespace Tubifarry.Core.Records
{
    /// <summary>
    /// MusicBrainz IDs extracted from Lidarr's RemoteAlbum context.
    /// These come from Lidarr's own database — the same release Lidarr
    /// matched when it decided to grab this album.
    /// </summary>
    public record MusicBrainzIds
    {
        public string? ReleaseId { get; init; }
        public string? ReleaseGroupId { get; init; }
        public string? ArtistId { get; init; }
        public string? ReleaseArtistId { get; init; }
        public Dictionary<int, string>? TrackRecordingIds { get; init; }
    }
}
