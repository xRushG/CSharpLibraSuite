
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class BoolEntryTest
    {

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
    }
}
