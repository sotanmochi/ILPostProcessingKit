using System.Collections.Generic;
using UnityEditor;

namespace Unity.ILPostProcessingKit.BuildProcessor
{
    public static class BuildProcessorHelpers
    {
        public static void AddDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
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

            newSymbols.Add(symbol);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", newSymbols));
        }

        public static void RemoveDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
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
