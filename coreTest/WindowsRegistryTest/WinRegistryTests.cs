using System;
using core.WindowsRegistry.entry;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Linq;

namespace CoreTest.WindowsRegistryTest
{
    public class WinRegistryTests : WinRegistry
    {
        private const RegistryHive TestHive = RegistryHive.CurrentUser;
        private const string TestPath = @"Software\CSharpLibraSuite";

        [SetUp]
        public void Setup()
        {
        }

        #region WinRegistry.SetValue()
        [Test]
        public void SetValue_NewKey_SetsValueSuccessfully()
        {
            const string testName = "TestValue";
            const string testValue = "Test1";
            SetValue(TestHive, TestPath, testName, testValue, RegistryValueKind.String);

            string key = GetStringValue(TestHive, TestPath, testName, testValue);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void SetValue_ExistingKey_SetsValueSuccessfully()
        {
            const string testName = "TestValue";
            const string testValue = "Test2";
            SetValue(TestHive, TestPath, testName, testValue, RegistryValueKind.String);

            string key = GetStringValue(TestHive, TestPath, testName, testValue);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void SetValue_InvalidHive_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => SetValue((RegistryHive)(-1), TestPath, "TestValue", "Test", RegistryValueKind.String));
        }

        [Test]
        public void SetValue_InvalidPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => SetValue(TestHive, "", "TestValue", "Test", RegistryValueKind.String));
        }

        [Test]
        public void SetValue_InvalidName_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SetValue(TestHive, TestPath, null, "Test", RegistryValueKind.String));
        }
        #endregion

        #region WinRegistry.CreateKey()
        [Test]
        public void CreateKey_NewKey_CreatesKeySuccessfully()
        {
            const string testPath = TestPath + @"\TestSubKey";
            Assert.DoesNotThrow(() => CreateKey(TestHive, testPath));
        }

        [Test]
        public void CreateKey_ExistingKey_DoesNotThrow()
        {
            const string testPath = TestPath + @"\TestSubKey";
            Assert.DoesNotThrow(() => CreateKey(TestHive, testPath));
        }

        [Test]
        public void CreateKey_InvalidHive_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => CreateKey((RegistryHive)(-1), @"Software\Test"));
        }

        [Test]
        public void CreateKey_InvalidPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentException>(() => CreateKey(TestHive, null));
        }
        #endregion

        #region WinRegistry.GetSubKeyNames()
        [Test]
        public void GetSubKeyNames_ExistingKey_ReturnsSubKeyNames()
        {
            const string testPath = TestPath + @"\GetSubKeyNames";
            const string testSubKey1 = testPath + @"\subkey1";
            const string testSubKey2 = testPath + @"\subkey2";
            const string testSubKey3 = testPath + @"\subkey3";

            CreateKey(TestHive, testSubKey1);
            CreateKey(TestHive, testSubKey2);
            CreateKey(TestHive, testSubKey3);

            string[] subKeyNames = GetSubKeyNames(TestHive, testPath);

            Assert.That(subKeyNames.Length, Is.EqualTo(3));
            Assert.That(subKeyNames, Contains.Item("subkey1"));
            Assert.That(subKeyNames, Contains.Item("subkey2"));
            Assert.That(subKeyNames, Contains.Item("subkey3"));
        }
        
        [Test]
        public void GetSubKeyNames_NonExistingKey_ReturnsEmptyArray()
        {
            string[] subKeyNames = GetSubKeyNames(TestHive, @"Software\NonExistingKey");
            Assert.That(subKeyNames, Is.Empty);
        }
        
        [Test]
        public void GetSubKeyNames_InvalidHive_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GetSubKeyNames((RegistryHive)(-1), TestPath));
        }
        
        [Test]
        public void GetSubKeyNames_InvalidPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GetSubKeyNames(TestHive, ""));
        }
        #endregion

        #region WinRegistry.GetStringValue()
        [Test]
        public void GetStringValue_ExistingValue_ReturnsValue()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = "TestValue";
            const string testValue = "Test";
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);
            string key = GetStringValue(TestHive, testPath, testName);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void GetStringValue_CanRetrieveDefaultPropertyValue()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = ""; // to get or set default value
            const string testValue = "TestDefault{098f2470-bae0-11cd-b579-08002b30bfeb}";
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);

            var key = GetStringValue(TestHive, testPath, testName);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void GetStringValue_NonExistingValue_ReturnsNull()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";
            string key = GetStringValue(TestHive, testPath, nonExistingName);
            Assert.That(key, Is.Null);
        }

        [Test]
        public void GetStringValue_NonExistingValue_ReturnsDefault()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";
            const string defaultValue = "DefaultValue";
            string key = GetStringValue(TestHive, testPath, nonExistingName, defaultValue);
            Assert.That(key, Is.EqualTo(defaultValue));
        }
        [Test]
        public void GetValue_NullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentException>(() => GetStringValue(RegistryHive.CurrentUser, null, "TestValue"));
        }

        [Test]
        public void GetValue_InvalidName_ThrowsArgumentNullExceptionn()
        {
            Assert.Throws<ArgumentNullException>(() => GetStringValue(RegistryHive.CurrentUser, TestPath, null));
        }

        [Test]
        public void GetValue_InvalidHive_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => GetStringValue((RegistryHive)(-1), TestPath, "TestValue"));
        }
        #endregion

        #region WinRegistry.GetBoolValue() - Basically the same as GetStringValue()
        [Test]
        public void GetBoolValue_FromString_ReturnsTrue()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "strBool_true";
            const string testValue = "true";
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);

            var key = GetBoolValue(TestHive, testPath, testName, false); // set default to false
            Assert.That(key, Is.EqualTo(true));
        }

        [Test]
        public void GetBoolValue_FromString_ReturnsFalse()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "strBool_false";
            const string testValue = "false";
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);

            var key = GetBoolValue(TestHive, testPath, testName, true); // set default to true
            Assert.That(key, Is.EqualTo(false));
        }

        [Test]
        public void GetBoolValue_FromDword_ReturnsFalse()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "intBool_false";
            const int testValue = 0;
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord);

            var key = GetBoolValue(TestHive, testPath, testName, true); // set default to true
            Assert.That(key, Is.EqualTo(false));
        }

        [Test]
        public void GetBoolValue_FromDword_ReturnsTrue()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "intBool_true";
            const int testValue = 1;
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord);

            var key = GetBoolValue(TestHive, testPath, testName, false); // set default to false
            Assert.That(key, Is.EqualTo(true));
        }
        #endregion

        #region WinRegistry.GetDwordValue() - Basically the same as GetStringValue()
        [Test]
        public void GetDwordValue_ExistingKey_ReturnsInteger()
        {
            const string testPath = TestPath + @"\GetIntegerValue";
            const string testName = "ExistingDword";
            const int testValue = 2;
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord);

            var key = GetDwordValue(TestHive, testPath, testName); // set default to false
            Assert.That(key, Is.EqualTo(2));
        }

        [Test]
        public void GetDwordValue_NotExistingKey_ReturnsDefault()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = "NotExistingDword";

            var key = GetDwordValue(TestHive, testPath, testName, 12); // set default to true
            Assert.That(key, Is.EqualTo(12));
        }
        #endregion

        #region WinRegistry.GetRegistryEntry()
        [Test]
        public void GetRegistryEntry_ReturnsCorrectEntry()
        {
            const string testPath = TestPath + @"\GetRegistryEntry";
            const string testName = "TestEntry";
            const int testValue = 2011;
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord);

            var entry = GetRegistryEntry(TestHive, testPath, testName);

            Assert.Multiple(() =>
            {
                Assert.That(entry.Hive, Is.EqualTo(TestHive));
                Assert.That(entry.Path, Is.EqualTo(testPath));
                Assert.That(entry.Name, Is.EqualTo(testName));
                Assert.That(entry.ValueKind, Is.EqualTo(RegistryValueKind.DWord));
                Assert.That(entry.IsSet, Is.EqualTo(true));
            });
        }
        #endregion

        #region WinRegistry.GetRegistryEntries() 
        [Test]
        public void GetRegistryEntries_ReturnsCorrectEntries()
        {
            const string testPath = TestPath + @"\GetRegistryEntries";
            const string testNamePrefix = "TestEntry";
            const string testValue = "winRegEntriesTest";

            for (int i = 1; i <= 10; i++)
            {
                string testName = $"{testNamePrefix}{i}";
                SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);
            }

            const string testPathSubkeys = testPath + @"\Subkey";
            const string testNameSubKey = "TestSubEntry";
            SetValue(TestHive, testPathSubkeys, testNameSubKey, testValue, RegistryValueKind.String);
        
            var entries = GetRegistryEntries(TestHive, testPath);


            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries, Is.InstanceOf<List<WinRegistryEntry>>());

            foreach (var entry in entries)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(entry.Name, Is.Not.Null & Is.Not.EqualTo(testNameSubKey)); //Subkeys should not be read (non-recursive).

                    Assert.That(entry.Hive, Is.EqualTo(TestHive));
                    Assert.That(entry.Path, Is.EqualTo(testPath));
                    Assert.That(entry.Value, Is.EqualTo(testValue));
                    Assert.That(entry.ValueKind, Is.EqualTo(RegistryValueKind.String));
                });
            }
        }
        #endregion

        #region WinRegistry.GetRegistryEntriesRecursive() 
        [Test]
        public void GetRegistryEntriesRecursive_ReturnsCorrectEntries()
        {
            const string testPath = TestPath + @"\GetRegistryEntriesRecursive";
            const string testNamePrefix = "TestEntry";
            const string testValue = "winRegEntriesTest";

            for (int i = 1; i <= 10; i++)
            {
                string testName = $"{testNamePrefix}{i}";
                SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String);
            }

            const string testSubkeyPath = testPath + @"\Subkey";
            const string testNameSubKey = "TestSubEntry";
            SetValue(TestHive, testSubkeyPath, testNameSubKey, testValue, RegistryValueKind.String);

            var entries = GetRegistryEntriesRecursive(TestHive, testPath);

            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries, Is.InstanceOf<List<WinRegistryEntry>>());

            // Assert that the subkey is included in the entries list
            Assert.That(entries.Any(e => e.Name == testNameSubKey), Is.True, "Subkey entry should be included in the entries list.");

            foreach (var entry in entries)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(entry.Name, Is.Not.Null);
                    Assert.That(entry.Hive, Is.EqualTo(TestHive));
                    Assert.That(entry.Value, Is.EqualTo(testValue));
                    Assert.That(entry.ValueKind, Is.EqualTo(RegistryValueKind.String));
                });
            }

            
        }
        #endregion

        #region WinRegistry.DeleteRegistryValue()
        [Test]
        public void DeleteRegistryValue_DeletesValue()
        {
            const string testPath = TestPath + @"\DeleteRegistryValue";
            const string testName01 = "TestValue01";
            const string testName02 = "TestValue02";

            SetValue(TestHive, testPath, testName01, "Test", RegistryValueKind.String);
            SetValue(TestHive, testPath, testName02, "Test", RegistryValueKind.String);

            Assert.DoesNotThrow(() => DeleteRegistryValue(TestHive, testPath, testName01));
            Assert.Multiple(() =>
            {
                Assert.That(GetStringValue(TestHive, testPath, testName01), Is.Null);
                Assert.That(GetStringValue(TestHive, testPath, testName02), Is.Not.Null);
            });
        }
        #endregion

        #region WinRegistry.DeleteTree()
        [Test]
        public void DeleteTree_DeletesKeyAndSubkeys()
        {
            const string testPath = TestPath + @"\DeleteTree";

            SetValue(TestHive, $"{testPath}\\Subkey0", "Test", "Test", RegistryValueKind.String);
            SetValue(TestHive, $"{testPath}\\Subkey1", "Test", "Test", RegistryValueKind.String);
            SetValue(TestHive, $"{testPath}\\Subkey2", "Test", "Test", RegistryValueKind.String);
            SetValue(TestHive, $"{testPath}\\Subkey3", "Test", "Test", RegistryValueKind.String);


            Assert.DoesNotThrow(() => DeleteTree(TestHive, testPath));

            string[] subKeys = GetSubKeyNames(TestHive, testPath);
            Assert.That(subKeys, Is.Empty);
        }
        #endregion

        [OneTimeTearDown]
        public void Cleanup()
        {
            // Delete all created tests
            DeleteTree(TestHive, TestPath);
        }
    }
}
