using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLPageBuilder
{
    private const string DefaultBuildPath = "Builds/WebGLPages";

    public static void Build()
    {
        var buildPath = GetCommandLineArg("-buildPath") ?? DefaultBuildPath;
        if (!Path.IsPathRooted(buildPath))
            buildPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, buildPath));

        if (Directory.Exists(buildPath))
            Directory.Delete(buildPath, true);
        Directory.CreateDirectory(buildPath);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"WebGL build failed: {report.summary.result}");
    }

    private static string GetCommandLineArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }

        return null;
    }
}
