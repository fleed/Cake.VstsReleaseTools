namespace Cake.VstsReleaseTools.Entities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the fields for a work item.
    /// </summary>
    public class Fields
    {
        /// <summary>
        /// Gets or sets the title
        /// </summary>
        [JsonProperty("System.Title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type of the work item.
        /// </summary>
        [JsonProperty("System.WorkItemType")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the release notes.
        /// </summary>
        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("System.Description")]
        public string Description { get; set; }
    }
}