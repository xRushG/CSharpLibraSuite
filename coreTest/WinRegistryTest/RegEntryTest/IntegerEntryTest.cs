
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Xml.Linq;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class IntegerEntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\IntegerEntryTest";

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
        public void IsValid_InvalidValueKind_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                IntegerEntry entry = new(TestEntry.Hive, TestEntry.Path, TestEntry.Name, TestEntry.Value, RegistryValueKind.Unknown);
            });
        }

        [Test]
        public void Value_SetToNegativeNumber_ThrowArgumentException()
        {
            TestEntry.Value = "-100";
            Assert.Throws<ArgumentException>(() =>
            {
                IntegerEntry entry = new(TestEntry);
            });
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsInt12()
        {
            string name = "FluentReadAndWriteTest";

            Assert.DoesNotThrow(() => IntegerEntry.New(TestHive, TestPath, name, 12, RegistryValueKind.DWord).Write());

            IntegerEntry entry = IntegerEntry.New(TestHive, TestPath, name, 42).Read();
            Assert.That(entry.Value, Is.EqualTo(12));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            // Delete all created tests
            WinRegistry winReg = new();
            winReg.DeleteTree(TestHive, TestPath);
        }
    }
}
