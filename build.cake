#addin "Cake.FileHelpers&version=3.2.0"

void RestoreAll(string root, string glob)
{
    var csprojFiles = GetFiles(root + glob);
    if (csprojFiles.Any())
    {
        var tempSln = new FilePath(root + "BuildSolution.sln");
        FileWriteText(tempSln, CreateTempSolution(csprojFiles));
        NuGetRestore(tempSln, GetRestoreSettings());
    }
}

void Restore(string root) =>
    NuGetRestore(new FilePath($"{root}{Argument<string>("Solution")}/{Argument<string>("Solution")}.sln"), GetRestoreSettings());

void Build(string root)
{
    var solution = Argument<string>("Solution", "") != "" ?
        new FilePath($"{root}{Argument<string>("Solution")}/{Argument<string>("Solution")}.sln") :
        new FilePath(root + "BuildSolution.sln");

    MSBuild(solution, settings => settings
        .SetConfiguration(Argument<string>("Configuration", "Debug"))
        .UseToolVersion(MSBuildToolVersion.VS2019)
        .SetNoLogo(true)
        .SetVerbosity(Verbosity.Quiet)
        .WithConsoleLoggerParameter("ErrorsOnly")
        .SetMaxCpuCount(0)
        .SetNodeReuse(true)
        .WithProperty("BuildInParallel", "true")
        .WithProperty("DeployNugetPackages",Argument<string>("DeployNugetPackages", "false"))
        .WithProperty("MigrateProjects", Argument<string>("MigrateProjects", "false"))
        .WithProperty("UpdatePackagesInLegacyFormat", Argument<string>("UpdatePackagesInLegacyFormat", "false"))
        .WithProperty("CreateHardLinksForCopyFilesToOutputDirectoryIfPossible", "true")
        .WithProperty("CreateHardLinksForCopyAdditionalFilesIfPossible", "true")
        .WithProperty("CreateHardLinksForCopyLocalIfPossible", "true")
        .WithProperty("CreateHardLinksForPublishFilesIfPossible", "true"));
}

NuGetRestoreSettings GetRestoreSettings() 
{
    return new NuGetRestoreSettings 
    { 
        NonInteractive = true,
        Verbosity = NuGetVerbosity.Quiet,
        ConfigFile = new FilePath(@"tools\nuget.config")
    };
}

string CreateTempSolution(IEnumerable<FilePath> projects)
{
    var slnContents = new StringBuilder();
    slnContents.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
    slnContents.AppendLine("# Visual Studio Version 16");
    slnContents.AppendLine("VisualStudioVersion = 16.0.28803.202");
    slnContents.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");
    var projGuids = new List<string>();
    foreach(var project in projects) 
    {
        string projGuid;
        var projContents = FileReadText(project);
        var from = projContents.IndexOf("<ProjectGuid>{");

        if (from == -1) projGuid = Guid.NewGuid().ToString().ToUpperInvariant();
        else projGuid = projContents.Substring(from + 14, 36);
        projGuids.Add(projGuid);
        slnContents.AppendFormat("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\" \r\n", Guid.NewGuid().ToString().ToUpperInvariant(), project.GetFilenameWithoutExtension(), project.FullPath.ToString(), projGuid);
        slnContents.Append("EndProject \r\n");
    }

    slnContents.Append("Global \r\n");
    slnContents.Append("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution \r\n");
    slnContents.Append("\t\tDebug|Any CPU = Debug|Any CPU \r\n");
    slnContents.Append("\t\tRelease|Any CPU = Release|Any CPU \r\n");
    slnContents.Append("\tEndGlobalSection \r\n");
    slnContents.Append("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution \r\n");
    foreach(var guid in projGuids) 
    {
        slnContents.AppendFormat("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU \r\n", guid);
        slnContents.AppendFormat("\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU \r\n", guid);
        slnContents.AppendFormat("\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU \r\n", guid);
        slnContents.AppendFormat("\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU \r\n", guid);
    }
    slnContents.Append("\tEndGlobalSection \r\n");
    slnContents.Append("\tGlobalSection(SolutionProperties) = preSolution \r\n");
    slnContents.Append("\t\tHideSolutionNode = FALSE \r\n");
    slnContents.Append("\tEndGlobalSection \r\n");
    slnContents.Append("\tGlobalSection(ExtensibilityGlobals) = postSolution \r\n");
    slnContents.AppendFormat("\t\tSolutionGuid = {{{0}}} \r\n", Guid.NewGuid().ToString().ToUpperInvariant());
    slnContents.Append("\tEndGlobalSection \r\n");
    slnContents.Append("EndGlobal \r\n");

    return slnContents.ToString();
}

Task("RestoreAll").Does(() => RestoreAll("", "**/*.csproj"));
Task("Restore").Does(() => Restore(""));
Task("Build").Does(() => Build(""));