
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class EntryTest
    {
        [Test]
        public void Path_SetValidValue_GetReturnsSameValue()
        {
            var entry = new Entry();
            var expectedPath = @"SOFTWARE\Microsoft";
            entry.Path = expectedPath;
            Assert.That(expectedPath, Is.EqualTo(entry.Path));
        }

        [Test]
        public void Path_SetNullValue_ThrowsArgumentNullException()
        {
            var entry = new Entry();
            Assert.That(() => entry.Path = null, Throws.ArgumentNullException);
        }

        [Test]
        public void Name_SetValidValue_GetReturnsSameValue()
        {
            var entry = new Entry();
            var expectedName = "Version";
            entry.Name = expectedName;
            Assert.That(expectedName, Is.EqualTo(entry.Name));
        }

        [Test]
        public void Name_SetEmptyValue_ThrowsArgumentNullException()
        {
            var entry = new Entry();
            Assert.That(() => entry.Name = "", Throws.ArgumentNullException);
        }

        [Test]
        public void Value_SetValidValue_GetReturnsSameValue()
        {
            var entry = new Entry();
            var expectedValue = "1.0";
            entry.Value = expectedValue;
            Assert.That(expectedValue, Is.EqualTo(entry.Value));
        }


        [Test]
        public void IsSet_ValueHasBeenSet_ReturnsTrue()
        {
            var entry = new Entry
            {
                Value = "Test"
            };
            Assert.That(entry.IsSet, Is.True);
        }

        [Test]
        public void IsKeyReadable_AllPropertiesSet_ReturnsTrue()
        {
            var entry = new Entry
            {
                Hive = RegistryHive.LocalMachine,
                Path = @"SOFTWARE\Microsoft",
                Name = "Version"
            };
            Assert.That(entry.IsKeyReadable, Is.True);
        }

        [Test]
        public void IsKeyWritable_AllPropertiesSet_ReturnsTrue()
        {
            var entry = new Entry
            {
                Hive = RegistryHive.LocalMachine,
                ValueKind = RegistryValueKind.String,
                Path = @"SOFTWARE\Microsoft",
                Name = "Version",
                Value = "1.0"
            };
            Assert.That(entry.IsKeyWritable, Is.True);
        }
    }
}
