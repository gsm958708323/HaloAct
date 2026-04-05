using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame.Editor
{
    public static class HaloActEditModeBatchRunner
    {
        static TestRunnerApi activeApi;
        static HaloActEditModeBatchCallbacks activeCallbacks;

        [MenuItem("Tools/HaloFrame/Tests/Run All EditMode Tests")]
        public static void RunAllEditModeTests()
        {
            StartRun(HaloActEditModeBatchArguments.Parse(Array.Empty<string>(), GetProjectPath()));
        }

        public static void RunFromCommandLine()
        {
            if (Application.isBatchMode)
            {
                throw new InvalidOperationException(
                    "Batch mode should use Unity -runTests or tools/Run-HaloActEditModeTests.ps1. TestRunnerApi executeMethod exits before asynchronous callbacks complete.");
            }

            var projectPath = GetProjectPath();

            try
            {
                StartRun(HaloActEditModeBatchArguments.Parse(Environment.GetCommandLineArgs(), projectPath));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        static void StartRun(HaloActEditModeBatchRequest request)
        {
            if (activeApi != null)
            {
                throw new InvalidOperationException("HaloAct EditMode batch runner is already running.");
            }

            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeCallbacks = new HaloActEditModeBatchCallbacks(request, CleanupActiveRun);
            activeApi.RegisterCallbacks(activeCallbacks);

            try
            {
                Debug.Log(
                    $"[HaloActTestRunner] Starting EditMode tests. Assemblies={FormatValues(request.AssemblyNames)} Names={FormatValues(request.TestNames)} Filters={FormatValues(request.GroupNames)} Categories={FormatValues(request.CategoryNames)} Sync={request.RunSynchronously}");
                activeApi.Execute(HaloActEditModeBatchArguments.BuildExecutionSettings(request));
            }
            catch (Exception exception)
            {
                activeCallbacks.HandleInfrastructureFailure(exception);
            }
        }

        static void CleanupActiveRun()
        {
            if (activeApi != null && activeCallbacks != null)
            {
                activeApi.UnregisterCallbacks(activeCallbacks);
            }

            if (activeApi != null)
            {
                Object.DestroyImmediate(activeApi);
            }

            activeApi = null;
            activeCallbacks = null;
        }

        static string GetProjectPath()
        {
            return Path.GetDirectoryName(Application.dataPath)?.Replace("\\", "/") ?? string.Empty;
        }

        static string FormatValues(string[] values)
        {
            return values == null || values.Length == 0 ? "<none>" : string.Join(";", values);
        }
    }

    internal static class HaloActEditModeBatchArguments
    {
        const string DefaultSummaryFileName = "haloact-editmode-summary.json";

        public static HaloActEditModeBatchRequest Parse(string[] args, string projectPath)
        {
            var request = new HaloActEditModeBatchRequest
            {
                AssemblyNames = Array.Empty<string>(),
                TestNames = Array.Empty<string>(),
                GroupNames = Array.Empty<string>(),
                CategoryNames = Array.Empty<string>(),
                RunSynchronously = true
            };

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-haloTestAssembly":
                    case "-haloTestAssemblies":
                    case "-haloTestAssemblyNames":
                        request.AssemblyNames = ReadValues(args, ref i, args[i]);
                        break;
                    case "-haloTestName":
                    case "-haloTestNames":
                        request.TestNames = ReadValues(args, ref i, args[i]);
                        break;
                    case "-haloTestFilter":
                    case "-haloTestGroup":
                    case "-haloTestGroups":
                        request.GroupNames = ReadValues(args, ref i, args[i]);
                        break;
                    case "-haloTestCategory":
                    case "-haloTestCategories":
                    case "-haloTestCategoryNames":
                        request.CategoryNames = ReadValues(args, ref i, args[i]);
                        break;
                    case "-haloTestSummary":
                    case "-haloTestSummaryPath":
                    case "-haloTestResultJson":
                        request.SummaryPath = ReadPath(args, ref i, args[i], projectPath);
                        break;
                    case "-haloTestRunSynchronously":
                        request.RunSynchronously = ReadBool(args, ref i, args[i]);
                        break;
                }
            }

            request.SummaryPath = string.IsNullOrWhiteSpace(request.SummaryPath)
                ? GetDefaultSummaryPath(projectPath)
                : request.SummaryPath;

            return request;
        }

        public static ExecutionSettings BuildExecutionSettings(HaloActEditModeBatchRequest request)
        {
            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            if (request.AssemblyNames.Length > 0)
            {
                filter.assemblyNames = request.AssemblyNames;
            }

            if (request.TestNames.Length > 0)
            {
                filter.testNames = request.TestNames;
            }

            if (request.GroupNames.Length > 0)
            {
                filter.groupNames = request.GroupNames;
            }

            if (request.CategoryNames.Length > 0)
            {
                filter.categoryNames = request.CategoryNames;
            }

            return new ExecutionSettings(filter)
            {
                runSynchronously = request.RunSynchronously
            };
        }

        static string[] ReadValues(string[] args, ref int index, string flag)
        {
            return SplitValues(ReadRawValue(args, ref index, flag));
        }

        static string ReadPath(string[] args, ref int index, string flag, string projectPath)
        {
            var rawPath = ReadRawValue(args, ref index, flag);
            if (Path.IsPathRooted(rawPath))
            {
                return rawPath.Replace("\\", "/");
            }

            return PathTools.Combine(projectPath, rawPath);
        }

        static bool ReadBool(string[] args, ref int index, string flag)
        {
            var rawValue = ReadRawValue(args, ref index, flag);
            if (bool.TryParse(rawValue, out var parsed))
            {
                return parsed;
            }

            if (rawValue == "1")
            {
                return true;
            }

            if (rawValue == "0")
            {
                return false;
            }

            throw new ArgumentException($"Value '{rawValue}' is not a valid boolean for {flag}.");
        }

        static string ReadRawValue(string[] args, ref int index, string flag)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for {flag}.");
            }

            index += 1;
            return args[index];
        }

        static string[] SplitValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            var values = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }

            return values;
        }

        internal static string GetDefaultSummaryPath(string projectPath)
        {
            return PathTools.Combine(projectPath, "TestArtifacts", "TestRunner", DefaultSummaryFileName);
        }
    }

    [Serializable]
    internal sealed class HaloActEditModeBatchRequest
    {
        public string[] AssemblyNames { get; set; } = Array.Empty<string>();
        public string[] TestNames { get; set; } = Array.Empty<string>();
        public string[] GroupNames { get; set; } = Array.Empty<string>();
        public string[] CategoryNames { get; set; } = Array.Empty<string>();
        public string SummaryPath { get; set; }
        public bool RunSynchronously { get; set; }
    }

    internal sealed class HaloActEditModeBatchCallbacks : ICallbacks
    {
        readonly HaloActEditModeBatchRequest request;
        readonly Action onCompleted;
        readonly List<HaloActEditModeBatchFailure> failures = new List<HaloActEditModeBatchFailure>();
        readonly DateTime startedAtUtc = DateTime.UtcNow;
        bool completed;

        public HaloActEditModeBatchCallbacks(HaloActEditModeBatchRequest request, Action onCompleted)
        {
            this.request = request;
            this.onCompleted = onCompleted;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log($"[HaloActTestRunner] Loaded {testsToRun.TestCaseCount} EditMode test case(s).");
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (completed || result.Test == null || result.Test.IsSuite || result.HasChildren)
            {
                return;
            }

            if (result.TestStatus != TestStatus.Failed)
            {
                return;
            }

            failures.Add(new HaloActEditModeBatchFailure
            {
                FullName = result.FullName,
                ResultState = result.ResultState,
                Message = result.Message,
                StackTrace = result.StackTrace,
                DurationSeconds = result.Duration
            });
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            Complete(BuildSummary(result));
        }

        public void HandleInfrastructureFailure(Exception exception)
        {
            var summary = new HaloActEditModeBatchSummary
            {
                Succeeded = false,
                InfrastructureFailure = true,
                AssemblyNames = request.AssemblyNames,
                TestNames = request.TestNames,
                GroupNames = request.GroupNames,
                CategoryNames = request.CategoryNames,
                SummaryPath = request.SummaryPath,
                RunSynchronously = request.RunSynchronously,
                StartedAtUtc = startedAtUtc.ToString("o"),
                FinishedAtUtc = DateTime.UtcNow.ToString("o"),
                DurationSeconds = (DateTime.UtcNow - startedAtUtc).TotalSeconds,
                ResultState = "InfrastructureFailure",
                Message = exception.Message,
                Failures = new[]
                {
                    new HaloActEditModeBatchFailure
                    {
                        FullName = "Infrastructure",
                        ResultState = "InfrastructureFailure",
                        Message = exception.Message,
                        StackTrace = exception.ToString(),
                        DurationSeconds = 0d
                    }
                }
            };

            Complete(summary);
        }

        HaloActEditModeBatchSummary BuildSummary(ITestResultAdaptor result)
        {
            return new HaloActEditModeBatchSummary
            {
                Succeeded = result.FailCount == 0 && result.TestStatus != TestStatus.Failed,
                InfrastructureFailure = false,
                AssemblyNames = request.AssemblyNames,
                TestNames = request.TestNames,
                GroupNames = request.GroupNames,
                CategoryNames = request.CategoryNames,
                SummaryPath = request.SummaryPath,
                RunSynchronously = request.RunSynchronously,
                StartedAtUtc = startedAtUtc.ToString("o"),
                FinishedAtUtc = DateTime.UtcNow.ToString("o"),
                DurationSeconds = result.Duration,
                ResultState = result.ResultState,
                Message = result.Message,
                TotalCount = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount,
                PassedCount = result.PassCount,
                FailedCount = result.FailCount,
                SkippedCount = result.SkipCount,
                InconclusiveCount = result.InconclusiveCount,
                Failures = failures.ToArray()
            };
        }

        void Complete(HaloActEditModeBatchSummary summary)
        {
            if (completed)
            {
                return;
            }

            completed = true;

            try
            {
                WriteSummary(summary);
                Debug.Log(
                    $"[HaloActTestRunner] Finished. Passed={summary.PassedCount} Failed={summary.FailedCount} Skipped={summary.SkippedCount} Inconclusive={summary.InconclusiveCount} Summary={summary.SummaryPath}");

                if (summary.Failures != null && summary.Failures.Length > 0)
                {
                    var firstFailure = summary.Failures[0];
                    Debug.LogError($"[HaloActTestRunner] First failure: {firstFailure.FullName} :: {firstFailure.Message}");
                }
            }
            finally
            {
                onCompleted?.Invoke();

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(summary.Succeeded ? 0 : 1);
                }
            }
        }

        void WriteSummary(HaloActEditModeBatchSummary summary)
        {
            var directory = Path.GetDirectoryName(summary.SummaryPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(summary.SummaryPath, JsonUtility.ToJson(summary, true));
        }
    }

    [Serializable]
    internal sealed class HaloActEditModeBatchSummary
    {
        public bool Succeeded;
        public bool InfrastructureFailure;
        public string[] AssemblyNames = Array.Empty<string>();
        public string[] TestNames = Array.Empty<string>();
        public string[] GroupNames = Array.Empty<string>();
        public string[] CategoryNames = Array.Empty<string>();
        public string SummaryPath;
        public bool RunSynchronously;
        public string StartedAtUtc;
        public string FinishedAtUtc;
        public double DurationSeconds;
        public string ResultState;
        public string Message;
        public int TotalCount;
        public int PassedCount;
        public int FailedCount;
        public int SkippedCount;
        public int InconclusiveCount;
        public HaloActEditModeBatchFailure[] Failures = Array.Empty<HaloActEditModeBatchFailure>();
    }

    [Serializable]
    internal sealed class HaloActEditModeBatchFailure
    {
        public string FullName;
        public string ResultState;
        public string Message;
        public string StackTrace;
        public double DurationSeconds;
    }
}
