using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    public class IntegerEntry : Entry
    {
        #region private attributes
        private int[] AllowedValues;
        private int? MinValue;
        private int? MaxValue;
        private Type EnumType;
        #endregion

        #region public attributes
        /// <summary>
        /// Represents a default string value.
        /// </summary>
        /// <remarks>
        /// The default value is initially set to `-1`.
        /// </remarks>
        public int DefaultValue { get; private set; } = -1;


        /// <summary>
        /// Represents the integer value associated with the windows registry key.
        /// </summary>
        public new int Value
        {
            get => IntegerValue;
            private set
            {
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
                if (!IsValueSet())
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

        #region public
        public IntegerEntry() { }
        /// <summary>
        /// Initializes a new instance with the specified base key and default value.
        /// </summary>
        /// <param name="RegistryEntry">The base windows registry key to derive properties from.</param>
        /// <param name="defaultValue">The default string value (default: null).</param>
        /// <exception cref="ArgumentNullException">Thrown when baseKey is null.</exception>
        public IntegerEntry(Entry RegistryEntry, int? defaultValue = null)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), WinRegistryErrorMessages.InvalidBaseKeyParam);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = RegistryEntry.ValueKind;
            base.Value = RegistryEntry.Value;

            if (defaultValue.HasValue)
                DefaultValue = (int)defaultValue;

            ConvertToInteger(RegistryEntry.Value);
        }

        /// <summary>
        /// Sets up validation using an array of allowed integer values.
        /// </summary>
        /// <param name="allowedValues">The array of allowed integer values.</param>
        public void SetValidation(int[] allowedValues)
        {
            if (allowedValues != null && allowedValues.Length > 0)
                AllowedValues = allowedValues;
        }

        /// <summary>
        /// Sets up validation for a range of integer values.
        /// </summary>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        public void SetValidation(int minValue, int maxValue)
        {
            if (minValue <= maxValue)
            {
                MinValue = minValue;
                MaxValue = maxValue;
            }
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

        #endregion

        #region private
        /// <summary>
        /// Converts the provided string value to an integer and updates the IntegerValue property.
        /// </summary>
        /// <param name="newValue">The string value to convert.</param>
        /// <remarks>
        /// If the conversion is successful and the value is set, it updates the IntegerValue property.
        /// If the conversion fails or the value is not set, it uses the DefaultValue as the fallback value.
        /// </remarks>
        private void ConvertToInteger(string newValue)
        {
            if (IsSet && int.TryParse(newValue.ToString(), out int intValue))
                IntegerValue = intValue;
            else
                IntegerValue = DefaultValue;
        }

        /// <summary>
        /// Determines whether the registry value is considered.
        /// </summary>
        protected override bool IsValueSet()
        {
            if (ValueKind != RegistryValueKind.DWord)
                return false;

            return base.Value != null;
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
