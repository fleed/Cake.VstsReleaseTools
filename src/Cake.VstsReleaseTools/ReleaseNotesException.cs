namespace Cake.VstsReleaseTools
{
    using System;

    /// <summary>
    /// Exception occurring during generation of release notes.
    /// </summary>
    public class ReleaseNotesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseNotesException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ReleaseNotesException(string message)
        : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseNotesException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ReleaseNotesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}