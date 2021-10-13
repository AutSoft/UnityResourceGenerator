﻿using AutSoft.UnityResourceGenerator.Editor.Generation.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutSoft.UnityResourceGenerator.Editor.Generation.Modules
{
    public sealed class AllResources : IModuleGenerator
    {
        public string Generate(ResourceContext context) =>
            new StringBuilder()
                .AppendMultipleLines(context.Data.Select(d => Generate(context, d)))
                .ToString();

        private static string Generate(ResourceContext context, IResourceData data)
        {
            context.Info($"Started generating {data.ClassName}");

            // ReSharper disable once MissingIndent
            var classBegin =
$@"
        public static partial class {data.ClassName}
        {{
";
            // ReSharper disable once MissingIndent
            const string classEnd = "        }";

            var values = Directory
                .EnumerateFiles(context.AssetsFolder, data.FileExtension, SearchOption.AllDirectories)
                .Select(filePath =>
                {
                    var (canLoad, baseFolder) = GetBaseFolder(filePath, data.IsResource, context);
                    if (!canLoad) return (null, null);

                    var resourcePath = filePath
                        .Replace(baseFolder, string.Empty)
                        .Replace('\\', '/')
                        .Remove(0, 1);

                    resourcePath = Path.Combine
                        (
                            Path.GetDirectoryName(resourcePath) ?? string.Empty,
                            Path.GetFileNameWithoutExtension(resourcePath)
                        )
                        .Replace('\\', '/');
                    return
                    (
                        name: Path.GetFileNameWithoutExtension(filePath),
                        path: resourcePath
                    );
                })
                .Where(p => p.name != null)
                .ToArray();

            var duplicates = values.Duplicates(v => v.name).ToArray();

            if (duplicates.Length > 0)
            {
                context.Error(duplicates.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v.name).Append(' ').AppendLine(v.path)).ToString());
                throw new InvalidOperationException("Found duplicate file names");
            }

            var output = values
                .Aggregate(
                    new StringBuilder().Append(classBegin),
                    (sb, s) => sb
                        .Append("            public const string ")
                        .Append(s.name)
                        .Append(" = \"")
                        .Append(s.path)
                        .AppendLine("\";"))
                .AppendLine(classEnd)
                .ToString();

            context.Info($"Finished generating {data.ClassName}");

            return output;
        }

        private static (bool canLoad, string baseFolder) GetBaseFolder(string filePath, bool isResource, ResourceContext context)
        {
            if (!isResource) return (true, context.AssetsFolder);

            var parents = GetAllParentDirectories(filePath);
            var resourcesFolder = parents.LastOrDefault(p => p.Name == "Resources");

            return resourcesFolder is null
                ? (false, null)
                : (true, resourcesFolder.FullName);
        }

        private static IEnumerable<DirectoryInfo> GetAllParentDirectories(string directoryToScan)
        {
            var ret = new Stack<DirectoryInfo>();
            GetAllParentDirectories(new DirectoryInfo(directoryToScan), ref ret);
            return ret.ToList();
        }

        private static void GetAllParentDirectories(DirectoryInfo directoryToScan, ref Stack<DirectoryInfo> directories)
        {
            if (directoryToScan == null || directoryToScan.Name == directoryToScan.Root.Name) return;

            directories.Push(directoryToScan);
            GetAllParentDirectories(directoryToScan.Parent, ref directories);
        }
    }
}