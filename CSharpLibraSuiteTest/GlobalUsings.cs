global using NUnit.Framework;
global using CSharpLibraSuite.WinRegistry;
using Microsoft.Win32;

namespace CSharpLibraSuiteTest
{
    public static class GlobalConstants
    {
        /// <summary>
        /// Represents the global test path for registry tests.
        /// </summary>
        public const string WinRegTestsRootPath = @"Software\CSharpLibraSuite";

        /// <summary>
        /// Represents the global test hive for registry tests.
        /// </summary>
        public const RegistryHive WinRegTestsRootHive = RegistryHive.CurrentUser;
    }
}