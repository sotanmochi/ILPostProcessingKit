using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.ILPostProcessingKit.BuildProcessor
{
    public sealed class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public static readonly string EnableProfilingBlockWeaver = "ENABLE_PROFILING_BLOCK_WEAVER";
        public static readonly int CallbackOrder = 0;

        public int callbackOrder => CallbackOrder;

        public void OnPreprocessBuild(BuildReport report)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(report.summary.platform);

            BuildProcessorHelpers.RemoveDefineSymbol(buildTargetGroup, EnableProfilingBlockWeaver);

            var isDevelopmentBuild = report.summary.options.HasFlag(BuildOptions.Development);
            if (isDevelopmentBuild)
            {
                BuildProcessorHelpers.AddDefineSymbol(buildTargetGroup, EnableProfilingBlockWeaver);
            }
        }
    }
}
