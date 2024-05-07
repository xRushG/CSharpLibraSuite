
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class BoolEntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\BoolEntryTest";

        private Entry TestEntryString = new()
        {
            Hive = RegistryHive.CurrentUser,
            Path = @"SOFTWARE\Microsoft",
            Name = "Windows",
            ValueKind = RegistryValueKind.String,
            Value = "true"
        };

        private Entry TestEntryDWord = new()
        {
            Hive = RegistryHive.CurrentUser,
            Path = @"SOFTWARE\Microsoft",
            Name = "Windows",
            ValueKind = RegistryValueKind.DWord,
            Value = "0"
        };

        [Test]
        public void StringTrue_SuccessfulToBool_ReturnsTrue()
        {
            TestEntryString.Value = "True";
            var entry = new BoolEntry(TestEntryString);
            Assert.That(entry.Value, Is.True);
        }

        [Test]
        public void StringFalse_SuccessfulToBool_ReturnsFalse()
        {
            TestEntryString.Value = "False";
            var entry = new BoolEntry(TestEntryString);
            Assert.That(entry.Value, Is.False);
        }

        [Test]
        public void DWordTrue_SuccessfulToBool_ReturnsTrue()
        {
            TestEntryDWord.Value = "1";
            var entry = new BoolEntry(TestEntryDWord);
            Assert.That(entry.Value, Is.True);
        }

        [Test]
        public void DWordFalse_SuccessfulToBool_ReturnsFalse()
        {
            TestEntryDWord.Value = "0";
            var entry = new BoolEntry(TestEntryDWord);
            Assert.That(entry.Value, Is.False);
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsBoolFalse()
        {
            string name = "FluentReadAndWriteTest";

            Assert.DoesNotThrow(() => BoolEntry.New(TestHive, TestPath, name, 0, RegistryValueKind.DWord, true).Write());

            BoolEntry entry = BoolEntry.New(TestHive, TestPath, name, true).Read();
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
