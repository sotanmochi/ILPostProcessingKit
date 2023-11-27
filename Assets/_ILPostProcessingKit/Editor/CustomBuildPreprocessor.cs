using System;
using Unity.ILPostProcessingKit.BuildProcessor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using StandardBuildPreprocessor = Unity.ILPostProcessingKit.BuildProcessor.BuildPreprocessor;

namespace ILPostProcessingKit.Samples.BuildProcessor
{
    public sealed class CustomBuildPreprocessor : IPreprocessBuildWithReport
    {
        // Execute after StandardBuildPreprocessor
        public int callbackOrder => StandardBuildPreprocessor.CallbackOrder + 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            var symbol = StandardBuildPreprocessor.EnableProfilingBlockWeaver;

            var envVarTarget = EnvironmentVariableTarget.Machine;
            // var envVarTarget = EnvironmentVariableTarget.Process;

            var buildEnvironment = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT", envVarTarget);
            if (buildEnvironment == "Development")
            {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(report.summary.platform);
                BuildProcessorHelpers.AddDefineSymbol(buildTargetGroup, symbol);
            }
        }
    }
}
