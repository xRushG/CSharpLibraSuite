﻿
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegistryEntryTest
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

        [Test]
        public void IsValid_NoValidationSet_EntryComplete_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 1).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").Read();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_AllowedValuesSet_ValueInAllowedValues_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 2).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation(new int[] { 1, 2, 3 }).Read();
            Assert.That(entry.IsValid, Is.True);
        }
        
        [Test]
        public void IsValid_AllowedValuesSet_ValueNotInAllowedValues_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 4).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation(new int[] { 1, 2, 3 }).Read();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_RangeSet_ValueInRange_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 5).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation(1,10).Read();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_RangeSet_ValueOutOfRange_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 50).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation(1, 10).Read();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_EnumSet_ValueInEnum_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 2).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation<TestEnum>().Read();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_EnumSet_ValueNotInEnum_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 5).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid").SetValidation<TestEnum>().Read();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_InvalidValueKind_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", 5).SetValueKind(RegistryValueKind.Unknown));
        }

        [Test]
        public void Value_SetToNegativeNumber_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WinRegistryEntry<int>.New(TestHive, TestPath, "IsValid", -100));
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsInt12()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<int>.New(TestHive, TestPath, "FluentReadAndWriteTest", 12).Write());
            var entry = WinRegistryEntry<int>.New(TestHive, TestPath, "FluentReadAndWriteTest", 42).Read();
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
