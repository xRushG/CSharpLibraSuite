using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace core.WinRegistry.RegistryEntry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Represents an entry in the Windows Registry, providing methods to read and write values.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the functionality to interact with registry entries, including reading and writing values.
    /// It provides methods to read and write values to the registry under a specified path and name, with support for various value types.
    /// Users can create instances of this class to work with specific registry entries, providing hive, path, and name parameters.
    /// </remarks>
    public class BaseRegistryEntry
    {
        #region Private and Protected Constants

        // Lists all error messages
        private const string InvalidHiveParamMessage = "Invalid parameter: Unknown or unsupported RegistryHive value.";
        private const string InvalidPathParamMessage = "Invalid parameter: Path cannot be null, empty, or consist only of whitespace characters.";
        private const string InvalidValueKindParamMessage = "Invalid parameter: Unknown or unsupported RegistryValueKind value.";

        protected const string InvalidBaseKeyParamMessage = "Invalid parameter: Registry entry cannot be null.";
        protected const string InvalidNegativeIntValueMessage = "Negative value not allowed for REG_DWORD or REG_QWORD parameter.";
        protected const string InvalidMinAndMaxMessage = "MinValue must be less than or equal to MaxValue.";

        protected const string UnableToReadMessage = "Unable to read registry key. Hive, path, and name are required.";
        protected const string UnableToWriteMessage = "Unable to write registry key. Hive, path, name, value kind, and value are required.";
        protected const string ErrorWriteMessage = "Error writing to the Windows Registry.";

        #endregion

        #region Public Property: RegistryHive

        /// <summary>
        /// Represents the registry hive associated with the registry key.
        /// </summary>
        /// /// <remarks>
        /// The default value is <see cref="RegistryHive.CurrentUser"/>.
        /// </remarks>
        public RegistryHive Hive
        {
            get { return StoredHive; }
            set
            {
                StoredHive = 
                    !Enum.IsDefined(typeof(RegistryHive), value) || value == RegistryHive.CurrentConfig || value == RegistryHive.ClassesRoot
                    ? throw new ArgumentException(InvalidHiveParamMessage, nameof(Hive))
                    : value;

            }
        }
        private RegistryHive StoredHive = RegistryHive.CurrentUser;

        #endregion

        #region Public Property: Path

        /// <summary>
        /// Represents the path of the registry entry.
        /// </summary>
        public string Path
        {
            get { return StoredPath; }
            set
            {
                StoredPath = 
                    !string.IsNullOrWhiteSpace(value)
                    ? value
                    : throw new ArgumentNullException(nameof(Path), InvalidPathParamMessage);
            }
        }
        private string StoredPath;

        #endregion

        #region Public Property: Name

        /// <summary>
        /// Represents the name of the registry entry.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Property: ValueKind

        /// <summary>
        /// Represents the kind of data stored in the registry value.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="RegistryValueKind.Unknown"/>, indicating 
        /// that the data type is not explicitly specified or is unknown.
        /// </remarks>
        public RegistryValueKind ValueKind
        {
            get => StoredValueKind;
            set
            {
                StoredValueKind =
                    !Enum.IsDefined(typeof(RegistryValueKind), value) || value == RegistryValueKind.Unknown || value == RegistryValueKind.None || value == 0
                    ? throw new ArgumentException(InvalidValueKindParamMessage, nameof(ValueKind))
                    : value;
            }
        }
        private RegistryValueKind StoredValueKind = RegistryValueKind.Unknown;

        #endregion

        #region Public Virtual Properties: Value and Dependents

        /// <summary>
        /// Represents the value of the registry entry.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// This property indicates whether the entry has been explicitly set.
        /// </summary>
        /// <remarks>
        /// When set to <c>true</c>, it signifies that the property has been assigned a value.
        /// When set to <c>false</c>, it indicates that the property has not been explicitly set
        /// </remarks>
        public bool IsSet
        {
            get => ProtectedHasValue();
        }

        /// <summary>
        /// Indicates whether the entry value is valid.
        /// </summary>
        /// <remarks>
        /// <remarks>
        /// It mirrors the behavior of IsSet property to determine if the value has been explicitly set.
        /// Derived classes may override this property to implement custom logic for validation.
        /// </remarks>
        public virtual bool IsValid
        {
            get => ProtectedHasValue();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the Entry class.
        /// </summary>
        public BaseRegistryEntry() { }

        /// <summary>
        /// Constructor for the Entry class with parameters.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        public BaseRegistryEntry(RegistryHive hive, string path, string name)
        {
            Hive = hive;
            Path = path;
            Name = name;
        }

        /// <summary>
        /// Constructor for the Entry class with additional parameters for value and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        public BaseRegistryEntry(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind)
        {
            Hive = hive;
            Path = path;
            Name = name;
            Value = value;
            ValueKind = valueKind;
        }

        #endregion

        #region Public Factory Method: New

        /// <summary>
        /// Creates a new instance of the Entry class with the specified registry hive, path, and name.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <returns>A new instance of the Entry class.</returns>
        public static BaseRegistryEntry New(RegistryHive hive, string path, string name)
        {
            return new BaseRegistryEntry(hive, path, name);
        }

        /// <summary>
        /// Creates a new instance of the Entry class with the specified registry hive, path, name, value, and value kind.
        /// </summary>
        /// <param name="hive">The registry hive of the entry.</param>
        /// <param name="path">The path of the registry entry.</param>
        /// <param name="name">The name of the registry entry.</param>
        /// <param name="value">The integer value of the registry entry.</param>
        /// <param name="valueKind">The value kind of the registry entry.</param>
        /// <returns>A new instance of the Entry class.</returns>
        public static BaseRegistryEntry New(RegistryHive hive, string path, string name, string value, RegistryValueKind valueKind)
        {
            return new BaseRegistryEntry(hive, path, name, value.ToString(), valueKind);
        }

        #endregion

        #region Public Virtual Fluent Interface: Read, Write

        /// <summary>
        /// Reads the value of the registry entry from the specified registry path and assigns it to the Value property.
        /// </summary>
        public virtual BaseRegistryEntry Read()
        {
            Value = ProtectedRead();
            return this;
        }

        /// <summary>
        /// Writes the value of the registry entry to the specified registry path.
        /// </summary>

        public virtual BaseRegistryEntry Write()
        {
            ProtectedWrite();
            return this;
        }

        #endregion

        #region Public Methods: IsKeyReadable and IsKeyWritable

        /// <summary>
        /// Checks if the Windows Registry key is ready for reading by ensuring that the hive,
        /// path, and name properties are set.
        /// </summary>
        /// <returns>True if the key is ready for reading, otherwise false.</returns>
        public bool IsKeyReadable()
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
        public bool IsKeyWritable()
        {
            return IsHiveSet() && IsValueKindSet() && IsPathSet(); //&& IsNameSet() && IsValueSet();
        }

        #endregion

        #region Protected Methods: ProtectedRead, ProtectedWrite, IsValueSet

        /// <summary>
        /// Reads the value of the registry entry from the specified registry path and returns it as a string.
        /// </summary>
        /// <returns>The value of the registry entry as a string.</returns>
        protected string ProtectedRead()
        {
            if (!IsKeyReadable())
                throw new InvalidOperationException(UnableToReadMessage);

            string value = null;
            string name = string.IsNullOrEmpty(Name) ? null : Name;

            try
            {
                using var key = RegistryKey.OpenBaseKey(Hive, RegistryView.Default).OpenSubKey(Path);
                if (key != null)
                    value = key.GetValue(name)?.ToString();

                if (value != null)
                    ValueKind = key.GetValueKind(name);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error reading the Windows Registry.", ex);
            }

            return value;
        }

        /// <summary>
        /// Writes the registry value to the specified registry path, handling exceptions if they occur.
        /// </summary>
        protected void ProtectedWrite()
        {
            if (!IsKeyWritable())
                throw new InvalidOperationException(UnableToWriteMessage);

            string name = string.IsNullOrEmpty(Name) ? null : Name;
            RegistryValueKind valueKind = string.IsNullOrEmpty(Name) ? RegistryValueKind.String : ValueKind;

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Default);
                using RegistryKey registryKey = baseKey.CreateSubKey(Path, true);

                registryKey.SetValue(name, Value, valueKind);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ErrorWriteMessage, ex);
            }
        }

        /// <summary>
        /// Determines whether the value of the registry entry has been explicitly set.
        /// </summary>
        /// <returns><c>true</c> if the value is set; otherwise, <c>false</c>.</returns>
        protected virtual bool ProtectedHasValue() => Value != null;

        #endregion

        #region Private Methods: IsHiveSet, IsValueKindSet, IsPathSet, and IsNameSet

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
    }
}
