using Microsoft.Win32;
using System;

namespace core.WinRegistry
{
    internal static class WinRegistryErrorMessages
    {
        internal const string InvalidHiveParam = "Invalid parameter: Unknown or unsupported RegistryHive value.";
        internal const string InvalidPathParam = "Invalid parameter: Path cannot be null, empty, or consist only of whitespace characters.";
        internal const string InvalidNameParam = "Invalid parameter: Name cannot be null.";
        internal const string InvalidValueKindParam = "Invalid parameter: Unknown or unsupported RegistryValueKind value.";
        internal const string InvalidBaseKeyParam = "Invalid parameter: Registry entry cannot be null.";

        /// <summary>
        /// Validates the specified RegistryHive value.
        /// </summary>
        /// <param name="hive">The RegistryHive value to validate.</param>
        /// <exception cref="ArgumentException">Thrown when an unknown or unsupported RegistryHive value is provided.</exception>
        public static void ThrowIfHiveInvalid(RegistryHive Hive)
        {
            if (!Enum.IsDefined(typeof(RegistryHive), Hive) || Hive == RegistryHive.CurrentConfig || Hive == 0)
                throw new ArgumentException(InvalidHiveParam, nameof(Hive));
        }

        /// <summary>
        /// Throws an exception if the specified path is null or empty.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>The validated parameter path.</returns>
        public static void ThrowIfPathInvalid(string Path)
        {
            if (string.IsNullOrWhiteSpace(Path))
                throw new ArgumentException(InvalidPathParam, nameof(Path));
        }

        /// <summary>
        /// Throws an exception if the specified name is null or empty.
        /// </summary>
        /// <param name="name">The name to validate.</param>
        public static void ThrowIfNameInvalid(string Name)
        {
            if (Name == null)
                throw new ArgumentNullException(nameof(Name), InvalidNameParam);
        }

        /// <summary>
        /// Throws an exception if the specified RegistryValueKind is unknown.
        /// </summary>
        /// <param name="valueKind">The RegistryValueKind to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown when the RegistryValueKind is Unknown.</exception>
        public static void ThrowIfValueKindInvalid(RegistryValueKind ValueKind)
        {
            if (!Enum.IsDefined(typeof(RegistryValueKind), ValueKind) || ValueKind == RegistryValueKind.Unknown || ValueKind == RegistryValueKind.None)
                throw new ArgumentException(InvalidValueKindParam, nameof(ValueKind));
        }
    }
}
