using System;
using System.Linq;
using UnityEditor;

namespace Unity.ILPostProcessingKit
{
    [InitializeOnLoad]
    public static class EditorUtils
    {
        static EditorUtils()
        {
#if !ENABLE_PROFILING_BLOCK_WEAVER
            AddScriptingDefineSymbolToAllBuildTargetGroups("ENABLE_PROFILING_BLOCK_WEAVER");
#endif
        }

        public static void AddScriptingDefineSymbolToAllBuildTargetGroups(string symbol)
        {
            foreach (BuildTarget buildTarget in Enum.GetValues(typeof(BuildTarget)))
            {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

                if (buildTargetGroup == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
                                    .Split(';').Select(s => s.Trim()).ToList();

                if (!defineSymbols.Contains(symbol))
                {
                    defineSymbols.Add(symbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", defineSymbols));
                }
            }
        }
    }
}
