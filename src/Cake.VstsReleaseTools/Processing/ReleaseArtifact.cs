namespace Cake.VstsReleaseTools.Processing
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a release artifact.
    /// </summary>
    public class ReleaseArtifact
    {
        /// <summary>
        /// Gets or sets the files of the artifact.
        /// </summary>
        public ICollection<ArtifactFile> Files { get; set; } = new List<ArtifactFile>();
    }
}