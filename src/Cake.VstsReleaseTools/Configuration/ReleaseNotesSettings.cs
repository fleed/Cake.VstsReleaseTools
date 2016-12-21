namespace Cake.VstsReleaseTools.Configuration
{
    using System;

    /// <summary>
    /// The release notes settings.
    /// </summary>
    public class ReleaseNotesSettings
    {
        /// <summary>
        /// Gets or sets the token used for authentication.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the uri of the TeamFoundation instance.
        /// </summary>
        public Uri TfsUri { get; set; }

        /// <summary>
        /// Gets or sets the TeamFoundation project.
        /// </summary>
        public string TfsProject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show links.
        /// </summary>
        public bool ShowLinks { get; set; }

        /// <summary>
        /// Gets or sets the name for the Release Notes property.
        /// </summary>
        /// <value>
        /// The name of the VSTS property containing the release notes, if available; otherwise, <see langwor="null" />.
        /// </value>
        public string ReleaseNotesPropertyName { get; set; }
    }
}