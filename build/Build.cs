using GlobExpressions;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version of the NuGet package to build")]
    readonly string Version = string.Empty;

    [Parameter("The API key for publishing NuGet packages")]
    readonly string NuGetApiKey = string.Empty;

    [Parameter("The access token for making calls to the GitHub API")]
    readonly string GitHubToken = string.Empty;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestDirectory => RootDirectory / "test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath NugetDirectory => ArtifactsDirectory / "nuget";
    AbsolutePath TestResultDirectory => ArtifactsDirectory / "test-results";
    AbsolutePath CoverageResultDirectory => ArtifactsDirectory / "coverage";
    AbsolutePath CoverageReportDirectory => ArtifactsDirectory / "coverage-report";

    IEnumerable<Project> TestProjects => Solution.GetProjects("*.Tests");

    string GetRepositoryOwner() => GitRepository.Identifier.Split('/').First();

    Target Prepare => _ => _
        .Executes(() => IsServerBuild ? NpmTasks.NpmCi() : NpmTasks.NpmInstall());

    Target Verify => _ => _
        .Requires(() => IsLocalBuild || (!NuGetApiKey.IsNullOrEmpty() && !GitHubToken.IsNullOrEmpty()))
        .Executes(() => Log.Information("Environment variables verified"));

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Clean, Compile)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .EnableNoRestore()
            .EnableNoBuild()
            .ResetVerbosity()
            .SetResultsDirectory(TestResultDirectory)
            .EnableCollectCoverage()
            .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            .SetExcludeByFile("*.Generated.cs")
            .CombineWith(TestProjects, (_, v) => _
                .SetProjectFile(v)
                .SetLoggers($"trx;LogFileName={v.Name}.trx;FailureBodyFormat=Verbose")
                .SetCoverletOutput($"{CoverageResultDirectory}/{v.Name}.xml"))));

    Target Cover => _ => _
        .DependsOn(Test)
        .Consumes(Test)
        .Produces(CoverageResultDirectory / "lcov.info")
        .Executes(() =>
        {
            ReportGenerator(_ => _
                .SetFramework("net5.0")
                .SetReports(CoverageResultDirectory / "*.xml")
                .SetTargetDirectory(CoverageReportDirectory)
                .SetReportTypes("lcov")
                .When(IsLocalBuild, _ => _
                    .AddReportTypes(ReportTypes.HtmlInline)));
        });

    Target Pack => _ => _
        .DependsOn(Clean, Restore)
        .Requires(() => !string.IsNullOrWhiteSpace(Version))
        .Executes(() => DotNetPack(s => s
            .SetConfiguration(Configuration.Release)
            .SetVersion(Version)
            .EnableIncludeSymbols()
            .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
            .SetOutputDirectory(NugetDirectory)
            .CombineWith(Glob.Files(SourceDirectory, "**/*.csproj"), (ss, project) => ss
                .SetProject(SourceDirectory / project))));

    Target Publish => _ => _
        .Executes(() => { });

    Target PublishGitHub => _ => _
        .OnlyWhenStatic(() => IsServerBuild)
        .Requires(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .TriggeredBy(Publish)
        .Executes(() => DotNetNuGetPush(s => s
            .SetSource($"https://nuget.pkg.github.com/{GetRepositoryOwner()}/index.json")
            .SetApiKey(GitHubToken)
            .CombineWith(Glob.Files(NugetDirectory, "*.*nupkg"), (ss, package) => ss
                .SetTargetPath(NugetDirectory / package))));

    Target PublishNuGet => _ => _
        .OnlyWhenStatic(() => IsServerBuild && GitRepository.IsOnMainOrMasterBranch())
        .Requires(() => !string.IsNullOrWhiteSpace(NuGetApiKey))
        .TriggeredBy(Publish)
        .Executes(() => DotNetNuGetPush(s => s
            .SetSource("https://api.nuget.org/v3/index.json")
            .SetApiKey(NuGetApiKey)
            .CombineWith(Glob.Files(NugetDirectory, "*.*nupkg"), (ss, package) => ss
                .SetTargetPath(NugetDirectory / package))));
}