namespace Cake.VstsReleaseTools
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Common.IO;

    using Configuration;

    using Core;
    using Core.Diagnostics;
    using Core.IO;

    using Entities;

    using Processing;

    using Newtonsoft.Json;

    /// <summary>
    /// The release tools.
    /// </summary>
    public class ReleaseTools
    {
        private static readonly Regex DivReplacer = new Regex(@"<div>([\w\W\-]*?)</div>", RegexOptions.Multiline);
        private static readonly Regex BrReplacer = new Regex(@"<br/?>", RegexOptions.Multiline);
        private static readonly Regex HashReplacer = new Regex(@"^#", RegexOptions.Multiline);

        private readonly ICakeContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseTools"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public ReleaseTools(ICakeContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Copies the artifact to the specified output path using the whitelist provided as JSON file.
        /// </summary>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <param name="whitelistPath">
        /// The whitelist path.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that can be awaited.
        /// </returns>
        public async Task CopyArtifactAsync(DirectoryPath directory, FilePath whitelistPath, DirectoryPath outputPath)
        {
            var outputInfo = new DirectoryInfo(outputPath.FullPath);
            if (!outputInfo.Exists)
            {
                outputInfo.Create();
            }

            var whitelist = await ReadOutputWhitelistAsync(whitelistPath);

            foreach (var include in whitelist.Include)
            {
                this.context.CopyFiles(include, outputPath);
            }
        }

        /// <summary>
        /// Renders the release notes to the specified output path.
        /// </summary>
        /// <param name="outputPath">
        ///     The output path.
        /// </param>
        /// <param name="product">
        ///     The product.
        /// </param>
        /// <param name="renderedReport">
        ///     The rendered report.
        /// </param>
        /// <param name="renderedArtifact">
        ///     The rendered artifact.
        /// </param>
        /// <param name="force">
        ///     The force.
        /// </param>
        /// <param name="notes">
        ///     The notes.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that can be awaited.
        /// </returns>
        public async Task RenderNotesAsync(
            FilePath outputPath,
            string product,
            string renderedReport,
            string renderedArtifact,
            bool force,
            string notes = null)
        {
            var fileInfo = new FileInfo(outputPath.FullPath);
            if (fileInfo.Exists)
            {
                if (force)
                {
                    fileInfo.Delete();
                }
                else
                {
                    throw new ReleaseNotesException(
                        $"Specified path '{outputPath.FullPath}' exists and no 'force' flag has been specified");
                }
            }

            var separator = Environment.NewLine + Environment.NewLine;
            using (var stream = File.OpenWrite(outputPath.FullPath))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync($"# Release notes - {product}");
                    if (notes != null)
                    {
                        await streamWriter.WriteAsync(separator);
                        await streamWriter.WriteAsync(notes);
                    }

                    await streamWriter.WriteAsync(separator);
                    await streamWriter.WriteAsync(renderedReport);
                    await streamWriter.WriteAsync(separator);
                    await streamWriter.WriteAsync(renderedArtifact);
                }
            }
        }

        /// <summary>
        /// Renders the output of an artifact directory.
        /// </summary>
        /// <param name="directory">
        ///     The directory.
        /// </param>
        /// <param name="outputWhitelistPath">
        ///     The output Whitelist Path.
        /// </param>
        /// <param name="isArtifactContainer">
        /// A flag indicating whether the directory is a container for artifacts.
        /// </param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        public async Task<string> RenderArtifactAsync(
            DirectoryPath directory,
            FilePath outputWhitelistPath,
            bool isArtifactContainer)
        {
            var outputWhitelist = await ReadOutputWhitelistAsync(outputWhitelistPath);
            return this.RenderArtifact(directory, outputWhitelist, isArtifactContainer);
        }

        /// <summary>
        /// Renders the output of an artifact directory.
        /// </summary>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <param name="outputWhitelist">
        /// The output Whitelist.
        /// </param>
        /// <param name="isArtifactContainer">
        /// A flag indicating whether the directory is a container for artifacts.
        /// </param>
        /// <returns>
        /// The rendered output in markdown format.
        /// </returns>
        public string RenderArtifact(DirectoryPath directory, OutputWhitelist outputWhitelist, bool isArtifactContainer)
        {
            this.context.Log.Information($"Rendering content of directory '{directory}'");
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("## Content");
            if (isArtifactContainer)
            {
                var container = this.context.FileSystem.GetDirectory(directory);
                var folders = container.GetDirectories("*", SearchScope.Current);
                foreach (var folder in folders)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine($"### {folder.Path.GetDirectoryName()}");
                    var artifact = this.GetArtifact(folder.Path, outputWhitelist);
                    RenderArtifactContent(stringBuilder, artifact);
                }
            }
            else
            {
                var artifact = this.GetArtifact(directory, outputWhitelist);
                RenderArtifactContent(stringBuilder, artifact);
            }

            return stringBuilder.ToString();
        }

        private static void RenderArtifactContent(StringBuilder stringBuilder, ReleaseArtifact artifact)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Name | Containing Path | Version");
            stringBuilder.Append("-----|------|--------");
            foreach (var file in artifact.Files.OrderBy(f => f.RelativePath))
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"`{file.Name}` | /{file.RelativePath} | {file.Version ?? "-"}");
            }
        }

        /// <summary>
        /// Renders a report using work items with the specified tag.
        /// </summary>
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
        public async Task<string> RenderReportAsync(
            ReleaseNotesSettings settings,
            string[] tags,
            string query,
            FilePath template)
        {
            var report = await this.CreateReportAsync(settings, tags, query);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("## Implemented Features");
            var features = report.Entries.Where(e => e.Type == ReleaseNotesEntryType.Feature);
            this.RenderEntries(settings, stringBuilder, features);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.Append("## Bugs fixed");
            var bugs = report.Entries.Where(e => e.Type == ReleaseNotesEntryType.Bug);
            this.RenderEntries(settings, stringBuilder, bugs);

            return stringBuilder.ToString();
        }

        private static async Task<OutputWhitelist> ReadOutputWhitelistAsync(FilePath whitelistPath)
        {
            using (var file = File.OpenRead(whitelistPath.FullPath))
            {
                using (var reader = new StreamReader(file))
                {
                    return JsonConvert.DeserializeObject<OutputWhitelist>(await reader.ReadToEndAsync());
                }
            }
        }

        private void RenderEntries(
            ReleaseNotesSettings settings,
            StringBuilder stringBuilder,
            IEnumerable<ReleaseNotesEntry> entries)
        {
            var list = entries.ToList();
            if (!list.Any())
            {
                stringBuilder.AppendLine();
                stringBuilder.Append("No entry available");
                return;
            }

            foreach (var entry in list)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.AppendLine($"### {entry.BacklogReferenceId} - {entry.Title}");
                stringBuilder.AppendLine();
                string content;
                if (string.IsNullOrEmpty(entry.Content))
                {
                    content = "Description not available";
                }
                else
                {
                    try
                    {
                        content = BrReplacer.Replace(entry.Content, string.Empty);
                        content = DivReplacer.Replace(content, Environment.NewLine + "$1");

                        // it seems that a final <div></div> keeps in the content
                        content = DivReplacer.Replace(content, Environment.NewLine + "$1");
                        content = HashReplacer.Replace(content, "####");
                    }
                    catch (Exception exception)
                    {
                        this.context.Log.Error($"Error during conversion: {exception.Message}");
                        content = "Error during conversion";
                    }
                }

                stringBuilder.AppendLine(content);
                stringBuilder.AppendLine();
                var uri =
                    new Uri(
                        entry.Uri.ToString().Replace("_apis/wit/workItems", $"{settings.TfsProject}/_workitems/edit"));
                stringBuilder.Append($"Full description: [{uri}]({uri})");
            }
        }

        private ReleaseArtifact GetArtifact(DirectoryPath directory, OutputWhitelist outputWhitelist)
        {
            var artifact = new ReleaseArtifact();
            var files = ImmutableArray<ArtifactFile>.Empty;
            this.context.Log.Information($"Output white list includes {outputWhitelist.Include.Length} item(s)");
            foreach (var include in outputWhitelist.Include)
            {
                this.context.Log.Information($"Getting artifacts for include filter '{include}'");
                var artifactFiles =
                    this.context.GetFiles(directory + "/" + include)
                        .Select(filePath => this.GetArtifactFile(directory, filePath));
                files =
                    files.AddRange(
                        artifactFiles);
            }

            artifact.Files = files.ToList();
            return artifact;
        }

        private ArtifactFile GetArtifactFile(DirectoryPath directory, FilePath filePath)
        {
            var rootUri = new Uri(directory.FullPath + "/");
            var containingDirectory = filePath.GetDirectory();
            var fileUri = new Uri(containingDirectory.FullPath + "/");
            var relative = rootUri.MakeRelativeUri(fileUri).ToString();
            var info = new FileInfo(filePath.FullPath);
            var versionInfo = FileVersionInfo.GetVersionInfo(filePath.FullPath);
            return new ArtifactFile
                       {
                           FullPath = filePath,
                           Name = info.Name,
                           RelativePath = relative,
                           Version = versionInfo.FileVersion
                       };
        }

        private async Task<ReleaseNotesReport> CreateReportAsync(
            ReleaseNotesSettings settings,
            string[] tags,
            string query)
        {
            if (string.IsNullOrEmpty(query) && (tags == null || !tags.Any()))
            {
                throw new ArgumentException("Either query or tags must be defined");
            }

            ResultRow[] rows;
            if (string.IsNullOrEmpty(query))
            {
                var taggedWorkItems = await this.GetTaggedWorkItemsAsync(settings, tags);
                rows = taggedWorkItems.WorkItems;
            }
            else
            {
                var queryItems = await this.GetQueryWorkItemsAsync(settings, query);
                rows = queryItems.WorkItems;
            }

            var workItems = await this.GetWorkItemsAsync(settings, rows);
            var parents = await this.GetParentsAsync(settings, workItems.Value);
            var reportItems =
                workItems.Value.Union(parents.Value)
                    .Distinct(new WorkItemComparer())
                    .Where(f => f.Fields.Type == "Feature" || f.Fields.Type == "Bug");
            return new ReleaseNotesReport
                       {
                           Entries =
                               reportItems.Select(
                                   f =>
                                       new ReleaseNotesEntry
                                           {
                                               Content = f.Fields.ReleaseNotes,
                                               Title = f.Fields.Title,
                                               Type =
                                                   f.Fields.Type == "Feature"
                                                       ? ReleaseNotesEntryType
                                                           .Feature
                                                       : ReleaseNotesEntryType.Bug,
                                               BacklogReferenceId = f.Id,
                                               Uri = new Uri(f.Url)
                                           }).ToList()
                       };
        }

        private async Task<WorkItemsResult> GetWorkItemsAsync(ReleaseNotesSettings settings, ResultRow[] results)
        {
            if (!results.Any())
            {
                return new WorkItemsResult { Value = new WorkItem[0] };
            }

            var ids = results.Select(s => s.Id.ToString()).Aggregate((s, s1) => s + "," + s1);
            var uri = new Uri(settings.TfsUri, $"_apis/wit/workitems?api-version=1.0&ids={ids}&$expand=relations");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"scrum:{settings.Token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                                 "Basic",
                                                                 base64String);
                var response = await client.GetStringAsync(uri);
                var serializerSettings = this.CreateJsonSerializerSettings(settings.ReleaseNotesPropertyName);
                return JsonConvert.DeserializeObject<WorkItemsResult>(response, serializerSettings);
            }
        }

        private async Task<WorkItemsResult> GetParentsAsync(ReleaseNotesSettings settings, WorkItem[] workItems)
        {
            if (!workItems.Any())
            {
                return new WorkItemsResult { Value = new WorkItem[0] };
            }

            var reversed =
                workItems.Where(wi => wi.Relations != null)
                    .SelectMany(wi => wi.Relations)
                    .Where(r => r.Type == "System.LinkTypes.Hierarchy-Reverse")
                    .ToList();
            if (!reversed.Any())
            {
                return new WorkItemsResult { Value = new WorkItem[0] };
            }

            var ids =
                reversed.Select(s => s.Url.Substring(s.Url.LastIndexOf('/') + 1)).Aggregate((s, s1) => s + "," + s1);
            var uri = new Uri(settings.TfsUri, $"_apis/wit/workitems?api-version=1.0&ids={ids}&$expand=relations");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"scrum:{settings.Token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                                 "Basic",
                                                                 base64String);
                var response = await client.GetStringAsync(uri);
                var serializerSettings = this.CreateJsonSerializerSettings(settings.ReleaseNotesPropertyName);
                return JsonConvert.DeserializeObject<WorkItemsResult>(response, serializerSettings);
            }
        }

        private async Task<QueryResult> GetQueryWorkItemsAsync(ReleaseNotesSettings settings, string query)
        {
            this.context.Log.Information($"Getting work items with query: {query}");
            var uri = new Uri(settings.TfsUri, $"{settings.TfsProject}/_apis/wit/wiql/{query}?api-version=1.0");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"scrum:{settings.Token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                                 "Basic",
                                                                 base64String);
                var queries =
                    await client.GetAsync(
                        new Uri(
                            settings.TfsUri,
                            $"{settings.TfsProject}/_apis/wit/queries/Shared%20Queries%2FProducts%2FMotion?$depth=2&$expand=all&api-version=2.2"));
                var content = await queries.Content.ReadAsStringAsync();
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                var s = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QueryResult>(s);
            }
        }

        private async Task<QueryResult> GetTaggedWorkItemsAsync(ReleaseNotesSettings settings, string[] tags)
        {
            if (!tags.Any())
            {
                this.context.Log.Warning("No tag specified");
                return new QueryResult();
            }

            var tagString = tags.Select(s => $"[System.Tags] Contains '{s}'").Aggregate((s, s1) => s + " OR " + s1);
            this.context.Log.Information($"Getting work items with tags: {tagString}");
            var query = new { query = $"Select [System.Id] FROM WorkItems WHERE {tagString}" };
            var uri = new Uri(settings.TfsUri, $"{settings.TfsProject}/_apis/wit/wiql?api-version=1.0");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"scrum:{settings.Token}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                                                                 "Basic",
                                                                 base64String);
                var content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
                var s = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QueryResult>(s);
            }
        }

        private JsonSerializerSettings CreateJsonSerializerSettings(string releaseNotesPropertyName)
        {
            return new JsonSerializerSettings
                       {
                           ContractResolver =
                               new ReleaseNotesContractResolver(this.context.Log, releaseNotesPropertyName)
                       };
        }
    }

    internal class QueryResult
    {
        [JsonProperty("workItems")]
        public ResultRow[] WorkItems { get; set; } = new ResultRow[0];
    }

    internal class ResultRow
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    internal class WorkItemsResult
    {
        [JsonProperty("value")]
        public WorkItem[] Value { get; set; }
    }

    internal class WorkItemComparer : IEqualityComparer<WorkItem>
    {
        public bool Equals(WorkItem x, WorkItem y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(WorkItem obj)
        {
            return obj.Id;
        }
    }
}