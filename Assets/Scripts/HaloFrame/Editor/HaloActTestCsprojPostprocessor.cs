using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;

public class HaloActTestCsprojPostprocessor : AssetPostprocessor
{
    private const string EditModeTestsProjectName = "HaloAct.EditModeTests.csproj";

    private static readonly (string Include, string HintPath)[] RequiredReferences =
    {
        ("Sirenix.OdinInspector.Attributes", @"Assets\Plugins\Sirenix\Assemblies\Sirenix.OdinInspector.Attributes.dll"),
        ("Sirenix.Serialization.Config", @"Assets\Plugins\Sirenix\Assemblies\Sirenix.Serialization.Config.dll"),
        ("Sirenix.Utilities", @"Assets\Plugins\Sirenix\Assemblies\Sirenix.Utilities.dll"),
        ("Sirenix.Serialization", @"Assets\Plugins\Sirenix\Assemblies\Sirenix.Serialization.dll"),
    };

    // Unity 生成测试工程时会漏掉预定义主程序集和部分 Sirenix 依赖，导致 dotnet 构建无法解析玩法类型。
    public static string OnGeneratedCSProject(string path, string content)
    {
        if (!string.Equals(Path.GetFileName(path), EditModeTestsProjectName, StringComparison.OrdinalIgnoreCase))
        {
            return content;
        }

        var document = XDocument.Parse(content);
        var project = document.Root;
        if (project == null)
        {
            return content;
        }

        EnsureProjectReference(project, "Assembly-CSharp.csproj");
        EnsureRuntimeReferences(project);
        return document.ToString();
    }

    private static void EnsureProjectReference(XElement project, string include)
    {
        if (project.Descendants("ProjectReference").Any(element => IsInclude(element, include)))
        {
            return;
        }

        var itemGroup = new XElement("ItemGroup",
            new XElement("ProjectReference", new XAttribute("Include", include)));
        InsertBeforeLastImport(project, itemGroup);
    }

    private static void EnsureRuntimeReferences(XElement project)
    {
        var referenceGroup = project.Elements("ItemGroup")
            .FirstOrDefault(group => group.Elements("Reference").Any());
        if (referenceGroup == null)
        {
            referenceGroup = new XElement("ItemGroup");
            InsertBeforeLastImport(project, referenceGroup);
        }

        foreach (var (include, hintPath) in RequiredReferences)
        {
            if (referenceGroup.Elements("Reference").Any(element => IsInclude(element, include)))
            {
                continue;
            }

            referenceGroup.Add(
                new XElement("Reference",
                    new XAttribute("Include", include),
                    new XElement("HintPath", hintPath),
                    new XElement("Private", "False")));
        }
    }

    private static bool IsInclude(XElement element, string include)
    {
        return string.Equals((string)element.Attribute("Include"), include, StringComparison.OrdinalIgnoreCase);
    }

    private static void InsertBeforeLastImport(XElement project, XElement itemGroup)
    {
        var import = project.Elements("Import").LastOrDefault();
        if (import != null)
        {
            import.AddBeforeSelf(itemGroup);
            return;
        }

        project.Add(itemGroup);
    }
}
