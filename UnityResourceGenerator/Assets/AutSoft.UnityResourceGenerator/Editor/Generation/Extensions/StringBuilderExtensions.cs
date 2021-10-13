﻿using System.Collections.Generic;
using System.Text;

namespace AutSoft.UnityResourceGenerator.Editor.Generation.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendMultipleLines(this StringBuilder builder, IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            return builder;
        }
    }
}