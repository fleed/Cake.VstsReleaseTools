namespace Cake.VstsReleaseTools.Processing
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a release notes report.
    /// </summary>
    public class ReleaseNotesReport
    {
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        public ICollection<ReleaseNotesEntry> Entries { get; set; } = new List<ReleaseNotesEntry>();

        /// <summary>
        /// Gets or sets the date when the report has been created.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}