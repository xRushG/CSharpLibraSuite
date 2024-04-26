using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    public class BoolEntry : Entry
    {
        #region public attributes
        /// <summary>
        /// Gets or sets the default boolean value for a Windows Registry key.
        /// </summary>
        /// <remarks>
        /// The default value is initially set to `false`.
        /// </remarks>
        public bool DefaultValue { get; private set; } = false;

        /// <summary>
        /// Gets or sets the boolean value for a Windows Registry key.
        /// </summary>
        public new bool Value
        {
            get => BoolValue;
            private set
            {
                BoolValue = value;
            }
        }
        private bool BoolValue;

        #endregion

        #region public
        /// <summary>
        /// Initializes a new instance with the specified base key and default value.
        /// </summary>
        /// <param name="RegistryEntry">The base Windows Registry key to derive properties from.</param>
        /// <param name="defaultValue">The default string value (default: null).</param>
        /// <exception cref="ArgumentNullException">Thrown when baseKey is null.</exception>
        public BoolEntry(Entry RegistryEntry, bool? defaultValue = null)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), WinRegistryErrorMessages.InvalidBaseKeyParam);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = RegistryEntry.ValueKind;
            base.Value = RegistryEntry.Value;

            if (defaultValue.HasValue)
                DefaultValue = (bool)defaultValue;

            ConvertToBool(RegistryEntry.Value);
        }
        #endregion

        #region private
        /// <summary>
        /// Converts the provided string value to a boolean and updates the BoolValue property.
        /// </summary>
        /// <param name="newValue">The string value to convert.</param>
        /// <remarks>
        /// If the conversion to boolean is successful and the value is set, it updates the BoolValue property.
        /// If the conversion to boolean fails but the value is set and can be parsed as an integer, it updates the BoolValue property based on whether the integer value equals 1.
        /// If neither conversion succeeds or the value is not set, it uses the DefaultValue as the fallback value.
        /// </remarks>
        private void ConvertToBool(string newValue)
        {
            if (IsSet && bool.TryParse(newValue, out bool boolValue))
                BoolValue = boolValue;
            else if (IsSet && int.TryParse(newValue, out int intValue))
                BoolValue = intValue == 1;
            else
                BoolValue = DefaultValue;
        }

        /// <summary>
        /// Determines whether the registry value is considered.
        /// </summary>
        protected override bool IsValueSet()
        {
            if (!(ValueKind == RegistryValueKind.String || ValueKind == RegistryValueKind.DWord))
                return false;

            return base.Value != null;
        }
        #endregion
    }
}
