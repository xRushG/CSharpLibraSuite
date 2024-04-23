using core.WindowsRegistry;
using core.WindowsRegistry.entry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Security;

namespace Core.WindowsRegistry
{
    [SupportedOSPlatform("windows")]
    public class WinRegistry : IWinRegistry, IWinRegistryRead
    {
        #region public read
        /// <summary>
        /// Retrieves the names of subkeys under a specified registry key path.
        /// </summary>
        /// <param name="hive">The RegistryHive where the subkeys are located.</param>
        /// <param name="path">The path to the registry key containing the subkeys.</param>
        /// <returns>An array of strings containing the names of subkeys, or an empty array if no subkeys are found.</returns>
        /// /// <exception cref="ArgumentException">Thrown when the specified registry hive or path invalid</exception>
        public string[] GetSubKeyNames(RegistryHive hive, string path)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);

            using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(path);
            if (key != null)
                return key.GetSubKeyNames();
            else
                return Array.Empty<string>();
        }

        /// <summary>
        /// Retrieves the string value from the specified REG_SZ registry key.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value to return if the property is not found.</param>
        /// <returns>The value data as string, or the specified default value if the value is not found.</returns>
        public string GetStringValue(RegistryHive hive, string path, string name, string defaultValue = null)
        {
            string value = GetValue(hive, path, name);
            return value ?? defaultValue;
        }

        /// <summary>
        /// Retrieves the bool value from from the specified REG_SZ or REG_DWORD registry key.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value to return if the property is not found or cannot be parsed. (Default = false)</param>
        /// <returns>The value data as bool, parsed from its REG_SZ or REG_DWORD representation if possible, or the specified default value if the value is not found or cannot be parsed.</returns>
        public bool GetBoolValue(RegistryHive hive, string path, string name, bool defaultValue = false)
        {
            string value = GetValue(hive, path, name);

            if (!string.IsNullOrEmpty(value))
            {
                if (int.TryParse(value, out int intValue))
                    return intValue == 1;
                if (bool.TryParse(value, out bool boolValue))
                    return boolValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Retrieves the integer value from from the specified REG_DWORD registry key.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value to return if the property is not found or cannot be parsed. (Default = 0)</param>
        /// <returns>The value data as integer, parsed from its REG_DWORD representation if possible, or the specified default value if the value is not found or cannot be parsed.</returns>
        public int GetDwordValue(RegistryHive hive, string path, string name, int defaultValue = -1)
        {
            string value = GetValue(hive, path, name);

            if (int.TryParse(value, out int intValue))
                return intValue;

            return defaultValue;
        }

        /// <summary>
        /// Retrieves a windows registry entry for a specific registry hive, path, and value name.
        /// </summary>
        /// <param name="hive">The RegistryHive of the key.</param>
        /// <param name="path">The path of the key.</param>
        /// <param name="name">The name of the value to retrieve.</param>
        /// <returns>A WindowsRegistryKey object representing the specified registry key and value.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive, path or name is invalid</exception>
        public WinRegistryEntry GetRegistryEntry(RegistryHive hive, string path, string name)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);
            WinRegistryErrorMessages.ThrowIfNameInvalid(name);

            WinRegistryEntry registryEntry = new()
            {
                Hive = hive,
                Path = path,
                Name = name
            };

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(registryEntry.Hive, RegistryView.Default))
            using (RegistryKey subKey = baseKey.OpenSubKey(registryEntry.Path))
            {
                if (subKey != null)
                {
                    var value = subKey.GetValue(registryEntry.Name);
                    if (value != null)
                    {
                        registryEntry.Value = value.ToString();
                        registryEntry.ValueKind = subKey.GetValueKind(registryEntry.Name);
                    }
                }
            }

            return registryEntry;
        }

        /// <summary>
        /// Retrieves a list of registry entries and their values under a given key path.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <returns>A list of WindowsRegistryKey objects, each representing a value within the specified registry key path.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive or path is invalid</exception>
        public List<WinRegistryEntry> GetRegistryEntries(RegistryHive hive, string path)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);

            List<WinRegistryEntry> list = new();
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default), key = baseKey.OpenSubKey(path))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        list.Add(new WinRegistryEntry
                        {
                                Hive = hive,
                                Path = path,
                                Name = valueName,
                                Value = key.GetValue(valueName).ToString(),
                                ValueKind = key.GetValueKind(valueName)
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
        public List<WinRegistryEntry> GetRegistryEntriesRecursive(RegistryHive hive, string path)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);

            List<WinRegistryEntry> list = GetRegistryEntries(hive, path);
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default), key = baseKey.OpenSubKey(path))
            {
                if (key != null)
                {
                    foreach (string subPathName in key.GetSubKeyNames())
                    {
                        string subKey = $"{path}\\{subPathName}";
                        list.AddRange(GetRegistryEntriesRecursive(hive, subKey));
                    }
                }
            }

            return list;
        }
        #endregion

        #region public write
        /// <summary>
        /// Sets the value of a specific property within a registry key using individual parameters.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value to set for the property.</param>
        /// <param name="valueKind">The data type of the value to set.</param>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive, path, name or valuekind is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs while writing to the Windows Registry key.</exception>
        public void SetValue(RegistryHive hive, string path, string name, object value, RegistryValueKind valueKind)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);
            WinRegistryErrorMessages.ThrowIfNameInvalid(name);
            WinRegistryErrorMessages.ThrowIfValueKindInvalid(valueKind);

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                RegistryKey registryKey = baseKey.OpenSubKey(path, true);

                if (registryKey == null)
                {
                    // The specified subkey doesn't exist, so create it.
                    using RegistryKey newKey = baseKey.CreateSubKey(path);
                    newKey.SetValue(name, value, valueKind);
                }
                else
                {
                    registryKey.SetValue(name, value, valueKind);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error writing to the Windows Registry key.", ex);
            }
        }

        /// <summary>
        /// Creates a registry key at the specified location.
        /// </summary>
        /// <param name="hive">The registry hive to create the key under.</param>
        /// <param name="path">The path of the registry key to create.</param>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive or path is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs while creating the Windows Registry key.</exception>
        public void CreateKey(RegistryHive hive, string path)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                baseKey.CreateSubKey(path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating the Windows Registry key.", ex);
            }
        }

        /// <summary>
        /// Deletes a registry value under the specified registry key.
        /// </summary>
        /// <param name="hive">The registry hive to open.</param>
        /// <param name="path">The path of the registry key where the value exists.</param>
        /// <param name="name">The name of the value to delete.</param>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive, path or name is invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user does not have the necessary permissions to delete the registry value.</exception>
        /// <exception cref="SecurityException">Thrown when the user does not have the necessary permissions to delete the registry value due to security restrictions.</exception>
        public void DeleteRegistryValue(RegistryHive hive, string path, string name)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);
            WinRegistryErrorMessages.ThrowIfNameInvalid(name);
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using RegistryKey key = baseKey.OpenSubKey(path, true);
                key?.DeleteValue(name, true);
            }
            catch (SecurityException ex)
            {
                // SecurityException occurs if the user does not have necessary permissions to delete the value
                throw new SecurityException("Insufficient permissions to delete the registry value.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                // UnauthorizedAccessException occurs if the user does not have necessary permissions to delete the value due to security restrictions
                throw new UnauthorizedAccessException("Insufficient permissions to delete the registry value due to security restrictions.", ex);
            }
            catch (Exception ex)
            {
                // Catch any other exceptions and rethrow
                throw new Exception("An error occurred while deleting the registry value.", ex);
            }
        }

        /// <summary>
        /// Deletes a registry key and its subkeys.
        /// </summary>
        /// <param name="hive">The registry hive to open.</param>
        /// <param name="path">The path of the registry key to delete.</param>
        /// <exception cref="ArgumentException">Thrown when the specified registry hive or path is invalid</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user does not have the necessary permissions to delete the registry key.</exception>
        /// <exception cref="SecurityException">Thrown when the user does not have the necessary permissions to delete the registry key due to security restrictions.</exception>
        public void DeleteTree(RegistryHive hive, string path)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                baseKey?.DeleteSubKeyTree(path, true);
            }
            catch (SecurityException ex)
            {
                throw new SecurityException("Insufficient permissions to delete the registry key.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException("Insufficient permissions to delete the registry key due to security restrictions.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the registry key.", ex);
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Converts a string representation of a Registry Hive to the corresponding RegistryHive enum value.
        /// </summary>
        /// <param name="hiveString">A string representation of a Registry Hive, not case-sensitive.</param>
        /// <returns>The RegistryHive enum value corresponding to the provided string representation.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided string does not match a valid Registry Hive.</exception>
        public RegistryHive ConvertStringToRegistryHive(string hiveString)
        {
            return hiveString.ToLower() switch
            {
                "hkcr" or "hkey_classes_root" or "classesroot" => RegistryHive.ClassesRoot,
                "hkcu" or "hkey_current_user" or "currentuser" => RegistryHive.CurrentUser,
                "hklm" or "hkey_local_machine" or "localmachine" => RegistryHive.LocalMachine,
                "hku" or "hkey_users" or "users" => RegistryHive.Users,
                "hkcc" or "hkey_current_config" or "currentconfig" => RegistryHive.CurrentConfig,
                _ => throw new ArgumentException("Invalid registry hive string.", nameof(hiveString)),
            };
        }

        /// <summary>
        /// Converts a string representation of a RegistryValueKind to the corresponding RegistryValueKind enum value.
        /// </summary>
        /// <param name="valueType">A string representation of a RegistryValueKind, not case-sensitive.</param>
        /// <returns>The RegistryValueKind enum value corresponding to the provided string representation.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided string does not match a valid RegistryValueKind.</exception>
        public RegistryValueKind ConvertStringToRegistryValueKind(string valueType)
        {
            return valueType.ToLower() switch
            {
                "string" or "reg_sz" => RegistryValueKind.String,
                "dword" or "reg_dword" => RegistryValueKind.DWord,
                "binary" or "reg_binary" => RegistryValueKind.Binary,
                "qword" or "reg_qword" => RegistryValueKind.QWord,
                "multistring" or "reg_multi_sz" => RegistryValueKind.MultiString,
                "expandstring" or "reg_expand_sz" => RegistryValueKind.ExpandString,
                _ => throw new ArgumentException("Invalid RegistryValueKind string representation.", nameof(valueType)),
            };
        }


        /// <summary>
        /// Converts a .NET data type to the corresponding RegistryValueKind.
        /// </summary>
        /// <param name="valueType">The .NET data type to convert.</param>
        /// <returns>The corresponding RegistryValueKind.</returns>
        public RegistryValueKind ConvertTypeToRegistryValueKind(Type valueType)
        {
            return Type.GetTypeCode(valueType) switch
            {
                TypeCode.String => RegistryValueKind.String,
                TypeCode.Int32 => RegistryValueKind.DWord,
                TypeCode.Int64 => RegistryValueKind.QWord,
                TypeCode.Boolean => RegistryValueKind.DWord,
                TypeCode.Byte => RegistryValueKind.Binary,
                /*
                TypeCode.Single => RegistryValueKind.String,
                TypeCode.Double => RegistryValueKind.String;
                TypeCode.DateTime => RegistryValueKind.String;
                TypeCode.Char => RegistryValueKind.String;
                TypeCode.Decimal => RegistryValueKind.String;
                */
                _ => RegistryValueKind.String,// Default to String for unsupported types
            };
        }

        /// <summary>
        /// Converts a RegistryValueKind enumeration value to its corresponding .NET Type.
        /// </summary>
        /// <param name="valueKind">The RegistryValueKind value to be converted.</param>
        /// <returns>The .NET Type that corresponds to the given RegistryValueKind.</returns>
        public Type ConvertRegistryValueKindToType(RegistryValueKind valueKind)
        {
            return valueKind switch
            {
                RegistryValueKind.String or RegistryValueKind.ExpandString => typeof(string),
                RegistryValueKind.DWord => typeof(int),
                RegistryValueKind.QWord => typeof(long),
                RegistryValueKind.Binary => typeof(byte[]),
                RegistryValueKind.MultiString => typeof(string[]),
                _ => typeof(object),
            };
        }
        #endregion

        #region private methods
        /// <summary>
        /// Retrieves the data value associated with the specified registry key and value name.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The registry key path.</param>
        /// <param name="name">The name of the value.</param>
        /// <returns>The value data as a string, or null if the value is not found.</returns>
        /// <exception cref = "ArgumentException" > Thrown when the specified registry hive, path or name is invalid</exception>
        private static string GetValue(RegistryHive hive, string path, string name)
        {
            WinRegistryErrorMessages.ThrowIfHiveInvalid(hive);
            WinRegistryErrorMessages.ThrowIfPathInvalid(path);
            WinRegistryErrorMessages.ThrowIfNameInvalid(name);

            using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(path);
            if (key == null)
                return null;

            return key.GetValue(name)?.ToString();
        }
        #endregion
    }
}