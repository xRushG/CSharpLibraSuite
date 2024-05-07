using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Represents a string entry in the Windows Registry, providing methods to read and write string values.
    /// </summary>
    /// <remarks>
    /// This class extends the functionality of the base Entry class to specifically handle string values in the Windows Registry.
    /// It provides methods to read and write string values to the registry under a specified path and name.
    /// Users can create instances of this class to work with string registry entries, providing hive, path, and name parameters.
    /// </remarks>
    public class StringEntry : Entry
    {
        #region Private Attributes

        private string[] AllowedValues;
        private bool CaseSensitive;
        private Type EnumType;

        #endregion

        #region Public Property: DefaultValue, IsDefault

        /// <summary>
        /// Represents a default string value.
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this entry is using its default value.
        /// </summary>
        public bool IsDefault { get; private set; }

        #endregion

        #region Public Override Property: Value, IsValid

        /// <summary>
        /// Represents the string value associated with the windows registry key.
        /// </summary>
        public new string Value
        {
            get => StringValue;
            private set
            {
                StringValue = value;
            }
        }
        private string StringValue;

        /// <summary>
        /// Indicates whether the value is valid based on defined validation criteria.
        /// </summary>
        /// <remarks>
        /// Returns true if the value is valid, otherwise false. 
        /// Validity is determined by checking against allowed values or an enumeration type if specified.
        /// </remarks>
        public override bool IsValid
        {
            get
            {
                if (!ProtectedHasValue())
                    return false;

                if (IsDefault)
                    return false;

                if (AllowedValues != null)
                    return (bool)AssertAllowedValues();

                if (EnumType != null && EnumType.IsEnum)
                    return (bool)AssertEnum();

                return true;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Entry class.
        /// </summary>
        public StringEntry() : base() { }

        /// <summary>
        /// Constructor for the Entry class with parameters.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="defaultValue">The default integer value (default: null).</param>
        public StringEntry(RegistryHive hive, string path, string name, string defaultValue = null)
            : base(hive, path, name)
        {
            SetDefaultValue(defaultValue);
        }

        /// <summary>
        /// Constructor for the Entry class with additional parameters for value and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        /// <param name="defaultValue">The default integer value (default: null).</param>
        public StringEntry(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind, string defaultValue = null)
            : base(hive, path, name, value, valueKind)
        {
            SetDefaultValue(defaultValue);
        }

        /// <summary>
        /// Initializes a new instance with the specified base key and default value.
        /// </summary>
        /// <param name="RegistryEntry">The base Windows Registry key to derive properties from.</param>
        /// <param name="defaultValue">The default string value (default: null).</param>
        /// <exception cref="ArgumentNullException">Thrown when baseKey is null.</exception>
        public StringEntry(Entry RegistryEntry, string defaultValue = null)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), InvalidBaseKeyParamMessage);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = RegistryEntry.ValueKind;
            base.Value = RegistryEntry.Value;

            SetDefaultValue(defaultValue);
            ConvertToString(RegistryEntry.Value);
        }

        #endregion

        #region Public Factory Method: New

        /// <summary>
        /// Creates a new instance of the StringEntry class with the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <returns>A new instance of the StringEntry class.</returns>
        public static StringEntry New(RegistryHive hive, string path, string name, string defaultValue = null)
        {
            return new StringEntry(hive, path, name, defaultValue);
        }

        /// <summary>
        /// Creates a new instance of the StringEntry class with the specified registry hive, path, name, value, and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The integer value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        /// <returns>A new instance of the StringEntry class.</returns>
        public static StringEntry New(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind, string defaultValue = null)
        {
            return new StringEntry(hive, path, name, value.ToString(), valueKind, defaultValue);
        }

        #endregion

        #region Public Override Fluent Interface: Read

        public override StringEntry Read()
        {
            if (!IsKeyReadable())
                throw new InvalidOperationException(UnableToReadMessage);

            string stringValue = base.Value = ProtectedRead();
            ConvertToString(stringValue);
            return this;
        }

        #endregion

        #region Public Methods: SetValidation, SetDefaultValue
        /// <summary>
        /// Sets the allowed values for validation, with an option for case sensitivity.
        /// </summary>
        /// <param name="allowedValues">The array of allowed values.</param>
        /// <param name="caseSensitive">Flag indicating whether the validation should be case sensitive (default: false).</param>
        public void SetValidation(string[] allowedValues, bool caseSensitive = false)
        {
            if (allowedValues != null && allowedValues.Length > 0)
            {
                AllowedValues = allowedValues;
                CaseSensitive = caseSensitive;
            }
        }

        /// <summary>
        /// Sets the validation to use an enumeration type, with an option for case sensitivity.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <param name="caseSensitive">Flag indicating whether the validation should be case sensitive (default: false).</param>
        public void SetValidation<TEnum>(bool caseSensitive = false) where TEnum : Enum
        {
            Type enumType = typeof(TEnum);
            if (enumType != null)
            {
                EnumType = enumType;
                CaseSensitive = caseSensitive;
            }
        }

        /// <summary>
        /// Sets the default value for the registry entry. The value must be a non-negative integer.
        /// </summary>
        /// <param name="defaultValue">The default value to be set.</param>
        public void SetDefaultValue(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        #endregion

        #region Override Method: ProtectedHasValue
        protected override bool ProtectedHasValue()
        {
            return ValueKind == RegistryValueKind.String
                && base.Value != null;
        }

        #endregion

        #region Private Methods: ConvertToString, AssertAllowedValues, AssertEnum

        /// <summary>
        /// Converts the provided string value and updates the StringValue property.
        /// </summary>
        /// <param name="newValue">The string value to convert.</param>
        /// <remarks>
        /// If the value is set and not null, it updates the StringValue property with the provided value.
        /// If the value is not set or null, it uses the DefaultValue as the fallback value.
        /// </remarks>
        private void ConvertToString(string newValue)
        {
            if (IsSet && newValue != null)
                StringValue = newValue;
            else
                StringValue = DefaultValue;
        }

        /// <summary>
        /// Validates a Windows Registry key value against a set of allowed values, considering case sensitivity.
        /// </summary>
        /// <returns>True if the value matches any allowed value, otherwise false.</returns>
        private bool AssertAllowedValues()
        {
            return AllowedValues.Any(v =>
                CaseSensitive ? v == Value : v.Equals(Value, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Validates if a string value is equal to any of the enum values.
        /// </summary>
        /// <returns>True if the value matches any enum value, otherwise false.</returns>
        private bool AssertEnum()
        {
            string[] enumValues = Enum.GetValues(EnumType)
                 .Cast<Enum>()
                 .Select(e => e.ToString())
                 .ToArray();

            return enumValues.Any(v =>
                CaseSensitive ? v == Value : v.Equals(Value, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
