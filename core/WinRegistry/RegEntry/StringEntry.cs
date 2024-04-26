﻿using System;
using System.Linq;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    public class StringEntry : Entry
    {

        #region private attributes
        private string[] AllowedValues;
        private bool CaseSensitive;
        private Type EnumType;
        #endregion

        #region public attributes
        /// <summary>
        /// Represents a default string value.
        /// </summary>
        public string DefaultValue { get; private set; }

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
                if (!IsValueSet())
                    return false;

                if (AllowedValues != null)
                    return (bool)AssertAllowedValues();

                if (EnumType != null && EnumType.IsEnum)
                    return (bool)AssertEnum();

                return true;
            }
        }
        #endregion

        #region public
        /// <summary>
        /// Initializes a new instance with the specified base key and default value.
        /// </summary>
        /// <param name="RegistryEntry">The base Windows Registry key to derive properties from.</param>
        /// <param name="defaultValue">The default string value (default: null).</param>
        /// <exception cref="ArgumentNullException">Thrown when baseKey is null.</exception>
        public StringEntry(Entry RegistryEntry, string defaultValue = null)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), WinRegistryErrorMessages.InvalidBaseKeyParam);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = RegistryEntry.ValueKind;
            base.Value = RegistryEntry.Value;

            DefaultValue = defaultValue;
            ConvertToString(RegistryEntry.Value);
        }

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

        #endregion

        #region private
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
