using Microsoft.Win32;
using System.Collections.Generic;

namespace core.WinRegistry.RegEntry
{
    /// <summary>
    /// Interface for providing enhanced control and functionality to interact with the Windows Registry.
    /// </summary>
    public interface IRegistryEntryUtility
    {
        Entry GetEntry(RegistryHive hive, string path, string name);
        List<Entry> GetEntries(RegistryHive hive, string path);
        List<Entry> GetEntriesRecursive(RegistryHive hive, string path);
        IntegerEntry GetInteger(RegistryHive hive, string path, string name, int defaultValue = 0);
        IntegerEntry GetInteger<TEnum>(RegistryHive hive, string path, string name, int defaultValue = 0) where TEnum : System.Enum;
        IntegerEntry GetInteger(RegistryHive hive, string path, string name, int[] allowedValues, int defaultValue = 0);
        IntegerEntry GetInteger(RegistryHive hive, string path, string name, int minimum, int maximum, int defaultValue = 0);
        LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long defaultValue = 0);
        LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long[] allowedValues, long defaultValue = 0);
        LongIntEntry GetLongInt(RegistryHive hive, string path, string name, long minimum, long maximum, long defaultValue = 0);
        StringEntry GetString(RegistryHive hive, string path, string name, string defaultValue = null);
        StringEntry GetString(RegistryHive hive, string path, string name, string[] allowedValues, string defaultValue = null);
        StringEntry GetString<TEnum>(RegistryHive hive, string path, string name, string defaultValue = null) where TEnum : System.Enum;
        BoolEntry GetBoolean(RegistryHive hive, string path, string name, bool defaultValue = false);
    }
}