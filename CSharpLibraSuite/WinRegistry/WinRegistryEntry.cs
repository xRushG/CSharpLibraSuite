using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace CSharpLibraSuite.WinRegistry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Represents an entry in the Windows Registry.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the functionality needed to interact with Windows Registry entries, 
    /// including reading and writing values. It provides a comprehensive set of methods to handle 
    /// different value types, ensuring flexibility and ease of use.
    /// 
    /// Key features include:
    /// - Reading values from a specified registry path and name.
    /// - Writing values to a specified registry path and name.
    /// - Support for multiple data types such as strings, integers, and binary data.
    /// - Ability to specify the registry hive (e.g., HKEY_LOCAL_MACHINE, HKEY_CURRENT_USER).
    /// 
    /// This class is designed to simplify the manipulation of registry entries by providing a 
    /// straightforward interface for common registry operations.
    /// 
    /// Example usage:
    /// <code>
    /// var registryEntry = new RegistryEntry<string>(RegistryHive.LocalMachine, @"Software\MyApp", "Settings");
    /// var value = registryEntry.Read();
    /// if (value != **)
    ///     registryEntry.Write("newVal");
    /// </code>
    /// 
    /// <code>
    /// var registryEntry = new RegistryEntry<int>(RegistryHive.LocalMachine, @"Software\MyApp", "Settings").Read();
    /// </code>
    /// 
    /// <code>
    /// var registryEntry = new RegistryEntry<long>(RegistryHive.LocalMachine, @"Software\MyApp", "Settings").SetValidation(min, max).Read();
    /// if (registryEntry.IsValid())
    ///     Do Something
    /// </code>
    /// 
    /// License:
    /// This class is licensed under MIT License.
    /// </remarks>
    public class WinRegistryEntry<T>
    {
        #region Public Registry Properties

        /// <summary>
        /// Represents the registry hive associated with the registry key.
        /// </summary>
        /// /// <remarks>
        /// The default value is <see cref="RegistryHive.CurrentUser"/>.
        /// </remarks>
        public RegistryHive Hive
        {
            get { return privateHive; }
            set
            {
                privateHive =
                    !Enum.IsDefined(typeof(RegistryHive), value) || value == RegistryHive.CurrentConfig || value == RegistryHive.ClassesRoot
                    ? throw new ArgumentException("Invalid parameter: Unknown or unsupported RegistryHive value.", nameof(Hive))
                    : value;
            }
        }
        private RegistryHive privateHive = RegistryHive.CurrentUser;

        /// <summary>
        /// Represents the path of the registry entry.
        /// </summary>
        public string Path
        {
            get { return privatePath; }
            set
            {
                privatePath =
                    !string.IsNullOrWhiteSpace(value)
                    ? value
                    : throw new ArgumentNullException(nameof(Path), "Invalid parameter: Path cannot be null, empty, or consist only of whitespace characters.");
            }
        }
        private string privatePath;

        /// <summary>
        /// Represents the name of the registry entry.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the kind of data stored in the registry value.
        /// </summary>
        public RegistryValueKind ValueKind { get; private set; } = InitialRegistryValueKind();

        /// <summary>
        /// Represents the value of the registry entry.
        /// </summary>
        public T Value
        {
            get { return privateValue; }
            set
            {
                ValueValidationRules(value);
                privateValue = value;
            }
        }
        private T privateValue;

        #endregion

        #region Public Aditional Properties

        /// <summary>
        /// Represents the raw value retrieved directly from the registry.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the entry has been explicitly set in the registry.
        /// This check is faster as it only verifies if a value was readed (set in registry).
        /// </summary>
        public bool IsSet => RawValue != null;

        /// <summary>
        /// Gets a value indicating whether the entry's value is valid according to custom validation rules.
        /// This check includes whether the value has been set and whether it adheres to the defined validation criteria.
        /// </summary>
        public bool IsValid => CheckIsValid();

        #endregion

        #region Private Validation Properties

        private T[] AllowedValues;
        private bool CaseSensitive;
        private int? MinInt32Value;
        private int? MaxInt32Value;
        private long? MinInt64Value;
        private long? MaxInt64Value;
        private Type EnumType;

        #endregion

        #region Public Constructors

        /******************************************
            * Constructors
            *      Empty()
            *      Read default => (RegistryHive hive, string path)
            *      Write default => (RegistryHive hive, string path, T value)
            *      Read specific => (RegistryHive hive, string path, string name)
            *      Write specific => (RegistryHive hive, string path, string name, T value)
        *******************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRegistryEntry{T}"/> class with default values.
        /// </summary>
        public WinRegistryEntry() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRegistryEntry{T}"/> class for reading a default value from the specified registry hive and path.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        public WinRegistryEntry(RegistryHive hive, string path)
        {
            Hive = hive;
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRegistryEntry{T}"/> class for writing a default value to the specified registry hive and path.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        public WinRegistryEntry(RegistryHive hive, string path, T value)
        {
            Hive = hive;
            Path = path;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRegistryEntry{T}"/> class for reading a specific value from the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        public WinRegistryEntry(RegistryHive hive, string path, string name)
        {
            Hive = hive;
            Path = path;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRegistryEntry{T}"/> class for writing a specific value to the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        public WinRegistryEntry(RegistryHive hive, string path, string name, T value)
        {
            Hive = hive;
            Path = path;
            Name = name;
            Value = value;
        }

        #endregion

        #region Public Factory Methods

        /******************************************
            * Factory Methods
            *      Read default => New(RegistryHive hive, string path)
            *      Write default => New(RegistryHive hive, string path, T value)
            *      Read specific => New(RegistryHive hive, string path, string name)
            *      Write specific => New(RegistryHive hive, string path, string name, T value)
        *******************************************/

        /// <summary>
        /// Creates a new instance of the <see cref="WinRegistryEntry{T}"/> class for reading a default value from the specified registry hive and path.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <returns>A new instance of the <see cref="WinRegistryEntry{T}"/> class.</returns>
        public static WinRegistryEntry<T> New(RegistryHive hive, string path)
        {
            return new WinRegistryEntry<T>(hive, path);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WinRegistryEntry{T}"/> class for writing a value to the specified registry hive and path.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        /// <returns>A new instance of the <see cref="WinRegistryEntry{T}"/> class.</returns>
        public static WinRegistryEntry<T> New(RegistryHive hive, string path, T value)
        {
            return new WinRegistryEntry<T>(hive, path, value);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WinRegistryEntry{T}"/> class for reading a specific value from the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <returns>A new instance of the <see cref="WinRegistryEntry{T}"/> class.</returns>
        public static WinRegistryEntry<T> New(RegistryHive hive, string path, string name)
        {
            return new WinRegistryEntry<T>(hive, path, name);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WinRegistryEntry{T}"/> class for writing a specific value to the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        /// <returns>A new instance of the <see cref="WinRegistryEntry{T}"/> class.</returns>
        public static WinRegistryEntry<T> New(RegistryHive hive, string path, string name, T value)
        {
            return new WinRegistryEntry<T>(hive, path, name, value);
        }

        #endregion

        #region Public Fluent Interfaces

        /******************************************
            * Fluent Interfaces
            *      SetValueKind()
            *      SetDefaultValue()
            *      Read()
            *      Write()
        *******************************************/

        /// <summary>
        /// Sets the kind of the registry value, ensuring it is a valid and defined <see cref="RegistryValueKind"/>.
        /// </summary>
        /// <param name="valueKind">The registry value kind to set.</param>
        /// <returns>A new instance of the Entry class.</returns>
        public WinRegistryEntry<T> SetValueKind(RegistryValueKind valueKind)
        {
            ValueKind = ValueKindValidationRule(valueKind);
            return this;
        }

        /// <summary>
        /// Reads the value of the registry entry from the specified registry path and assigns it to the Value property.
        /// </summary>
        public WinRegistryEntry<T> Read()
        {
            ReadRegistry();
            return this;
        }

        /// <summary>
        /// Writes the value of the registry entry to the specified registry path.
        /// <returns>The current instance of <see cref="WinRegistryEntry{T}"/> to allow for method chaining.</returns>
        /// </summary>

        public WinRegistryEntry<T> Write()
        {
            WriteRegistry();
            return this;
        }

        /// <summary>
        /// Writes a new value to the registry entry.
        /// </summary>
        /// <param name="newValue">The new value to be written to the registry entry.</param>
        /// <returns>The current instance of <see cref="WinRegistryEntry{T}"/> to allow for method chaining.</returns>
        /// <remarks>
        /// This method updates the value of the registry entry and writes the new value to the registry.
        /// </remarks>
        public WinRegistryEntry<T> Write(T newValue)
        {
            Value = newValue;
            return Write();
        }


        #endregion

        #region Public Set Validation Mehtods

        /******************************************
         * Methods be unified but for less redability its not done! 
            * Public Validation Mehtods
            *      SetValidation(string[] allowedValues, bool caseSensitive = false)
            *      SetValidation(int[] allowedValues)
            *      SetValidation(long[] allowedValues)
            *      
            *      SetValidation(int minValue, int maxValue)
            *      SetValidation(long minValue, long maxValue)
            *      
            *      SetValidation<TEnum>(bool caseSensitive = false) where TEnum : Enum
        *******************************************/

        #region SetValidation => arrays

        /// <summary>
        /// Sets the allowed values for validation, with an option for case sensitivity.
        /// </summary>
        /// <param name="allowedValues">The array of allowed values.</param>
        /// <param name="caseSensitive">Flag indicating whether the validation should be case sensitive (default: false).</param>
        public WinRegistryEntry<T> SetValidation(T[] allowedValues, bool caseSensitive = false)
        {
            ResetValidationRules();

            if (allowedValues != null && allowedValues.Length > 0)
            {
                AllowedValues = allowedValues;
                CaseSensitive = caseSensitive;
            }

            return this;
        }

        /// <summary>
        /// Sets up validation using an array of allowed integer values.
        /// </summary>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        public WinRegistryEntry<T> SetValidation(int[] allowedValues)
        {
            T[] mappedValues = allowedValues?.Select(value => (T)(object)value).ToArray();
            return SetValidation(mappedValues, false);
        }

        /// <summary>
        /// Sets up validation using an array of allowed integer values.
        /// </summary>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        public WinRegistryEntry<T> SetValidation(long[] allowedValues)
        {
            T[] mappedValues = allowedValues?.Select(value => (T)(object)value).ToArray();
            return SetValidation(mappedValues, false);
        }

        #endregion

        #region SetValidation => Min Max for Int and Long

        /// <summary>
        /// Sets up validation for a range of integer values.
        /// </summary>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        public WinRegistryEntry<T> SetValidation(int minValue, int maxValue)
        {
            ValidateRange(minValue, maxValue);
            ResetValidationRules();

            MinInt32Value = minValue;
            MaxInt32Value = maxValue;

            return this;
        }

        /// <summary>
        /// Sets up validation for a range of integer values.
        /// </summary>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        public WinRegistryEntry<T> SetValidation(long minValue, long maxValue)
        {
            ValidateRange(minValue, maxValue);
            ResetValidationRules();

            MinInt64Value = minValue;
            MaxInt64Value = maxValue;

            return this;
        }

        /// <summary>
        /// Sets up validation rules for a range of Int32, Int64 values.
        /// </summary>
        /// <param name="minValue">The minimum value of the range. "*" can be provided to indicate no minimum value.</param>
        /// <param name="maxValue">The maximum value of the range. "*" can be provided to indicate no maximum value.</param>
        /// <returns>The current instance of the WinRegistryEntry<T> class.</returns>
        /// <exception cref="ArgumentException">Thrown when the registry entry type is not a valid Int32 or Int64.</exception>
        /// <exception cref="ArgumentException">Thrown when an invalid minimum value is provided for Int32 or Int64.</exception>
        /// <exception cref="ArgumentException">Thrown when an invalid maximum value is provided for Int32 or Int64.</exception>

        public WinRegistryEntry<T> SetValidation(string minValue, string maxValue)
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(T));

            if (string.IsNullOrEmpty(minValue) || minValue == "*")
                minValue = "0";
            if ((string.IsNullOrEmpty(maxValue) || maxValue == "*"))
                maxValue = (typeCode == TypeCode.Int32) 
                    ? Int32.MaxValue.ToString() 
                    : Int64.MaxValue.ToString();

            if (typeCode == TypeCode.Int32)
            {
                if (!int.TryParse(minValue, out int minIntValue))
                    throw new ArgumentException("Invalid minimum value for Int32.");
                if (!int.TryParse(maxValue, out int maxIntValue))
                    throw new ArgumentException("Invalid maximum value for Int32.");

                return SetValidation(minIntValue, maxIntValue);
            }
            else if (typeCode == TypeCode.Int64)
            {
                if (!long.TryParse(minValue, out long minLongValue))
                    throw new ArgumentException("Invalid minimum value for Int64.");
                if (!long.TryParse(maxValue, out long maxLongValue))
                    throw new ArgumentException("Invalid maximum value for Int64.");

                return SetValidation(minLongValue, maxLongValue);
            }
            else
            {
                throw new ArgumentException("Registry entry type must be either a valid Int32 or Int64 to use this validation.");
            }
        }


        /// <summary>
        /// Private method to validate the range values.
        /// </summary>
        /// <typeparam name="U">The type of the values being validated.</typeparam>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        /// <param name="type">The type of registry entry (used for error messages).</param>
        private static void ValidateRange<U>(U minValue, U maxValue) where U : IComparable<U>
        {
            TypeCode typeCode = Type.GetTypeCode(typeof(U));

            string type = 
                typeCode == TypeCode.Int32 ? "dword" : 
                typeCode == TypeCode.Int64 ? "qword" 
                : throw new ArgumentException("Registry entry type must be either Int32 or Int64 to use this validation.");

            if (minValue.CompareTo(default(U)) < 0)
                throw new ArgumentException($"Negative value not allowed for {type} parameter.", nameof(minValue));
            if (maxValue.CompareTo(default(U)) < 0)
                throw new ArgumentException($"Negative value not allowed for {type} parameter.", nameof(maxValue));
            if (minValue.CompareTo(maxValue) > 0)
                throw new ArgumentException("MinValue must be less than or equal to MaxValue.");
        }

        #endregion

        #region SetValidatio n => Enum

        /// <summary>
        /// Sets the validation to use an enumeration type
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="caseSensitive">Flag indicating whether the validation should be case sensitive for strings (default: false).</param>
        public WinRegistryEntry<T> SetValidation<TEnum>(bool caseSensitive = false) where TEnum : Enum
        {
            ResetValidationRules();

            CaseSensitive = caseSensitive;
            Type enumType = typeof(TEnum);
            if (enumType != null)
                EnumType = enumType;

            return this;
        }

        #endregion

        #endregion

        #region Public Methods

        /******************************************
            * Public Methods
            *      IsReadable()
            *      IsWritable()
        *******************************************/

        /// <summary>
        /// Checks if the Windows Registry key is ready for reading by ensuring that the hive,
        /// path, and name properties are set.
        /// </summary>
        /// <returns>True if the key is ready for reading, otherwise false.</returns>
        public bool IsReadable()
        {
            return IsHiveSet() && IsPathSet();
        }

        /// <summary>
        /// Checks if the Windows Registry key is ready for a write operation.
        /// The key is considered write-ready if none of the following conditions are met:
        /// - The hive is set
        /// - The registry value type is set
        /// - The key path is set
        /// </summary>
        /// <returns>Returns true if the key is write-ready, otherwise false.</returns>
        public bool IsWritable()
        {
            return IsHiveSet() && IsValueKindSet() && IsPathSet();
        }

        #endregion

        #region Private Methods

        /******************************************
            * Private Methods
            *      ReadRegistry()
            *      WriteRegistry()
            *      ConvertValueBasedOnType()
        *******************************************/

        /// <summary>
        /// Reads the value of the registry entry from the specified registry path and returns it as a string.
        /// </summary>
        /// <returns>The value of the registry entry as a string.</returns>
        private void ReadRegistry()
        {
            if (!IsReadable())
                throw new InvalidOperationException("Unable to read registry key. Hive, path, and name are required.");

            string rawValue = null;
            string name = string.IsNullOrEmpty(Name) ? null : Name;

            try
            {
                using var key = RegistryKey.OpenBaseKey(Hive, RegistryView.Default).OpenSubKey(Path);
                if (key != null)
                    RawValue = rawValue = key.GetValue(name)?.ToString();

                if (rawValue != null)
                    ValueKind = key.GetValueKind(name);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading the Windows Registry.", ex);
            }

            if (string.IsNullOrEmpty(rawValue))
                return;


            Value = ConvertValueBasedOnType(rawValue);
        }

        /// <summary>
        /// Writes the registry value to the specified registry path, handling exceptions if they occur.
        /// </summary>
        private void WriteRegistry()
        {
            if (!IsWritable())
                throw new InvalidOperationException("Unable to write registry key. Hive, path, name, value kind, and value are required.");

            string name = string.IsNullOrEmpty(Name) ? null : Name;
            RegistryValueKind valueKind = string.IsNullOrEmpty(Name) ? RegistryValueKind.String : ValueKind;

            string value;
            if (typeof(T) == typeof(bool))
            {
                value = (bool)(object)Value
                    ? ValueKind == RegistryValueKind.DWord ? "1" : "True"
                    : ValueKind == RegistryValueKind.DWord ? "0" : "False";
            }
            else
            {
                value = Value.ToString();
            }


            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Default);
                using RegistryKey registryKey = baseKey.CreateSubKey(Path, true);

                registryKey.SetValue(name, value, valueKind);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error writing to the Windows Registry.", ex);
            }
        }

        /// <summary>
        /// Converts a string value to the specified .NET data type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target .NET data type to which the value is converted.</typeparam>
        /// <param name="originalValue">The string value to be converted.</param>
        /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the specified type <typeparamref name="T"/>.</exception>
        /// <exception cref="FormatException">Thrown when the format of the value is invalid for the specified type <typeparamref name="T"/>.</exception>
        private T ConvertValueBasedOnType(string originalValue)
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.String:
                        return (T)(object)originalValue;
                    case TypeCode.Int32:
                        if (int.TryParse(originalValue, out int intValue))
                            return (T)(object)intValue;
                        break;
                    case TypeCode.Int64:
                        if (long.TryParse(originalValue, out long longValue))
                            return (T)(object)longValue;
                        break;
                    case TypeCode.Boolean:
                        if (bool.TryParse(originalValue, out bool boolValue))
                            return (T)(object)boolValue;
                        else if (int.TryParse(originalValue, out int intBool))
                            return (T)(object)(intBool == 1);
                        break;
                    default:
                        throw new InvalidOperationException($"Conversion not supported for type '{typeof(T)}'.");
                }
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Unable to convert value '{originalValue}' to type '{typeof(T)}'.");
            }
            catch (FormatException)
            {
                throw new FormatException($"Invalid format for value '{originalValue}' when converting to type '{typeof(T)}'.");
            }

            throw new InvalidOperationException($"Conversion for '{typeof(T)}' failed.");
        }

        #endregion

        #region Private Protperty Methods

        /******************************************
            * Private Protperty Methods
            *      ValueValidationRules()
            *      ValueKindValidationRule()
            *      InitialRegistryValueKind()
            *      InitialDefaultValue()
        *******************************************/

        /// <summary>
        /// Validates the provided value based on its type-specific rules.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the value is invalid based on its type-specific rules:
        /// - If the value is of type <see cref="int"/> or <see cref="long"/> and is negative.
        /// - If the value type is not supported.
        /// </exception>
        private void ValueValidationRules(T value)
        {
            // Boolean values are either string or DWORD. Mapping is needed to update ValueKind.
            var booleanRegistryValueKindMap = new Dictionary<string, RegistryValueKind>
            {
                { "true", RegistryValueKind.String },
                { "false", RegistryValueKind.String },
                { "0", RegistryValueKind.DWord },
                { "1", RegistryValueKind.DWord }
            };

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    // No validation needed for strings.
                    break;

                case TypeCode.Boolean:
                    if (!booleanRegistryValueKindMap.TryGetValue(value.ToString().ToLower(), out var valueKind))
                        throw new ArgumentException("Invalid value. Supported values are ci strings 'True'/'False' or numbers '0'/'1'.", nameof(value));

                    ValueKind = valueKind;
                    break;

                case TypeCode.Int32:
                case TypeCode.Int64:
                    if (Convert.ToInt64(value) < 0)
                        throw new ArgumentException("Value cannot be negative.", nameof(value));
                    break;

                // case TypeCode.Byte:
                //     if (value == null || ((byte[])(object)value).Length == 0)
                //     {
                //         throw new ArgumentException("Value cannot be null or empty.", nameof(value));
                //     }
                //     break;

                default:
                    throw new ArgumentException($"Value type '{typeof(T).FullName}' is not supported.", nameof(value));
            }
        }

        /// <summary>
        /// Validates the specified registry value kind.
        /// </summary>
        /// <param name="valueKind">The registry value kind to validate.</param>
        /// <returns>The validated <see cref="RegistryValueKind"/>.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided <paramref name="valueKind"/> is not a valid, defined <see cref="RegistryValueKind"/> value,
        /// or if it is <see cref="RegistryValueKind.Unknown"/>, <see cref="RegistryValueKind.None"/>, or 0.
        /// </exception>
        private static RegistryValueKind ValueKindValidationRule(RegistryValueKind valueKind)
        {
            if (!Enum.IsDefined(typeof(RegistryValueKind), valueKind) || valueKind == RegistryValueKind.Unknown || valueKind == RegistryValueKind.None || valueKind == 0)
                throw new ArgumentException("Invalid parameter: Unknown or unsupported RegistryValueKind value.", nameof(valueKind));

            bool isValid = Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.String => valueKind == RegistryValueKind.String,
                TypeCode.Boolean => valueKind == RegistryValueKind.String || valueKind == RegistryValueKind.DWord,
                TypeCode.Int32 => valueKind == RegistryValueKind.DWord,
                TypeCode.Int64 => valueKind == RegistryValueKind.QWord,
                //TypeCode.Byte => valueKind == RegistryValueKind.Binary,
                _ => throw new ArgumentException($"Value type '{typeof(T).FullName}' is not supported.")
            };

            if (!isValid)
                throw new ArgumentException($"Invalid parameter: {typeof(T).Name} should be set as {valueKind}.", nameof(valueKind));

            return valueKind;
        }

        /// <summary>
        /// Determines the initial RegistryValueKind based on the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The initial RegistryValueKind determined by the type <typeparamref name="T"/>.</returns>
        private static RegistryValueKind InitialRegistryValueKind()
        {
            return Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.Int32 => RegistryValueKind.DWord,
                TypeCode.Int64 => RegistryValueKind.QWord,
                TypeCode.Boolean => RegistryValueKind.String,
                //TypeCode.Byte => RegistryValueKind.Binary,
                _ => RegistryValueKind.String,// Default to String for unsupported types
            };
        }

        /// <summary>
        /// Determines the initial default value based on the type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The initial default value determined by the type <typeparamref name="T"/>.</returns>
        private static T InitialDefaultValue()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    return (T)(object)Convert.ToInt32(0);
                case TypeCode.Int64:
                    return (T)(object)Convert.ToInt64(0);
                case TypeCode.Boolean:
                    return (T)(object)Convert.ToBoolean(false);
                default:
                    return default; // Default to null for unsupported types
            }
        }

        #endregion

        #region Private Assert Methods

        /******************************************
            * Private Assert Methods
            *      IsHiveSet()
            *      IsValueKindSet()
            *      IsPathSet()
        *******************************************/

        /// <summary>
        /// Determines whether the registry hive has been explicitly set.
        /// </summary>
        /// <returns><c>true</c> if the hive is set; otherwise, <c>false</c>.</returns>
        private bool IsHiveSet() => Hive != 0;

        /// <summary>
        /// Determines whether the value kind of the registry entry has been explicitly set.
        /// </summary>
        /// <returns><c>true</c> if the value kind is set; otherwise, <c>false</c>.</returns>
        private bool IsValueKindSet() => ValueKind != 0;

        /// <summary>
        /// Determines whether the path of the registry entry has been explicitly set.
        /// </summary>
        /// <returns><c>true</c> if the path is set; otherwise, <c>false</c>.</returns>
        private bool IsPathSet() => Path != null;

        #endregion

        #region Private Value Validation

        /******************************************
            * Private Assert Methods
            *      ResetValidationRules();
            *      CheckIsValid()
            *      ValidateString()
            *      ValidateInt32()
            *      ValidateInt64()
        *******************************************/

        /// <summary>
        /// Resets all validation rules for the entry to their default values.
        /// This includes clearing allowed values, resetting case sensitivity, setting numeric ranges and enum types to null.
        /// </summary>
        private void ResetValidationRules()
        {
            AllowedValues = null;
            CaseSensitive = false;

            MinInt32Value = null;
            MaxInt32Value = null;
            MinInt64Value = null;
            MaxInt64Value = null;

            EnumType = null;
        }

        /// <summary>
        /// Determines the validity of the registry entry's value after validation.
        /// </summary>
        /// <summary>
        /// Checks if the current value is valid according to its type-specific rules and constraints.
        /// </summary>
        /// <returns>True if the value is valid; otherwise, false.</returns>
        private bool CheckIsValid()
        {
            if (RawValue == null)
                return false;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    return ValidateString();

                case TypeCode.Boolean:
                    return true;

                case TypeCode.Int32:
                    return ValidateInt32();

                case TypeCode.Int64:
                    return ValidateInt64();

                default:
                    throw new ArgumentException($"Value type '{typeof(T).FullName}' is not supported.");
            }
        }

        /// <summary>
        /// Validates a string value based on allowed values or enumeration values.
        /// </summary>
        /// <returns>True if the string value is valid; otherwise, false.</returns>
        private bool ValidateString()
        {
            string value = Value.ToString();

            if (AllowedValues != null)
            {
                return CaseSensitive
                    ? AllowedValues.Contains(Value)
                    : AllowedValues.Any(v => v.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
            }
            else if (EnumType != null && EnumType.IsEnum)
            {
                return Enum.GetValues(EnumType)
                           .Cast<Enum>()
                           .Any(e => e.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        /// <summary>
        /// Validates an integer value based on allowed values, minimum and maximum values, or enumeration values.
        /// </summary>
        /// <returns>True if the integer value is valid; otherwise, false.</returns>
        private bool ValidateInt32()
        {
            int value = (int)(object)Value;

            if (AllowedValues != null)
                return AllowedValues.Contains(Value);

            else if (MinInt32Value != null && MaxInt32Value != null)
                return value >= MinInt32Value && value <= MaxInt32Value;

            else if (EnumType != null && EnumType.IsEnum)
            {
                foreach (object enumValue in Enum.GetValues(EnumType))
                {
                    if (Convert.ToInt32(enumValue) == value)
                    {
                        return true;
                    }
                }

                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a long integer value based on allowed values, minimum and maximum values, or enumeration values.
        /// </summary>
        /// <returns>True if the long integer value is valid; otherwise, false.</returns>
        private bool ValidateInt64()
        {
            long value = (long)(object)Value;

            if (AllowedValues != null)
                return AllowedValues.Contains(Value);

            else if (MinInt64Value != null && MaxInt64Value != null)
                return value >= MinInt64Value && value <= MaxInt64Value;

            return true;
        }

        #endregion
    }
}
