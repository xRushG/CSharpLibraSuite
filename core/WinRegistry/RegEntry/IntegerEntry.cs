using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Represents an integer entry in the Windows Registry, providing methods to read and write integer values.
    /// </summary>
    /// <remarks>
    /// This class extends the functionality of the base Entry class to specifically handle integer values in the Windows Registry.
    /// It provides methods to read and write integer values to the registry under a specified path and name.
    /// Users can create instances of this class to work with integer registry entries, providing hive, path, and name parameters.
    /// </remarks>
    public class IntegerEntry : Entry
    {
        #region Private Attributes

        private int[] AllowedValues;
        private int? MinValue;
        private int? MaxValue;
        private Type EnumType;

        #endregion

        #region Public Property: DefaultValue, IsDefault

        /// <summary>
        /// Represents a default integer value.
        /// </summary>
        /// <remarks>
        /// The default value is initially set to `0`.
        /// </remarks>
        public int DefaultValue { get; private set; } = 0;

        /// <summary>
        /// Gets a value indicating whether this entry is using its default value.
        /// </summary>
        public bool IsDefault { get; private set; }

        #endregion

        #region Public Override Property: Value, IsValid

        /// <summary>
        /// Represents the integer value associated with the windows registry key.
        /// </summary>
        public new int Value
        {
            get => IntegerValue;
            private set
            {
                if (value < 0)
                    throw new ArgumentException(InvalidNegativeIntValueMessage, nameof(Value));

                IntegerValue = value;
            }
        }
        private int IntegerValue;

        /// <summary>
        /// Indicates whether the value is valid based on defined validation criteria.
        /// </summary>
        /// <remarks>
        /// Returns true if the value is valid, otherwise false. 
        /// Validity is determined by checking against allowed values, a specified range, or an enumeration type if provided.
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

                if (MinValue != null && MaxValue != null)
                    return (bool)AssertRange();

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
        public IntegerEntry() : base() { }

        /// <summary>
        /// Constructor for the Entry class with parameters.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="defaultValue">The default integer value (default: null).</param>
        public IntegerEntry(RegistryHive hive, string path, string name, int defaultValue = 0) 
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
        public IntegerEntry(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind, int defaultValue = 0)
            : base(hive, path, name, value, valueKind) 
        {
            SetDefaultValue(defaultValue);
        }

        /// <summary>
        /// Initializes a new instance with the specified base key and default value.
        /// </summary>
        /// <param name="RegistryEntry">The base windows registry key to derive properties from.</param>
        /// <param name="defaultValue">The default integer value (default: null).</param>
        /// <exception cref="ArgumentNullException">Thrown when baseKey is null.</exception>
        public IntegerEntry(Entry RegistryEntry, int defaultValue = 0)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), InvalidBaseKeyParamMessage);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = RegistryValueKind.DWord;
            base.Value = RegistryEntry.Value;

            SetDefaultValue(defaultValue);
            ConvertToInt(RegistryEntry.Value);
        }

        #endregion

        #region Public Factory Method: New

        /// <summary>
        /// Creates a new instance of the IntegerEntry class with the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <returns>A new instance of the IntegerEntry class.</returns>
        public static IntegerEntry New(RegistryHive hive, string path, string name, int defaultValue = 0)
        {
            return new IntegerEntry(hive, path, name, defaultValue);
        }

        /// <summary>
        /// Creates a new instance of the IntegerEntry class with the specified registry hive, path, name, value, and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The integer value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        /// <returns>A new instance of the IntegerEntry class.</returns>
        public static IntegerEntry New(RegistryHive hive, string path, string name, int value, RegistryValueKind valueKind, int defaultValue = 0)
        {
            return new IntegerEntry(hive, path, name, value.ToString(), valueKind, defaultValue);
        }

        #endregion

        #region Public Override Fluent Interface: Read

        public override IntegerEntry Read()
        {
            if (!IsKeyReadable())
                throw new InvalidOperationException(UnableToReadMessage);

            string stringValue = base.Value = ProtectedRead();
            ConvertToInt(stringValue);
            return this;
        }

        #endregion

        #region Public Methods: SetValidation, SetDefaultValue

        /// <summary>
        /// Sets up validation using an array of allowed integer values.
        /// </summary>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        public void SetValidation(int[] allowedValues)
        {
            if (allowedValues == null || allowedValues.Length == 0)
            {
                AllowedValues = null;
            }
            else if (allowedValues.Any(value => value < 0))
            {
                throw new ArgumentException(InvalidNegativeIntValueMessage, nameof(allowedValues));
            }
            else
            {
                AllowedValues = allowedValues;
            }
        }

        /// <summary>
        /// Sets up validation for a range of integer values.
        /// </summary>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        public void SetValidation(int minValue, int maxValue)
        {
            if (minValue < 0)
            {
                throw new ArgumentException(InvalidNegativeIntValueMessage, nameof(minValue));
            }
            else if (maxValue < 0)
            {
                throw new ArgumentException(InvalidNegativeIntValueMessage, nameof(maxValue));
            }
            else if (minValue > maxValue)
            {
                throw new ArgumentException(InvalidMinAndMaxMessage);
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }


        /// <summary>
        /// Sets up validation for an enumeration type.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        public void SetValidation<TEnum>() where TEnum : Enum
        {
            Type enumType = typeof(TEnum);
            EnumType = enumType;
        }

        /// <summary>
        /// Sets the default value for the registry entry. The value must be a non-negative integer.
        /// </summary>
        /// <param name="defaultValue">The default value to be set.</param>
        /// <exception cref="ArgumentException">Thrown when the specified default value is negative.</exception>
        public void SetDefaultValue(int defaultValue)
        {
            if (defaultValue < 0)
                throw new ArgumentException(InvalidNegativeIntValueMessage, nameof(defaultValue));

            DefaultValue = defaultValue;
        }

        #endregion

        #region Override Method: ProtectedHasValue
        protected override bool ProtectedHasValue()
        {
            return ValueKind == RegistryValueKind.DWord
                && base.Value != null;
        }

        #endregion

        #region Private Methods: ConvertToInt, AssertAllowedValues, AssertRange, AssertEnum

        /// <summary>
        /// Converts the provided string value to an integer and updates the IntegerValue property.
        /// </summary>
        /// <param name="newValue">The string value to convert.</param>
        /// <remarks>
        /// If the conversion is successful and the value is set, it updates the IntegerValue property.
        /// If the conversion fails or the value is not set, it uses the DefaultValue as the fallback value.
        /// </remarks>
        private void ConvertToInt(string newValue)
        {
            if (IsSet && int.TryParse(newValue, out int intValue))
            {
                Value = intValue;
                IsDefault = false;
            }
            else
            {
                Value = DefaultValue;
                IsDefault = true;
            }
        }

        /// <summary>
        /// Validates if the integer value matches any value in the provided array of allowed values.
        /// </summary>
        /// <returns>True if the value is valid, otherwise false.</returns>
        private bool AssertAllowedValues()
        {
            return AllowedValues.Contains(IntegerValue);
        }

        /// <summary>
        /// Validates if the integer value falls within the specified range.
        /// </summary>
        /// <returns>True if the value is within the range, otherwise false.</returns>
        private bool AssertRange()
        {
            return IntegerValue >= MinValue && IntegerValue <= MaxValue;
        }

        /// <summary>
        /// Validates if the integer value is within the range of enum values.
        /// </summary>
        /// <returns>True if the value is within the enum range, otherwise false.</returns>
        private bool AssertEnum()
        {
            Array enumValues = Enum.GetValues(EnumType);
            foreach (object enumValue in enumValues)
            {
                if (Convert.ToInt32(enumValue) == IntegerValue)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
