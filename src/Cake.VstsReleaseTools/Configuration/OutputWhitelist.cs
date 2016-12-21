namespace Cake.VstsReleaseTools.Configuration
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the whitelist to filter the output.
    /// </summary>
    public class OutputWhitelist
    {
        /// <summary>
        /// Gets or sets the extensions to be included. Use the globbing notation.
        /// </summary>
        [JsonProperty("include")]
        public string[] Include { get; set; }
    }
}