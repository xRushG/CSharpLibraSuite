
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class IntegerEntryTest
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
            ValueKind = RegistryValueKind.DWord,
            Value = "1"
        };

        [Test]
        public void IsValid_NoValidationSet_EntryComplete_ReturnsTrue()
        {
            IntegerEntry entry = new(TestEntry);
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_AllowedValuesSet_ValueInAllowedValues_ReturnsTrue()
        {
            TestEntry.Value = "2";
            IntegerEntry entry = new (TestEntry);
            entry.SetValidation(new int[] { 1, 2, 3 });
            Assert.That(entry.IsValid, Is.True);
        }
        
        [Test]
        public void IsValid_AllowedValuesSet_ValueNotInAllowedValues_ReturnsFalse()
        {
            TestEntry.Value = "4";
            IntegerEntry entry = new(TestEntry);
            entry.SetValidation(new int[] { 1, 2, 3 });
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_RangeSet_ValueInRange_ReturnsTrue()
        {
            TestEntry.Value = "5";
            IntegerEntry entry = new(TestEntry);
            entry.SetValidation(1, 10);
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_RangeSet_ValueOutOfRange_ReturnsFalse()
        {
            TestEntry.Value = "20";
            IntegerEntry entry = new(TestEntry);
            entry.SetValidation(1, 10);
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_EnumSet_ValueInEnum_ReturnsTrue()
        {
            TestEntry.Value = "2";
            IntegerEntry entry = new(TestEntry);
            entry.SetValidation<TestEnum>();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_EnumSet_ValueNotInEnum_ReturnsFalse()
        {
            TestEntry.Value = "5";
            IntegerEntry entry = new(TestEntry);
            entry.SetValidation<TestEnum>();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_InvalidValueKind_ReturnsFalse()
        {
            TestEntry.ValueKind = RegistryValueKind.Unknown; // change ValueKind
            IntegerEntry entry = new(TestEntry);
            TestEntry.ValueKind = RegistryValueKind.DWord; // change ValueKind back
            Assert.That(entry.IsValid, Is.False);
        }
    }
}
