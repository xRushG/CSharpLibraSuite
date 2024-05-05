using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace core.WinRegistry
{
    [SupportedOSPlatform("windows")]
    public interface IWinRegistryRead
    {
        #region Registry Reader

        /// <summary>
        /// Gets the names of subkeys under the specified registry hive and path.
        /// </summary>
        string[] GetSubKeyNames(RegistryHive hive, string path);

        /// <summary>
        /// Gets the value of a registry entry specified by its name.
        /// </summary>
        string GetValue(RegistryHive hive, string path, string name);

        /// <summary>
        /// Gets the string value of a registry entry specified by its name.
        /// </summary>
        string GetStringValue(RegistryHive hive, string path, string name, string defaultValue = null);

        /// <summary>
        /// Gets the boolean value of a registry entry specified by its name.
        /// </summary>
        bool GetBoolValue(RegistryHive hive, string path, string propertyName, bool defaultValue = false);

        /// <summary>
        /// Gets the DWORD value of a registry entry specified by its name.
        /// </summary>
        int GetIntegerValue(RegistryHive hive, string path, string propertyName, int defaultValue = -1);

        #endregion

        #region Registry Tools

        /// <summary>
        /// Converts a string representation of a registry hive to the corresponding RegistryHive enum value.
        /// </summary>
        RegistryHive ConvertStringToRegistryHive(string hiveString);

        /// <summary>
        /// Converts a string representation of a registry value kind to the corresponding RegistryValueKind enum value.
        /// </summary>
        RegistryValueKind ConvertStringToRegistryValueKind(string valueType);

        /// <summary>
        /// Converts a .NET type to the corresponding RegistryValueKind enum value.
        /// </summary>
        RegistryValueKind ConvertTypeToRegistryValueKind(Type valueType);

        /// <summary>
        /// Converts a RegistryValueKind enum value to the corresponding .NET type.
        /// </summary>
        Type ConvertRegistryValueKindToType(RegistryValueKind valueKind);

        #endregion
    }
}