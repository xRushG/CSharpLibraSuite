using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace core.WinRegistry.RegEntry
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// The WinRegistryEntry class represents a registry entry in the Windows registry. 
    /// It provides properties for the registry hive, path, name, and value, along with methods 
    /// for checking the readiness of the registry key for reading and writing operations. 
    /// </summary>
    public class Entry
    {
        #region properties
        #region Property registryHive
        /// <summary>
        /// Represents the registry hive associated with the registry key.
        /// </summary>
        public RegistryHive Hive
        {
            get { return storedHive; }
            set
            {
                storedHive = 
                    !Enum.IsDefined(typeof(RegistryHive), value) || value == RegistryHive.CurrentConfig || value == 0
                    ? throw new ArgumentException(WinRegistryErrorMessages.InvalidHiveParam, nameof(Hive))
                    : value;

            }
        }
        private RegistryHive storedHive { get; set; }
        #endregion

        #region Property path
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
                    : throw new ArgumentNullException(nameof(Path), WinRegistryErrorMessages.InvalidPathParam);
            }
        }
        private string StoredPath { get; set; }
        #endregion

        #region Property name
        /// <summary>
        /// Represents the name of the registry entry.
        /// </summary>
        public string Name
        {
            get { return StoredName; }
            set
            {
                StoredName = 
                    !string.IsNullOrWhiteSpace(value) 
                    ? value 
                    : throw new ArgumentNullException(nameof(Name), WinRegistryErrorMessages.InvalidNameParam);
            }
        }
        private string StoredName { get; set; }
        #endregion

        #region Property value
        /// <summary>
        /// Represents the value of the registry entry.
        /// </summary>
        public virtual string Value
        {
            get => StoredValue;
            set
            {
                StoredValue = value;
            }
        }
        private string StoredValue;

        /// <summary>
        /// This property indicates whether the entry has been explicitly set.
        /// </summary>
        /// <remarks>
        /// When set to <c>true</c>, it signifies that the property has been assigned a value.
        /// When set to <c>false</c>, it indicates that the property has not been explicitly set
        /// </remarks>
        public bool IsSet
        {
            get => IsValueSet();
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
            get => IsValueSet();
        }

        #endregion

        /// <summary>
        /// Represents the kind of data stored in the registry value.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="RegistryValueKind.Unknown"/>, indicating 
        /// that the data type is not explicitly specified or is unknown.
        /// </remarks>
        public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.Unknown;

        #endregion

        #region public methods
        /// <summary>
        /// Checks if the Windows Registry key is ready for reading by ensuring that the hive,
        /// path, and name properties are set.
        /// </summary>
        /// <returns>True if the key is ready for reading, otherwise false.</returns>
        public bool IsKeyReadable()
        {
            return IsHiveSet() && IsPathSet() && IsNameSet();
        }

        /// <summary>
        /// Checks if the Windows Registry key is ready for a write operation.
        /// The key is considered write-ready if none of the following conditions are met:
        /// - The hive is set
        /// - The registry value type is set
        /// - The key path is set
        /// - The value name is set
        /// </summary>
        /// <returns>Returns true if the key is write-ready, otherwise false.</returns>
        public bool IsKeyWritable()
        {
            return IsHiveSet() && IsValueKindSet() && IsPathSet() && IsNameSet() && IsValueSet();
        }
        #endregion

        #region private methods
        protected virtual bool IsValueSet() => Value != null;

        private bool IsHiveSet() => Hive != 0;

        private bool IsValueKindSet() => ValueKind != 0;

        private bool IsPathSet() => Path != null;

        private bool IsNameSet() => Name != null;
        #endregion
    }
}
