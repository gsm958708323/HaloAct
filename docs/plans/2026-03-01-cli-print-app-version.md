# CLI Print App Version Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 添加一个可通过命令行调用的 CLI 命令，用于打印当前应用版本号并以 0 退出。

**Architecture:** 版本来源以 Unity 的 `PlayerSettings.bundleVersion` 为准（在 Editor/batchmode 下可稳定读取）。提供一个纯 C# 的版本字符串规范化函数（便于测试），再提供一个可被 `-executeMethod` 调用的静态入口 `HaloFrame.Cli.PrintVersion()`，负责输出并退出。

**Tech Stack:** Unity 2021.3, C#, UnityEditor batchmode (`-executeMethod`), Unity Test Framework (EditMode), NUnit.

---

### Task 1: 版本字符串规范化（纯逻辑）+ EditMode 测试

**Files:**
- Create: `Assets/Scripts/HaloFrame/Runtime/Cli/AppVersion.cs`
- Create: `Assets/Tests/EditMode/Cli/AppVersionTests.cs`

**Step 1: Write the failing test**

```csharp
using NUnit.Framework;

namespace Ability.Tests
{
    public class AppVersionTests
    {
        [Test]
        public void Normalize_Null_ReturnsUnknown()
        {
            Assert.AreEqual("unknown", HaloFrame.AppVersion.Normalize(null));
        }

        [Test]
        public void Normalize_Whitespace_ReturnsUnknown()
        {
            Assert.AreEqual("unknown", HaloFrame.AppVersion.Normalize("   ")); 
        }

        [Test]
        public void Normalize_Trim_ReturnsTrimmed()
        {
            Assert.AreEqual("1.2.3", HaloFrame.AppVersion.Normalize("  1.2.3  "));
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.AppVersionTests" -testResults "Logs/EditMode-AppVersionTests.xml"`

Expected: FAIL because `HaloFrame.AppVersion` does not exist.

**Step 3: Write minimal implementation**

```csharp
namespace HaloFrame
{
    public static class AppVersion
    {
        public static string Normalize(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "unknown";
            }

            return version.Trim();
        }
    }
}
```

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Scripts/HaloFrame/Runtime/Cli/AppVersion.cs" "Assets/Tests/EditMode/Cli/AppVersionTests.cs"
git commit -m "test: add app version normalization contract"
```

### Task 2: 增加可执行的 CLI 命令入口（-executeMethod）

**Files:**
- Create: `Assets/Scripts/HaloFrame/Runtime/Cli/Cli.cs`
- Create: `Assets/Tests/EditMode/Cli/CliCommandContractTests.cs`

**Step 1: Write the failing test**

```csharp
using System.Reflection;
using NUnit.Framework;

namespace Ability.Tests
{
    public class CliCommandContractTests
    {
        [Test]
        public void Cli_MustExpose_PrintVersion_ExecuteMethodEntryPoint()
        {
            var t = typeof(HaloFrame.Cli);
            var m = t.GetMethod("PrintVersion", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(m);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.CliCommandContractTests" -testResults "Logs/EditMode-CliCommandContractTests.xml"`

Expected: FAIL because `HaloFrame.Cli` does not exist.

**Step 3: Write minimal implementation**

```csharp
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HaloFrame
{
    public static class Cli
    {
        // 用法（Unity Editor 批处理/CI）：
        // "<UNITY_EXE>" -batchmode -nographics -quit -projectPath "..." -executeMethod HaloFrame.Cli.PrintVersion
        public static void PrintVersion()
        {
#if UNITY_EDITOR
            var version = AppVersion.Normalize(PlayerSettings.bundleVersion);
            Debug.Log(version);
            EditorApplication.Exit(0);
#else
            Debug.Log(AppVersion.Normalize(Application.version));
            Application.Quit(0);
#endif
        }
    }
}
```

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "Assets/Scripts/HaloFrame/Runtime/Cli/Cli.cs" "Assets/Tests/EditMode/Cli/CliCommandContractTests.cs"
git commit -m "feat: add CLI entrypoint to print app version"
```

### Task 3: 文档化 CLI 用法（README）

**Files:**
- Modify: `README.md`

**Step 1: Write the failing test**

Add a minimal doc contract test that ensures README mentions the execute method string.

```csharp
using System.IO;
using NUnit.Framework;

namespace Ability.Tests
{
    public class ReadmeCliDocsTests
    {
        [Test]
        public void Readme_Mentions_PrintVersion_ExecuteMethod()
        {
            var readme = File.ReadAllText("README.md");
            StringAssert.Contains("-executeMethod HaloFrame.Cli.PrintVersion", readme);
        }
    }
}
```

**Step 2: Run test to verify it fails**

Run:
`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -runTests -testPlatform EditMode -testFilter "Ability.Tests.ReadmeCliDocsTests" -testResults "Logs/EditMode-ReadmeCliDocsTests.xml"`

Expected: FAIL because README does not mention the command.

**Step 3: Write minimal implementation**

Update `README.md` by adding a short section (Chinese) such as:

```text
打印当前版本号（Editor/batchmode）

"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\Work\UnityProject\HaloAct" -executeMethod HaloFrame.Cli.PrintVersion
```

**Step 4: Run test to verify it passes**

Run same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add "README.md" "Assets/Tests/EditMode/Cli/ReadmeCliDocsTests.cs"
git commit -m "docs: document print-version CLI command"
```

---

## 手动验证（不走测试）

运行（会打印一行版本号后退出 0）：

`"<UNITY_EXE>" -batchmode -nographics -quit -projectPath "D:\\Work\\UnityProject\\HaloAct" -executeMethod HaloFrame.Cli.PrintVersion -logFile "Logs/PrintVersion.log"`

Expected: `Logs/PrintVersion.log` 中包含一行类似 `1.0.0` 的输出。
