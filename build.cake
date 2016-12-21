#load "helpers.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var versionInfo = Argument("versionInfo", "0.0.1");
var framework = Argument("framework", "net451");
var artifacts = "./dist/";

var releaseToolsDir = Directory("./src/Cake.VstsReleaseTools");
var releaseToolsOutput = Directory("./output/Cake.VstsReleaseTools");
var releaseToolsNuspec = File("./Cake.VstsReleaseTools.nuspec");

var frameworks = GetFrameworks(framework);

Task("Clean")
    .Does(() => {
        if (DirectoryExists(releaseToolsOutput))
        {
            CleanDirectory(releaseToolsOutput);
        }
    Information("Cleaning common files...");
    CleanDirectory(artifacts);
    DeleteFiles(GetFiles("./*.temp.nuspec"));
    });

Task("Restore")
    .Does(() =>
{
        DotNetCoreRestore(releaseToolsDir.ToString());
});

Task("Build")
    //.IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() => {
    Information("Building solution...");
    CreateDirectory(artifacts + "build/");
        var buildSettings = new DotNetCoreBuildSettings
        {
            Configuration = configuration
        };
        foreach (var f in frameworks)
        {
            DotNetCoreBuild(releaseToolsDir.ToString(), new DotNetCoreBuildSettings {
                    Framework = f,
                    Configuration = configuration,
                    //OutputDirectory = artifacts + "build/" + configuration + "/" + f
                });
        }
    });

Task("Post-Build")
    .IsDependentOn("Build")
    .Does(() => {
        CopyDirectory(releaseToolsDir.ToString() + "/bin/" + configuration, artifacts + "build/Cake.VstsReleaseTools");
    });

Task("NuGet")
    .IsDependentOn("Post-Build")
    .Does(() => {
        var packagePath = Directory(artifacts + "package/");
        CreateDirectory(packagePath.ToString());
        Information("Building NuGet package");
        var content = GetContent(frameworks, artifacts + "build/Cake.VstsReleaseTools/", "/Cake.VstsReleaseTools");
        foreach (var item in content)
        {
            Information("Item(s): " + item.Source);
        }
        
        NuGetPack(releaseToolsNuspec, new NuGetPackSettings() {
            Version = versionInfo,
            ReleaseNotes = new List<string>(),
            OutputDirectory = packagePath.ToString(),
            Files = content
            });
    });

Task("PublishReleaseNotes")
    .IsDependentOn("NuGet")
    .Does(() =>
    {
        Information("Publishing");
        var packagePath = Directory(artifacts + "package/");

        var package = File(packagePath.ToString() + "/Cake.VstsReleaseTools." + versionInfo + ".nupkg");

        // Push the package.
    var configFile = "./NuGet.config";
        NuGetPush(package, new NuGetPushSettings {
            Source = "Cake.VstsReleaseTools",
            ApiKey = "VSTS"
        });
    });


Task("Default")
    .IsDependentOn("Post-Build");
RunTarget(target);