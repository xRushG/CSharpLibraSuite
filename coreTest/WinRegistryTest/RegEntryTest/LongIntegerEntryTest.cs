
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class LongIntegerEntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\LongIntegerEntryTest";

        private Entry TestEntry = new()
        {
            Hive = RegistryHive.CurrentUser,
            Path = @"SOFTWARE\Microsoft",
            Name = "Windows",
            ValueKind = RegistryValueKind.DWord,
            Value = "3047483647"
        };

        [Test]
        public void IsValid_NoValidationSet_EntryComplete_ReturnsTrue()
        {
            LongIntEntry entry = new(TestEntry);
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_AllowedValuesSet_ValueInAllowedValues_ReturnsTrue()
        {
            TestEntry.Value = "2147483648";
            LongIntEntry entry = new (TestEntry);
            entry.SetValidation(new long[] { 2147483648, 2147483649, 2147483650 });
            Assert.That(entry.IsValid, Is.True);
        }
        
        [Test]
        public void IsValid_AllowedValuesSet_ValueNotInAllowedValues_ReturnsFalse()
        {
            TestEntry.Value = "4";
            LongIntEntry entry = new(TestEntry);
            entry.SetValidation(new long[] { 2147483648, 2147483649, 2147483650 });
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_RangeSet_ValueInRange_ReturnsTrue()
        {
            TestEntry.Value = "2147483650";
            LongIntEntry entry = new(TestEntry);
            entry.SetValidation(2147483647, 2200000000);
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_RangeSet_ValueOutOfRange_ReturnsFalse()
        {
            TestEntry.Value = "20";
            LongIntEntry entry = new(TestEntry);
            entry.SetValidation(2147483647, 2200000000);
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_InvalidValueKind_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                LongIntEntry entry = new(TestEntry.Hive, TestEntry.Path, TestEntry.Name, TestEntry.Value, RegistryValueKind.Unknown);
            });
        }

        [Test]
        public void Value_SetToNegativeNumber_ThrowArgumentException()
        {
            TestEntry.Value = "-100";
            Assert.Throws<ArgumentException>(() =>
            {
                LongIntEntry entry = new(TestEntry);
            });
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsLong15()
        {
            string name = "FluentReadAndWriteTest";

            Assert.DoesNotThrow(() => LongIntEntry.New(TestHive, TestPath, name, 15, RegistryValueKind.QWord).Write());

            LongIntEntry entry = LongIntEntry.New(TestHive, TestPath, name, 42).Read();
            Assert.That(entry.Value, Is.EqualTo(15));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            // Delete all created tests
            WinRegistry winReg = new();
            winReg.DeleteTree(TestHive, TestRoot);
        }
    }
}
