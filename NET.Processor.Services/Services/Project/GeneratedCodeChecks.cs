using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NET.Processor.Core.Services.Project
{
    public static class GeneratedCodeChecks
    {
        public static bool IsGeneratedFile(this string filePath) =>
            Regex.IsMatch(filePath, @"(obj\\debug\\|obj\\release|bin\\debug|bin\\release|\\service|\\TemporaryGeneratedFile_.*|\\assemblyinfo|\\assemblyattributes|\.(g\.i|g|designer|generated|assemblyattributes))\.(cs|vb)$",
                RegexOptions.IgnoreCase);
    }
}
