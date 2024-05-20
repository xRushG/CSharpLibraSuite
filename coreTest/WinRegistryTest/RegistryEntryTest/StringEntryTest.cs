﻿
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegistryEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class StringEntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\StringEntryTest";

        public enum TestEnum
        {
            First = 1,
            Second = 2,
            Third = 3
        }

        [Test]
        public void IsValid_NoValidationSet_EntryComplete_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "IsValid").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid").Read();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_AllowedValuesSet_ValueInAllowedValues_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "Banana").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid").SetValidation(new string[] { "Banana", "Strawberry", "Apple" }).Read();
            Assert.That(entry.IsValid, Is.True);
        }
        
        [Test]
        public void IsValid_AllowedValuesSet_ValueNotInAllowedValues_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "Cheese").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid").SetValidation(new string[] { "Banana", "Strawberry", "Apple" }).Read();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_EnumSet_ValueInEnum_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "Second").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid").SetValidation<TestEnum>().Read();
            Assert.That(entry.IsValid, Is.True);
        }

        [Test]
        public void IsValid_EnumSet_ValueNotInEnum_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "Fourth").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid").SetValidation<TestEnum>().Read();
            Assert.That(entry.IsValid, Is.False);
        }

        [Test]
        public void IsValid_InvalidValueKind_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => WinRegistryEntry<string>.New(TestHive, TestPath, "IsValid", "Windows").SetValueKind(RegistryValueKind.Unknown));
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsStrJustATest()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<string>.New(TestHive, TestPath, "FluentReadAndWriteTest", "JustATest").Write());
            var entry = WinRegistryEntry<string>.New(TestHive, TestPath, "FluentReadAndWriteTest", "TestFailed").Read();
            Assert.That(entry.Value, Is.EqualTo("JustATest"));
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
