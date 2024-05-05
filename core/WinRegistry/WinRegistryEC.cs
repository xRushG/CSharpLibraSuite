using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using core.WinRegistry.RegEntry;

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
    public class WinRegistryEC : WinRegistry
    {
        #region Public Entry Methods: GetEntry, GetEntries, GetEntriesRecursive

        /// <summary>
        /// Retrieves a windows registry entry for a specific registry hive, path, and value name.
        /// </summary>
        /// <param name="hive">The RegistryHive of the key.</param>
        /// <param name="path">The path of the key.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A WindowsRegistryKey object representing the specified registry key and value.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive, path or name is invalid</exception>
        public Entry GetEntry(RegistryHive hive, string path, string name)
        {
            ThrowIfHiveInvalid(hive);
            ThrowIfPathInvalid(path);
            ThrowIfNameInvalid(name);

            Entry WinRegistryKey = new(hive, path, name); // Create an Entry object
            WinRegistryKey.Read(); // Read the registry entry

            return WinRegistryKey;
        }

        /// <summary>
        /// Retrieves a list of registry entries and their values under a given key path.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <returns>A list of WindowsRegistryKey objects, each representing a value within the specified registry key path.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive or path is invalid</exception>
        public List<Entry> GetEntries(RegistryHive hive, string path)
        {
            ThrowIfHiveInvalid(hive);
            ThrowIfPathInvalid(path);

            List<Entry> list = new();
            using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(path);
            {
                if (key != null)
                {
                    foreach (string name in key.GetValueNames())
                    {
                        list.Add(new Entry
                        {
                            Hive = hive,
                            Path = path,
                            Name = name,
                            Value = key.GetValue(name).ToString(),
                            ValueKind = key.GetValueKind(name)
                        }
                        );
                    }
                }

            }
            return list;
        }

        /// <summary>
        /// Recursively retrieves registry entries under a given key path and its subkeys.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <returns>A list of WindowsRegistryKey objects, each representing a value within the specified registry key path.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive or path is invalid</exception>
        public List<Entry> GetEntriesRecursive(RegistryHive hive, string path)
        {
            ThrowIfHiveInvalid(hive);
            ThrowIfPathInvalid(path);

            List<Entry> list = GetEntries(hive, path);
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default), key = baseKey.OpenSubKey(path))
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
            IntegerEntry IntegerEntry = new(hive, path, name, defaultValue);
            IntegerEntry.Read();
            return IntegerEntry;
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
            IntegerEntry IntegerEntry = new(hive, path, name, defaultValue);
            IntegerEntry.SetValidation<TEnum>();
            IntegerEntry.Read();

            return IntegerEntry;
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
            IntegerEntry IntegerEntry = new(hive, path, name, defaultValue);
            IntegerEntry.SetValidation(allowedValues);
            IntegerEntry.Read();
            return IntegerEntry;
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
            IntegerEntry IntegerEntry = new(hive, path, name, defaultValue);
            IntegerEntry.SetValidation(minimum, maximum);
            IntegerEntry.Read();
            return IntegerEntry;
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
            LongIntEntry LongIntEntry = new(hive, path, name, defaultValue);
            LongIntEntry.Read();
            return LongIntEntry;
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
            LongIntEntry LongIntEntry = new(hive, path, name, defaultValue);
            LongIntEntry.SetValidation(allowedValues);
            LongIntEntry.Read();
            return LongIntEntry;
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
            LongIntEntry LongIntEntry = new(hive, path, name, defaultValue);
            LongIntEntry.SetValidation(minimum, maximum);
            LongIntEntry.Read();
            return LongIntEntry;
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
            StringEntry StringEntry = new(hive, path, name, defaultValue);
            StringEntry.Read();
            return StringEntry;
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
            StringEntry StringEntry = new(hive, path, name, defaultValue);
            StringEntry.SetValidation(allowedValues);
            StringEntry.Read();
            return StringEntry;
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
            StringEntry StringEntry = new(hive, path, name, defaultValue);
            StringEntry.SetValidation<TEnum>();
            StringEntry.Read();
            return StringEntry;
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
            BoolEntry BoolEntry = new(hive, path, name, defaultValue);
            BoolEntry.Read();
            return BoolEntry;
        }

        #endregion
    }
}