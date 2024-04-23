using core.WindowsRegistry.entry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Core.WindowsRegistry
{
    [SupportedOSPlatform("windows")]
    public interface IWinRegistryRead
    {
        #region registry reader
        string[] GetSubKeyNames(RegistryHive hive, string path);

        string GetStringValue(RegistryHive hive, string path, string name, string defaultValue = null);
        bool GetBoolValue(RegistryHive hive, string path, string propertyName, bool defaultValue = false);
        int GetDwordValue(RegistryHive hive, string path, string propertyName, int defaultValue = 0);

        WinRegistryEntry GetRegistryEntry(RegistryHive hive, string path, string name);
        List<WinRegistryEntry> GetRegistryEntries(RegistryHive hive, string path);
        List<WinRegistryEntry> GetRegistryEntriesRecursive(RegistryHive hive, string path);
        #endregion

        #region registry tools
        RegistryHive ConvertStringToRegistryHive(string hiveString);
        RegistryValueKind ConvertStringToRegistryValueKind(string valueType);
        RegistryValueKind ConvertTypeToRegistryValueKind(Type valueType);
        Type ConvertRegistryValueKindToType(RegistryValueKind valueKind);
        #endregion
    }
}