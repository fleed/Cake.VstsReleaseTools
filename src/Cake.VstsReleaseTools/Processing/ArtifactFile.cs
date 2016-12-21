namespace Cake.VstsReleaseTools.Processing
{
    using Core.IO;

    /// <summary>
    /// Describes an artifact file.
    /// </summary>
    public class ArtifactFile
    {
        /// <summary>
        /// Gets or sets the full path.
        /// </summary>
        public FilePath FullPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the artifact directory.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the (optional) version.
        /// </summary>
        /// <value>The version of the file, if valid; otherwise, <see langword="null" />.</value>
        public string Version { get; set; }
    }
}