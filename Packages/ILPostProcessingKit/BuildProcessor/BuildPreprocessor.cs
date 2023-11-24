using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.ILPostProcessingKit.BuildProcessor
{
    public class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private static readonly string EnableProfilingBlockWeaver = "ENABLE_PROFILING_BLOCK_WEAVER";

        public void OnPreprocessBuild(BuildReport report)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(report.summary.platform);

            RemovePredefinedSymbol(buildTargetGroup, EnableProfilingBlockWeaver);

            var isDevelopmentBuild = report.summary.options.HasFlag(BuildOptions.Development);
            if (isDevelopmentBuild)
            {
                var predefinedSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, $"{predefinedSymbols};{EnableProfilingBlockWeaver}");
            }
        }

        private void RemovePredefinedSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            var predefinedSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');

            var newSymbols = new List<string>();
            foreach (var s in predefinedSymbols)
            {
                if (s != symbol)
                {
                    newSymbols.Add(s);
                }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", newSymbols));
        }
    }
}
