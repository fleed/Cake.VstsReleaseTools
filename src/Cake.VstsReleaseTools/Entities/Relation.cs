namespace Cake.VstsReleaseTools.Entities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines a relation between items.
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// Gets or sets the type of the relation.
        /// </summary>
        [JsonProperty("rel")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}