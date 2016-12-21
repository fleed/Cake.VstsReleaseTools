namespace Cake.VstsReleaseTools
{
    using Configuration;

    using Core;
    using Core.Annotations;
    using Core.IO;

    /// <summary>
    /// Defines the Cake aliases.
    /// </summary>
    [CakeAliasCategory("VstsReleaseTools")]
    public static class VstsReleaseToolsAlias
    {
        /// <summary>
        /// Renders the release notes.
        /// </summary>
        /// <param name="context">The cake context.</param>
        /// <param name="outputPath">The output path where the rendered release notes will be saved.</param>
        /// <param name="product">The name of the product.</param>
        /// <param name="notes">The manual notes.</param>
        /// <param name="renderedReport">The rendered report.</param>
        /// <param name="renderedArtifact">The rendered artifact.</param>
        /// <param name="force">A flag indicating whether to replace the output if exists.</param>
        [CakeMethodAlias]
        public static void RenderNotes(
            this ICakeContext context,
            FilePath outputPath,
            string product,
            string notes,
            string renderedReport,
            string renderedArtifact,
            bool force)
        {
            var tools = new ReleaseTools(context);
            tools.RenderNotesAsync(outputPath, product, renderedReport, renderedArtifact, force, notes)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Renders the output of an artifact directory.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <param name="outputWhitelistPath">The path to the JSON file containing output whitelist.</param>
        /// <param name="isArtifactContainer">
        /// A flag indicating whether the directory is a container for artifacts.
        /// </param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        [CakeMethodAlias]
        public static string RenderArtifact(
            this ICakeContext context,
            DirectoryPath directory,
            FilePath outputWhitelistPath,
            bool isArtifactContainer)
        {
            var tools = new ReleaseTools(context);
            return tools.RenderArtifactAsync(directory, outputWhitelistPath, isArtifactContainer).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Renders the output of an artifact directory.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <param name="outputWhitelist">Object containing output whitelist.</param>
        /// <param name="isArtifactContainer">
        /// A flag indicating whether the directory is a container for artifacts.
        /// </param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        [CakeMethodAlias]
        public static string RenderArtifact(
            this ICakeContext context,
            DirectoryPath directory,
            OutputWhitelist outputWhitelist,
            bool isArtifactContainer)
        {
            var tools = new ReleaseTools(context);
            return tools.RenderArtifact(directory, outputWhitelist, isArtifactContainer);
        }

        /// <summary>
        /// Renders a report using work items with the specified tag.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="tags">
        /// The tags to search.
        /// </param>
        /// <param name="query">The optional query.</param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        [CakeMethodAlias]
        public static string RenderReport(
            this ICakeContext context,
            ReleaseNotesSettings settings,
            string[] tags,
            string query,
            FilePath template)
        {
            var tools = new ReleaseTools(context);
            return tools.RenderReportAsync(settings, tags, query, template).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Renders a report using work items with the specified tag and the default template.
        /// </summary>
        /// <param name="context">The Cake context.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="tags">
        /// The tags to search.
        /// </param>
        /// <param name="query">The optional query.</param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        [CakeMethodAlias]
        public static string RenderReport(
            this ICakeContext context,
            ReleaseNotesSettings settings,
            string[] tags,
            string query)
        {
            return context.RenderReport(settings, tags, query, null);
        }

        /// <summary>
        /// Copies the artifact to the specified output path using the whitelist provided as JSON file.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <param name="whitelistPath">
        /// The whitelist path.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        [CakeMethodAlias]
        public static void CopyArtifact(
            ICakeContext context,
            DirectoryPath directory,
            FilePath whitelistPath,
            DirectoryPath outputPath)
        {
            var tools = new ReleaseTools(context);
            tools.CopyArtifactAsync(directory, whitelistPath, outputPath).GetAwaiter().GetResult();
        }
    }
}