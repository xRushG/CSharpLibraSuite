using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace core.WinRegistry.RegistryEntry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Represents a boolean entry in the Windows Registry, providing methods to read and write boolean values.
    /// </summary>
    /// <remarks>
    /// This class extends the functionality of the base Entry class to specifically handle boolean values in the Windows Registry.
    /// It provides methods to read and write boolean values to the registry under a specified path and name.
    /// Users can create instances of this class to work with boolean registry entries, providing hive, path, and name parameters.
    /// </remarks>
    public class BoolEntry : BaseRegistryEntry
    {
        #region Public Property: DefaultValue, IsDefault

        /// <summary>
        /// Gets or sets the default boolean value for a Windows Registry key.
        /// </summary>
        public bool DefaultValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this entry is using its default value.
        /// </summary>
        public bool IsDefault { get; private set; }

        #endregion

        #region Public Override Property: Value, IsValid

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

        /// <summary>
        /// Indicates whether the value is valid based on defined validation criteria.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (!ProtectedHasValue())
                    return false;

                if (IsDefault)
                    return false;

                return true;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Entry class.
        /// </summary>
        public BoolEntry() : base() { }

        /// <summary>
        /// Constructor for the Entry class with parameters.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="defaultValue">The default integer value (default: null).</param>
        public BoolEntry(RegistryHive hive, string path, string name, bool defaultValue = false)
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
        public BoolEntry(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind, bool defaultValue = false)
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
        public BoolEntry(BaseRegistryEntry RegistryEntry, bool defaultValue = false)
        {
            if (RegistryEntry == null)
                throw new ArgumentNullException(nameof(RegistryEntry), InvalidBaseKeyParamMessage);

            Hive = RegistryEntry.Hive;
            Path = RegistryEntry.Path;
            Name = RegistryEntry.Name;
            ValueKind = DetermineValueKind(RegistryEntry.Value);
            base.Value = RegistryEntry.Value;

            SetDefaultValue(defaultValue);
            ConvertToBool(RegistryEntry.Value);
        }

        #endregion

        #region Public Factory Method: New

        /// <summary>
        /// Creates a new instance of the BoolEntry class with the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <returns>A new instance of the BoolEntry class.</returns>
        public static BoolEntry New(RegistryHive hive, string path, string name, bool defaultValue = false)
        {
            return new BoolEntry(hive, path, name, defaultValue);
        }

        /// <summary>
        /// Creates a new instance of the BoolEntry class with the specified registry hive, path, name, value, and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The integer value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        /// <returns>A new instance of the BoolEntry class.</returns>
        public static BoolEntry New(RegistryHive hive, string path, string name, int value, RegistryValueKind valueKind, bool defaultValue = false)
        {
            return new BoolEntry(hive, path, name, value.ToString(), valueKind, defaultValue);
        }

        #endregion

        #region Public Override Fluent Interface: Read, Write

        public override BoolEntry Read()
        {
            if (!IsKeyReadable())
                throw new InvalidOperationException(UnableToReadMessage);

            string stringValue = base.Value = ProtectedRead();
            ConvertToBool(stringValue);

            return this;
        }

        /// <summary>
        /// Writes the specified value to the Windows Registry under the given path, name, and value kind.
        /// </summary>
        /// <remarks>
        /// Value is based on ValueKind. 
        /// When ValueKind is DWord, it writes '0' or '1'. 
        /// When ValueKind is String, it writes 'true' or 'false'.
        /// </remarks>
        public override BoolEntry Write()
        {
            if (!IsKeyWritable())
                throw new InvalidOperationException(UnableToWriteMessage);

            string writeValue = Value ? (ValueKind == RegistryValueKind.DWord ? "1" : "True") 
                : (ValueKind == RegistryValueKind.DWord ? "0" : "False");

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Default);
                using RegistryKey registryKey = baseKey.CreateSubKey(Path, true);
                registryKey.SetValue(Name, writeValue, ValueKind);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ErrorWriteMessage, ex);
            }

            return this;
        }

        #endregion

        #region Public Methods: SetDefaultValue

        /// <summary>
        /// Sets the default value for the registry entry. The value must be a non-negative integer.
        /// </summary>
        /// <param name="defaultValue">The default value to be set.</param>
        public void SetDefaultValue(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        #endregion

        #region Override Methods: IsValueSet

        protected override bool ProtectedHasValue()
        {
            if (!(ValueKind == RegistryValueKind.String || ValueKind == RegistryValueKind.DWord))
                return false;

            return base.Value != null;
        }

        #endregion

        #region Private Methods: ConvertToBool, DetermineValueKind

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
        /// Determines the appropriate RegistryValueKind based on the input string.
        /// </summary>
        /// <param name="value">The input string to evaluate.</param>
        /// <returns>The RegistryValueKind determined based on the input string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input string is not "true", "false", "0", or "1".</exception>
        private static RegistryValueKind DetermineValueKind(string value)
        {
            string lcValue = value.ToLower();
            if (lcValue == "true" || lcValue == "false")
            {
                return RegistryValueKind.String; //  Represent "true"/"false" as DWORD
            }
            else if (lcValue == "0" || lcValue == "1")
            {
                return RegistryValueKind.DWord; // Represent "0"/"1" as DWORD
            }
            else
            {
                throw new ArgumentException("Invalid value. Supported values are 'true', 'false', '0', and '1'.", nameof(value));
            }
        }

        #endregion
    }
}
