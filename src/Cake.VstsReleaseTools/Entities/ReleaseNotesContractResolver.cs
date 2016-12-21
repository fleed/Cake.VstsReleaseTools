namespace Cake.VstsReleaseTools.Entities
{
    using System.Reflection;

    using Cake.Core.Diagnostics;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Extends the <see cref="DefaultContractResolver"/> to replace the release notes field.
    /// </summary>
    public class ReleaseNotesContractResolver : DefaultContractResolver
    {
        private readonly string releaseNotesPropertyName;

        private readonly ICakeLog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseNotesContractResolver"/> class.
        /// </summary>
        /// <param name="log">The cake log.</param>
        /// <param name="releaseNotesPropertyName">The name of the release notes property.</param>
        public ReleaseNotesContractResolver(ICakeLog log, string releaseNotesPropertyName)
        {
            this.log = log;
            this.releaseNotesPropertyName = releaseNotesPropertyName;
        }

        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member,
                                         MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType != typeof(Fields) ||
                  property.PropertyName != nameof(Fields.ReleaseNotes))
            {
                return property;
            }

            if (string.IsNullOrEmpty(this.releaseNotesPropertyName))
            {
                this.log.Information("Release notes property name not set");
                return property;
            }

            this.log.Information($"Replacing ReleaseNotes property with value '{this.releaseNotesPropertyName}'");
            property.PropertyName = this.releaseNotesPropertyName;
            return property;
        }
    }
}