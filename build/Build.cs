using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using System.Collections.Generic;
using System.Linq;
using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.BuildDockerImage);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net6.0")] readonly GitVersion GitVersion;


    private readonly HashSet<string> ProjectsToBuildImage = new HashSet<string>()
    {
      "AspNetCore.POC.WebApi",
      "AspNetCore.POC.WebMvc"
    };


    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath AssetsJson => RootDirectory / "build" / "obj" / "project.assets.json";


    const string IMAGE_PATH = "aspnetcore-poc";


    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            TestsDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(solutionFile => solutionFile.SetProjectFile(Solution.Path));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVerbosity(DotNetVerbosity.Minimal)
            );
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var projectsToPublish = Solution.AllProjects.Where(
                p => !p.Name.ToLower().Contains("test")
                  && !p.Name.ToLower().Contains("build")
                  && !p.Name.ToLower().Contains("docker")
            );

            foreach (var project in projectsToPublish)
            {
                DotNetPublish(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore()
                .SetOutput(ArtifactsDirectory / project.Name));
            }
        });

    Target BuildDockerImage => _ => _
        .Executes(() =>
        {
            (ArtifactsDirectory / "docker").CreateOrCleanDirectory();

            foreach (var project in Solution.AllProjects.Where(project => ProjectsToBuildImage.Contains(project.Name)))
            {
                var IMAGE_NAME = project.Name.ToLower().Replace('.', '-');
                var projectFilePath = project.Path.ToString().Replace('\\', '/');
                var projectDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('/'));
                var dockerFilePath = projectDirectory + "/Dockerfile";

                DockerBuild(toolSettings => toolSettings
                    .SetProcessWorkingDirectory(projectDirectory)
                    .SetPath(projectDirectory)
                    .SetFile(dockerFilePath)
                    .AddBuildArg($"AssemblyVersion={GitVersion.AssemblySemVer}")
                    .AddBuildArg($"FileVersion={GitVersion.AssemblySemFileVer}")
                    .AddBuildArg($"InformationalVersion={GitVersion.InformationalVersion}")
                    .AddLabel($"Version={GitVersion.NuGetVersionV2}")
                    .AddTag($"{IMAGE_PATH}/{IMAGE_NAME}:{GitVersion.NuGetVersionV2}")
                );
            }
        });

}
