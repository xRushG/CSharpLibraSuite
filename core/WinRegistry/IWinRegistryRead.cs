using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using core.WinRegistry.RegEntry;

namespace core.WinRegistry
{
    [SupportedOSPlatform("windows")]
    public interface IWinRegistryRead
    {
        #region registry reader
        string[] GetSubKeyNames(RegistryHive hive, string path);

        string GetStringValue(RegistryHive hive, string path, string name, string defaultValue = null);
        bool GetBoolValue(RegistryHive hive, string path, string propertyName, bool defaultValue = false);
        int GetDwordValue(RegistryHive hive, string path, string propertyName, int defaultValue = 0);

        Entry GetRegistryEntry(RegistryHive hive, string path, string name);
        List<Entry> GetRegistryEntries(RegistryHive hive, string path);
        List<Entry> GetRegistryEntriesRecursive(RegistryHive hive, string path);
        #endregion

        #region registry tools
        RegistryHive ConvertStringToRegistryHive(string hiveString);
        RegistryValueKind ConvertStringToRegistryValueKind(string valueType);
        RegistryValueKind ConvertTypeToRegistryValueKind(Type valueType);
        Type ConvertRegistryValueKindToType(RegistryValueKind valueKind);
        #endregion
    }
}