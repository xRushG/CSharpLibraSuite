using core.WinRegistry.RegEntry;
using System;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace core.WinRegistry
{
    [SupportedOSPlatform("windows")]
    public class WinRegistryPlus : WinRegistry
    {
        #region dword methods
        /// <summary>
        /// Retrieves a REG_DWORD (32-bit integer) value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyInteger instance representing the retrieved REG_DWORD value.</returns>
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int? defaultValue = null)
        {
            var key = GetRegistryEntry(hive, path, name);
            IntegerEntry IntKey = new(key, defaultValue);
            return IntKey;
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
        public IntegerEntry GetInteger<TEnum>(RegistryHive hive, string path, string name, int? defaultValue = null) where TEnum : Enum
        {
            var key = GetRegistryEntry(hive, path, name);
            IntegerEntry IntKey = new(key, defaultValue);
            IntKey.SetValidation<TEnum>();

            return IntKey;
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
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int[] allowedValues, int? defaultValue = null)
        {
            var key = GetRegistryEntry(hive, path, name);
            IntegerEntry IntKey = new(key, defaultValue);
            IntKey.SetValidation(allowedValues);

            return IntKey;
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
        public IntegerEntry GetInteger(RegistryHive hive, string path, string name, int minimum, int maximum, int? defaultValue = null)
        {
            var key = GetRegistryEntry(hive, path, name);
            IntegerEntry IntKey = new(key, defaultValue);
            IntKey.SetValidation(minimum, maximum);

            return IntKey;
        }

        #endregion

        #region string methods
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
            var key = GetRegistryEntry(hive, path, name);
            StringEntry StrKey = new(key, defaultValue);

            return StrKey;
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
            var key = GetRegistryEntry(hive, path, name);
            StringEntry StrKey = new(key, defaultValue);
            StrKey.SetValidation(allowedValues);

            return StrKey;
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
            var key = GetRegistryEntry(hive, path, name);
            StringEntry StrKey = new(key, defaultValue);
            StrKey.SetValidation<TEnum>();

            return StrKey;
        }
        #endregion

        #region bool methods
        /// <summary>
        /// Retrieves a boolean (REG_SZ or REG_DWORD) value from the windows registry based on the specified registry information.
        /// </summary>
        /// <param name="hive">The registry hive.</param>
        /// <param name="path">The path to the registry key.</param>
        /// <param name="name">The name of the registry property.</param>
        /// <param name="defaultValue">The default value if the registry value is not found (default: null).</param>
        /// <returns>A WindowsRegistryKeyString instance representing the retrieved REG_SZ or REG_DWORD as bool value.</returns>
        public BoolEntry GetBoolean(RegistryHive hive, string path, string name, bool? defaultValue = null)
        {
            var key = GetRegistryEntry(hive, path, name);
            BoolEntry boolKey = new (key, defaultValue);

            return boolKey;
        }

        #endregion
    }
}