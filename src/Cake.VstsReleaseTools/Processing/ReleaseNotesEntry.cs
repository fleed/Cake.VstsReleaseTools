namespace Cake.VstsReleaseTools.Processing
{
    using System;

    /// <summary>
    /// Defines a release notes entry.
    /// </summary>
    public class ReleaseNotesEntry
    {
        /// <summary>
        /// Gets or sets the id used to reference the item in the backlog.
        /// </summary>
        public int BacklogReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content in Markdown format.</value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public ReleaseNotesEntryType Type { get; set; } = ReleaseNotesEntryType.Feature;

        /// <summary>
        /// Gets or sets the uri.
        /// </summary>
        public Uri Uri { get; set; }
    }
}