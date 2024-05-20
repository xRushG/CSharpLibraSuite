﻿
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegistryEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class BoolEntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\BoolEntryTest";

        [Test]
        public void StringTrue_SuccessfulToBool_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<bool>.New(TestHive, TestPath, "IsTrueString", true).Write());
            var entry = WinRegistryEntry<bool>.New(TestHive, TestPath, "IsTrueString").Read();
            Assert.That(entry.Value, Is.True);
        }

        [Test]
        public void StringFalse_SuccessfulToBool_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<bool>.New(TestHive, TestPath, "IsFalseString", false).Write());
            var entry = WinRegistryEntry<bool>.New(TestHive, TestPath, "IsFalseString").SetDefaultValue(true).Read();
            Assert.That(entry.Value, Is.False);
        }

        [Test]
        public void DWordTrue_SuccessfulToBool_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<bool>.New(TestHive, TestPath, "IsTrueDword", true).SetValueKind(RegistryValueKind.DWord).Write());
            var entry = WinRegistryEntry<bool>.New(TestHive, TestPath, "IsTrueDword").Read();
            Assert.That(entry.Value, Is.True);
        }

        [Test]
        public void DWordFalse_SuccessfulToBool_ReturnsFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<bool>.New(TestHive, TestPath, "IsFalseDword", false).SetValueKind(RegistryValueKind.DWord).Write());
            var entry = WinRegistryEntry<bool>.New(TestHive, TestPath, "IsFalseDword").SetDefaultValue(true).Read();
            Assert.That(entry.Value, Is.False);
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsBoolFalse()
        {
            Assert.DoesNotThrow(() => WinRegistryEntry<bool>.New(TestHive, TestPath, "FluentReadAndWriteTest", false).SetValueKind(RegistryValueKind.DWord).Write());
            var entry = WinRegistryEntry<bool>.New(TestHive, TestPath, "FluentReadAndWriteTest", true).Read();
            Assert.That(entry.Value, Is.False);
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
