
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class StringEntryTest
    {
        public enum TestEnum
        {
            First = 1,
            Second = 2,
            Third = 3
        }

        private Entry TestEntry = new()
        {
            Hive = RegistryHive.CurrentUser,
            Path = @"SOFTWARE\Microsoft",
            Name = "Windows",
            ValueKind = RegistryValueKind.String,
            Value = ""
        };

        [Test]
        public void IsValid_NoValidationSet_EntryComplete_ReturnsTrue()
        {
            StringEntry entry = new(TestEntry);
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_AllowedValuesSet_ValueInAllowedValues_ReturnsTrue()
        {
            TestEntry.Value = "Banana";
            StringEntry entry = new (TestEntry);
            entry.SetValidation(new string[] { "Banana", "Strawberry", "Apple" });
            Assert.That(entry.IsValid, Is.True);
        }
        
        [Test]
        public void IsValid_AllowedValuesSet_ValueNotInAllowedValues_ReturnsFalse()
        {
            TestEntry.Value = "Cheese";
            StringEntry entry = new(TestEntry);
            entry.SetValidation(new string[] { "Banana", "Strawberry", "Apple" });
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_EnumSet_ValueInEnum_ReturnsTrue()
        {
            TestEntry.Value = "Second";
            StringEntry entry = new(TestEntry);
            entry.SetValidation<TestEnum>();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_EnumSet_ValueNotInEnum_ReturnsFalse()
        {
            TestEntry.Value = "Fourth";
            StringEntry entry = new(TestEntry);
            entry.SetValidation<TestEnum>();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_InvalidValueKind_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                StringEntry entry = new(TestEntry.Hive, TestEntry.Path, TestEntry.Name, TestEntry.Value, RegistryValueKind.Unknown);
            });
        }
    }
}
