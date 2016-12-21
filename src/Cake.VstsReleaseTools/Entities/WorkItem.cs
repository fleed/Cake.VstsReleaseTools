namespace Cake.VstsReleaseTools.Entities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines a VSTS work item.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fields")]
        public Fields Fields { get; set; }

        /// <summary>
        /// Gets or sets the relations.
        /// </summary>
        [JsonProperty("relations")]
        public Relation[] Relations { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}