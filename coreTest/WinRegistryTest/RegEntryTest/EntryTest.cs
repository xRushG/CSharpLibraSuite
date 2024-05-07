
using Microsoft.Win32;
using NUnit.Framework.Internal;
using System;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest.RegEntryTest
{
    [SupportedOSPlatform("windows")]
    internal class EntryTest
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\EntryTests";

        [Test]
        public void Path_SetValidValue_GetReturnsSameValue()
        {
            string expectedPath = @"SOFTWARE\Microsoft";
            var entry = new Entry
            {
                Path = expectedPath
            };
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
            string expectedName = "Version";
            var entry = new Entry
            {
                Name = expectedName
            };
            Assert.That(expectedName, Is.EqualTo(entry.Name));
        }

        [Test]
        public void Value_SetValidValue_GetReturnsSameValue()
        {
            string expectedValue = "1.0";
            var entry = new Entry
            {
                Value = expectedValue
            };
            Assert.That(expectedValue, Is.EqualTo(entry.Value));
        }

        [Test]
        public void ValueKind_SetInvalidValue_ThrowsArgumentException()
        {
            var entry = new Entry();
            Assert.That(() => entry.ValueKind = (RegistryValueKind)100, Throws.ArgumentException);
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

        [Test]
        public void Read_WhenRegistryKeyIsNull_ThrowsInvalidOperationException()
        {
            var entry = new Entry();
            Assert.That(() => entry.Read(), Throws.InvalidOperationException);
        }

        [Test]
        public void Write_WhenKeyIsUnwritable_ThrowsInvalidOperationException()
        {
            var entry = new Entry
            {
                Hive = RegistryHive.CurrentUser,
                Path = @"SOFTWARE\Microsoft",
                Name = "Version"
            };
            Assert.That(() => entry.Write(), Throws.InvalidOperationException);
        }

        [Test]
        public void WriteDefaultAndReadDefault_Entry_DoesNotThrowAndReadsCorrectly()
        {
            var entry = new Entry
            {
                Hive = TestHive,
                Path = TestPath,
                Value = "TestWrite",
                ValueKind = RegistryValueKind.DWord,
            };

            var readEntry = new Entry
            {
                Hive = TestHive,
                Path = TestPath,
            };

            Assert.That(() => entry.Write(), Throws.Nothing);
            Assert.That(() => readEntry.Read(), Throws.Nothing);
            Assert.Multiple(() =>
            {
                Assert.That(readEntry.ValueKind, Is.EqualTo(RegistryValueKind.String));
                Assert.That(readEntry.Value, Is.EqualTo(entry.Value));
            });
        }

        [Test]
        public void WriteAndRead_Entry_DoesNotThrowAndReadsCorrectly()
        {
            var entry = new Entry
            {
                Hive = TestHive,
                Path = TestPath,
                Name = "TestRead",
                ValueKind = RegistryValueKind.QWord,
                Value = "200"
            };

            var readEntry = new Entry
            {
                Hive = TestHive,
                Path = TestPath,
                Name = "TestRead"
            };

            Assert.That(() => entry.Write(), Throws.Nothing);
            Assert.That(() => readEntry.Read(), Throws.Nothing);
            Assert.Multiple(() =>
            {
                Assert.That(readEntry.ValueKind, Is.EqualTo(entry.ValueKind));
                Assert.That(readEntry.Value, Is.EqualTo(entry.Value));
            });
        }

        [Test]
        public void FluentWrite_And_Read_DoesNotThrow_ReturnsStrJustATest()
        {
            string name = "FluentReadAndWriteTest";

            Assert.DoesNotThrow(() => Entry.New(TestHive, TestPath, name, "JustATest", RegistryValueKind.String).Write());

            Entry entry = Entry.New(TestHive, TestPath, name).Read();
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
