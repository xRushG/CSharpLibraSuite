using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using core.WinRegistry.RegEntry;
using System.Linq;

namespace core.WinRegistry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Provides enhanced control and functionality to interact with the Windows Registry, extending the base WinRegistry class.
    /// </summary>
    /// <remarks>
    /// This class extends the functionality provided by the base WinRegistry class by incorporating additional features and control mechanisms for working with the Windows Registry.
    /// It includes methods for advanced registry operations, error handling, and security measures to ensure safe and efficient registry manipulation.
    /// Users can create instances of this class to leverage enhanced capabilities for reading, writing, and managing registry entries in a secure and controlled manner.
    /// </remarks>
    public class WinRegistryEC
    {
        #region Public Entry Methods: GetEntry, GetEntries, GetEntriesRecursive

        /// <summary>
        /// Retrieves a windows registry entry for a specific registry hive, path, and value name.
        /// </summary>
        /// <param name="hive">The RegistryHive of the key.</param>
        /// <param name="path">The path of the key.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A WindowsRegistryKey object representing the specified registry key and value.</returns>
        public Entry GetEntry(RegistryHive hive, string path, string name)
        {
            return Entry.New(hive, path, name).Read();
        }

        /// <summary>
        /// Retrieves a list of registry entries and their values under a given key path.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <returns>A list of WindowsRegistryKey objects, each representing a value within the specified registry key path.</returns>
        public List<Entry> GetEntries(RegistryHive hive, string path)
        {
            using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(path);
            if (key == null)
                return new List<Entry>(); // Return an empty list when no key is found

            return key.GetValueNames()
                .Select(name => Entry.New(hive, path, name).Read())
                .ToList();
        }

        /// <summary>
        /// Recursively retrieves registry entries under a given key path and its subkeys.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <returns>A list of WindowsRegistryKey objects, each representing a value within the specified registry key path.</returns>
        public List<Entry> GetEntriesRecursive(RegistryHive hive, string path)
        {
            List<Entry> list = new();
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (var key = baseKey.OpenSubKey(path))
            {
                if (key != null)
                {
                    foreach (string subPathName in key.GetSubKeyNames())
                    {
                        string subKey = $"{path}\\{subPathName}";
                        list.AddRange(GetEntriesRecursive(hive, subKey));
                    }
                }
            }

            list.AddRange(GetEntries(hive, path));
            return list;
        }
        #endregion

        #region Public IntegerEntry Methods: GetInteger

        /// <summary>
        /// Retrieves a REG_DWORD (32-bit integer) value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_DWORD value.</returns>
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int defaultValue = 0)
        {
            return IntegerEntry.New(hive, path, name, defaultValue).Read();
        }

        /// <summary>
        /// Retrieves an REG_DWORD (32-bit integer) value from the windows registry with validation based on the specified enumeration type.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type for validation.</typeparam>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_DWORD value.</returns>
        public IntegerEntry GetInteger<TEnum>(RegistryHive hive, string path, string name, int defaultValue = 0) where TEnum : Enum
        {
            return IntegerEntry.New(hive, path, name, defaultValue).SetValidation<TEnum>().Read();
        }

        /// <summary>
        /// Retrieves an REG_DWORD (32-bit integer) value from the windows registry with validation based on a set of allowed values.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_DWORD value.</returns>
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int[] allowedValues, int defaultValue = 0)
        {
            return IntegerEntry.New(hive, path, name, defaultValue).SetValidation(allowedValues).Read();
        }

        /// <summary>
        /// Retrieves an REG_DWORD (32-bit integer) value from the windows registry with validation based on a specified range.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_DWORD value.</returns>
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int minimum, int maximum, int defaultValue = 0)
        {
            return IntegerEntry.New(hive, path, name, defaultValue).SetValidation(minimum, maximum).Read();
        }

        #endregion

        #region Public LongIntEntry Methods: GetLongInt

        /// <summary>
        /// Retrieves a REG_QWORD (64-bit integer) value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_QWORD value.</returns>
        public LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long defaultValue = 0)
        {
            return LongIntEntry.New(hive, path, name, defaultValue).Read();
        }

        /// <summary>
        /// Retrieves an REG_QWORD (64-bit integer) value from the windows registry with validation based on a set of allowed values.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_QWORD value.</returns>
        public LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long[] allowedValues, long defaultValue = 0)
        {
            return LongIntEntry.New(hive, path, name, defaultValue).SetValidation(allowedValues).Read();
        }

        /// <summary>
        /// Retrieves an REG_QWORD (64-bit integer) value from the windows registry with validation based on a specified range.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="minimum">The minimum allowed value.</param>
        /// <param name="maximum">The maximum allowed value.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_QWORD value.</returns>
        public LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long minimum, long maximum, long defaultValue = 0)
        {
            return LongIntEntry.New(hive, path, name, defaultValue).SetValidation(minimum, maximum).Read();
        }

        #endregion

        #region Public StringEntry Methods: GetString

        /// <summary>
        /// Retrieves a REG_SZ value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyString instance representing the retrieved REG_SZ value.</returns>
        public StringEntry GetString(RegistryHive hive, string path, string name, string defaultValue = null)
        {
            return StringEntry.New(hive, path, name, defaultValue).Read();
        }

        /// <summary>
        /// Retrieves a REG_SZ value from the windows registry based on the specified registry information and validates it against a set of allowed values.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="allowedValues">The array of allowed string values.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyString instance representing the retrieved REG_SZ value.</returns>
        public StringEntry GetString(RegistryHive hive, string path, string name, string[] allowedValues, string defaultValue = null)
        {
            return StringEntry.New(hive, path, name, defaultValue).SetValidation(allowedValues).Read();
        }

        /// <summary>
        /// Retrieves a REG_SZ value from the windows registry and validates it against the enum values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to validate the registry value against.</typeparam>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyString instance representing the retrieved REG_SZ value.</returns>
        public StringEntry GetString<TEnum>(RegistryHive hive, string path, string name, string defaultValue = null) where TEnum : Enum
        {
            return StringEntry.New(hive, path, name, defaultValue).SetValidation<TEnum>().Read();
        }

        #endregion

        #region Public BoolEntry Methods: GetBoolean

        /// <summary>
        /// Retrieves a boolean (REG_SZ or REG_DWORD) value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyString instance representing the retrieved REG_SZ or REG_DWORD as bool value.</returns>
        public BoolEntry GetBoolean(RegistryHive hive, string path, string name, bool defaultValue = false)
        {
            return BoolEntry.New(hive, path, name, defaultValue).Read();
        }

        #endregion
    }
}