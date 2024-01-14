using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using Unity.ILPostProcessingKit.CodeGen.Helpers;

namespace Unity.ILPostProcessingKit.CodeGen
{
    public class ProfilingBlockWeaver : ILPostProcessor
    {
        public static readonly string SettingsFilePath = "Assets/_ILPostProcessingKit/ProfilingBlockWeaverSettings.json";

        public static readonly bool IsEnable =
#if ENABLE_PROFILING_BLOCK_WEAVER
            true;
#else
            false;
#endif

        class TargetInfo
        {
            public bool IsBaseType;
            public string TypeName;
            public List<string> MethodNames = new();
        }

        private List<string> _targetAssemblyNamePatterns = new();
        private Dictionary<string, TargetInfo> _targetBaseTypes = new();
        private Dictionary<string, TargetInfo> _targetTypes = new();

        private List<DiagnosticMessage> _diagnostics = new();

        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly) => true;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var assemblyDefinition = CodeGenHelpers.AssemblyDefinitionFor(compiledAssembly);

            ImportSettings();

            var assemblyNamPatternMatched = false;
            foreach (var pattern in _targetAssemblyNamePatterns)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(compiledAssembly.Name, pattern))
                {
                    assemblyNamPatternMatched = true;
                }
            }

            if (!assemblyNamPatternMatched)
            {
                return CodeGenHelpers.GetResult(assemblyDefinition, _diagnostics);
            }

            if (!IsEnable)
            {
                _diagnostics.AddWarning($"<color=orange>[{nameof(ProfilingBlockWeaver)}] IsEnable: {IsEnable}</color>");
                return CodeGenHelpers.GetResult(assemblyDefinition, _diagnostics);
            }

            _diagnostics.AddWarning($"=============================================================");
            _diagnostics.AddWarning($"<color=cyan>[{nameof(ProfilingBlockWeaver)}] Target assembly: {compiledAssembly.Name}</color>");

            foreach (var typeDefinition in assemblyDefinition.MainModule.Types)
            {
                if (typeDefinition.BaseType != null &&
                    _targetBaseTypes.TryGetValue(typeDefinition.BaseType.Name, out var targetBaseType))
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (targetBaseType.MethodNames.Contains(methodDefinition.Name))
                        {
                            _diagnostics.AddWarning($"<color=cyan>[{nameof(ProfilingBlockWeaver)}] Insert profiling block to '{typeDefinition.FullName}.{methodDefinition.Name}'</color>");
                            InsertProfilingBlock(assemblyDefinition, typeDefinition, methodDefinition);
                        }
                    }
                }

                if (_targetTypes.TryGetValue(typeDefinition.Name, out var targetType))
                {
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (targetType.MethodNames.Contains(methodDefinition.Name))
                        {
                            _diagnostics.AddWarning($"<color=cyan>[{nameof(ProfilingBlockWeaver)}] Insert profiling block to '{typeDefinition.FullName}.{methodDefinition.Name}'</color>");
                            InsertProfilingBlock(assemblyDefinition, typeDefinition, methodDefinition);
                        }
                    }
                }
            }

            return CodeGenHelpers.GetResult(assemblyDefinition, _diagnostics);
        }

        private void InsertProfilingBlock(AssemblyDefinition assemblyDefinition, TypeDefinition typeDefinition, MethodDefinition methodDefinition)
        {
            var processor = methodDefinition.Body.GetILProcessor();

            var instructions = methodDefinition.Body.Instructions;
            var firstInstruction = instructions[0];
            var lastInstruction = instructions[instructions.Count - 1];

            var beginSampleRef = assemblyDefinition.MainModule.ImportReference(
                typeof(UnityEngine.Profiling.Profiler).GetMethod("BeginSample", new Type[] { typeof(string) }));

            var endSampleRef = assemblyDefinition.MainModule.ImportReference(
                typeof(UnityEngine.Profiling.Profiler).GetMethod("EndSample"));

            // Add instructions to execute Profiler.BeginSample at the beginning of the method.
            var methodFullName = $"{typeDefinition.FullName}.{methodDefinition.Name}";
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldstr, methodFullName)); // Push the method name onto the stack.
            processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, beginSampleRef));

            // Add instructions to execute Profiler.EndSample in the finally block
            var finallyInstructions = new List<Instruction>()
            {
                Instruction.Create(OpCodes.Call, endSampleRef),
                Instruction.Create(OpCodes.Endfinally),
            };
            foreach (var instruction in finallyInstructions)
            {
                processor.InsertAfter(lastInstruction, instruction);
                lastInstruction = instruction;
            }

            // Set up the try-finally block
            var handler = new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = firstInstruction,
                TryEnd = finallyInstructions.First(),
                HandlerStart = finallyInstructions.First(),
                HandlerEnd = finallyInstructions.Last().Next,
            };

            methodDefinition.Body.ExceptionHandlers.Add(handler);
        }

        private void ImportSettings()
        {
            if (!File.Exists(SettingsFilePath)) 
            {
                _diagnostics.AddError($"[{nameof(ProfilingBlockWeaver)}] File not found: '{SettingsFilePath}'");
                return;
            }

            try
            {
                var jsonUtf8Bytes = File.ReadAllBytes(SettingsFilePath);
                var jsonNode = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(jsonUtf8Bytes));

                _targetAssemblyNamePatterns = jsonNode["TargetAssemblyNamePatterns"].Select(node => node.Get<string>()).ToList();

                _targetBaseTypes = jsonNode["TargetInfoList"]
                    .Where(node => node["IsBaseType"].Get<bool>() == true)
                    .Select(node => new TargetInfo()
                    {
                        IsBaseType = node["IsBaseType"].Get<bool>(),
                        TypeName = node["TypeName"].Get<string>(),
                        MethodNames = node["MethodNames"].Select(methodName => methodName.Get<string>()).ToList(),
                    })
                    .ToDictionary(node => node.TypeName, node => node);

                _targetTypes = jsonNode["TargetInfoList"]
                    .Where(node => node["IsBaseType"].Get<bool>() == false)
                    .Select(node => new TargetInfo()
                    {
                        IsBaseType = node["IsBaseType"].Get<bool>(),
                        TypeName = node["TypeName"].Get<string>(),
                        MethodNames = node["MethodNames"].Select(methodName => methodName.Get<string>()).ToList(),
                    })
                    .ToDictionary(node => node.TypeName, node => node);
            }
            catch (Exception e)
            {
                _diagnostics.AddError($"[{nameof(ProfilingBlockWeaver)}] Failed to parse '{SettingsFilePath}'");
                _diagnostics.AddError($"[{nameof(ProfilingBlockWeaver)}] {e.Message}");
            }
        }
    }
}
