using System;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest
{
    [SupportedOSPlatform("windows")]
    internal class WinRegistryTests : WinRegistry
    {
        private const string TestRoot = GlobalConstants.WinRegTestsRootPath;
        private const RegistryHive TestHive = GlobalConstants.WinRegTestsRootHive;
        private const string TestPath = $"{TestRoot}\\WinRegistryTests";

        [SetUp]
        public void Setup()
        {
        }

        #region WinRegistry.ThrowIfHiveInvalid()

        [Test]
        public void ThrowIfHiveInvalid_ValidHive_DoesNotThrow()
        {
            Assert.That(() => ThrowIfHiveInvalid(RegistryHive.LocalMachine), Throws.Nothing);
        }

        [Test]
        public void ThrowIfHiveInvalid_CurrentConfig_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfHiveInvalid(RegistryHive.CurrentConfig), Throws.ArgumentException);
        }

        [Test]
        public void ThrowIfHiveInvalid_InvalidHive_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfHiveInvalid((RegistryHive)100), Throws.ArgumentException);
        }

        #endregion

        #region WinRegistry.ThrowIfPathInvalid()

        [Test]
        public void ThrowIfPathInvalid_ValidPath_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ThrowIfPathInvalid(@"SOFTWARE\Microsoft"));
        }

        [Test]
        public void ThrowIfPathInvalid_NullPath_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfPathInvalid(null), Throws.ArgumentException);
        }

        [Test]
        public void ThrowIfPathInvalid_EmptyPath_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfPathInvalid(string.Empty), Throws.ArgumentException);
        }

        [Test]
        public void ThrowIfPathInvalid_WhitespacePath_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfPathInvalid("   "), Throws.ArgumentException);
        }

        #endregion

        #region WinRegistry.ThrowIfNameInvalid()

        [Test]
        public void ThrowIfNameInvalid_ValidName_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ThrowIfNameInvalid("TestName"));
        }

        [Test]
        public void ThrowIfNameInvalid_NullName_ThrowsArgumentNullException()
        {
            Assert.That(() => ThrowIfNameInvalid(null), Throws.ArgumentNullException);
        }

        #endregion

        #region WinRegistry.ThrowIfValueKindInvalid()

        [Test] 
        public void ThrowIfValueKindInvalid_ValidValueKind_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ThrowIfValueKindInvalid(RegistryValueKind.String));
        }

        [Test]
        public void ThrowIfValueKindInvalid_InvalidValueKind_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfValueKindInvalid((RegistryValueKind)100), Throws.ArgumentException);
        }

        [Test]
        public void ThrowIfValueKindInvalid_ValueKindUnknown_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfValueKindInvalid(RegistryValueKind.Unknown), Throws.ArgumentException);
        }

        [Test]
        public void ThrowIfValueKindInvalid_ValueKindNone_ThrowsArgumentException()
        {
            Assert.That(() => ThrowIfValueKindInvalid(RegistryValueKind.None), Throws.ArgumentException);
        }

        #endregion

        #region WinRegistry.SetValue()

        [Test]
        public void SetValue_NewKey_SetsValueSuccessfully()
        {
            const string testName = "TestValue";
            const string testValue = "Test1";
            const string testPath = TestPath + @"\SetValue";
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));
 
            string key = GetStringValue(TestHive, testPath, testName, testValue);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void SetValue_OverwriteValue_SetsValueSuccessfully()
        {
            const string testName = "TestValue";
            const string testValue = "Test2";
            const string testPath = TestPath + @"\SetValue";
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            string key = GetStringValue(TestHive, testPath, testName, testValue);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void SetValue_DefaultValueWithWrongValueKind_SetsValueSuccessfully()
        {
            const string? testName = null;
            const string testValue = "SetDefault";
            const string testPath = TestPath + @"\SetValue";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.Unknown));

            string key = GetStringValue(TestHive, testPath, testName, testValue);
            Assert.That(key, Is.EqualTo(testValue));
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

        #endregion

        #region WinRegistry.GetValue()

        [Test]
        public void GetValue_RetrieveValue_ReturnsValue()
        {
            const string testPath = TestPath + @"\GetValue";
            const string testName = "TestValue";
            const string testValue = "Test";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            string key = GetValue(TestHive, testPath, testName);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void GetValue_RetrieveDefault_RetrunsValue()
        {
            const string testPath = TestPath + @"\GetValue";
            const string testName = "";
            const string testValue = "TestDefault{098f2470-bae0-11cd-b579-08002b30bfeb}";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            var key = GetValue(TestHive, testPath, testName);
            Assert.That(key, Is.EqualTo(testValue));
        }

        [Test]
        public void GeValue_NonExistingValue_ReturnsNull()
        {
            const string testPath = TestPath + @"\GetValue";
            const string nonExistingName = "NonExistingValue";

            string key = GetValue(TestHive, testPath, nonExistingName);
            Assert.That(key, Is.Null);
        }

        #endregion

        #region WinRegistry.GetStringValue()

        [Test]
        public void GetStringValue_RetrieveValue_ReturnsValue()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = "TestValue";
            const string testValue = "Test";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            var key = GetStringValue(TestHive, testPath, testName);
            Assert.That(key, Is.TypeOf<string>().And.EqualTo(testValue));
        }

        [Test]
        public void GetStringValue_RetrieveDefault_ReturnsValue()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = "";
            const string testValue = "TestDefault{098f2470-bae0-11cd-b579-08002b30bfeb}";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            var key = GetStringValue(TestHive, testPath, testName);
            Assert.That(key, Is.TypeOf<string>().And.EqualTo(testValue));
        }

        [Test]
        public void GetStringValue_NonExistingValue_ReturnsNull()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";

            var key = GetStringValue(TestHive, testPath, nonExistingName);
            Assert.That(key, Is.Null);
        }

        [Test]
        public void GetStringValue_NonExistingValue_ReturnsDefault()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";
            const string defaultValue = "DefaultValue";

            string key = GetStringValue(TestHive, testPath, nonExistingName, defaultValue);
            Assert.That(key, Is.TypeOf<string>().And.EqualTo(defaultValue));
        }

        #endregion

        #region WinRegistry.GetBoolValue()

        [Test]
        public void GetBoolValue_FromString_ReturnsTrue()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "strBool_true";
            const string testValue = "true";
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            var key = GetBoolValue(TestHive, testPath, testName, false); // set default to false
            Assert.That(key, Is.TypeOf<bool>().And.EqualTo(true));
        }

        [Test]
        public void GetBoolValue_FromString_ReturnsFalse()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "strBool_false";
            const string testValue = "false";
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.String));

            var key = GetBoolValue(TestHive, testPath, testName, true); // set default to true
            Assert.That(key, Is.TypeOf<bool>().And.EqualTo(false));
        }

        [Test]
        public void GetBoolValue_FromDword_ReturnsFalse()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "intBool_false";
            const int testValue = 0;
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord));

            var key = GetBoolValue(TestHive, testPath, testName, true); // set default to true
            Assert.That(key, Is.TypeOf<bool>().And.EqualTo(false));
        }

        [Test]
        public void GetBoolValue_FromDword_ReturnsTrue()
        {
            const string testPath = TestPath + @"\GetBoolValue";
            const string testName = "intBool_true";
            const int testValue = 1;
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord));

            var key = GetBoolValue(TestHive, testPath, testName, false); // set default to false
            Assert.That(key, Is.TypeOf<bool>().And.EqualTo(true));
        }

        [Test]
        public void GetBoolValue_NonExistingValue_ReturnsFalse()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";

            var key = GetBoolValue(TestHive, testPath, nonExistingName);
            Assert.That(key, Is.TypeOf<bool>().And.EqualTo(false));
        }

        #endregion

        #region WinRegistry.GetDwordValue()

        [Test]
        public void GetIntegerValue_RetrieveValue_ReturnsValue()
        {
            const string testPath = TestPath + @"\GetIntegerValue";
            const string testName = "ExistingDword";
            const int testValue = 2;
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord));

            var key = GetIntegerValue(TestHive, testPath, testName);
            Assert.That(key, Is.TypeOf<int>().And.EqualTo(testValue));
        }

        [Test]
        public void GetIntegerValue_NonExistingValue_ReturnsZero()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string nonExistingName = "NonExistingValue";

            var key = GetIntegerValue(TestHive, testPath, nonExistingName);
            Assert.That(key, Is.TypeOf<int>().And.EqualTo(0));
        }

        [Test]
        public void GetIntegerValue_NotExistingKey_ReturnsDefault()
        {
            const string testPath = TestPath + @"\GetStringValue";
            const string testName = "NotExistingDword";

            var key = GetIntegerValue(TestHive, testPath, testName, 12); // set default to true
            Assert.That(key, Is.TypeOf<int>().And.EqualTo(12));
        }

        #endregion

        #region WinRegistry.DeleteRegistryValue()
        [Test]
        public void DeleteRegistryValue_DeletesValue()
        {
            const string testPath = TestPath + @"\DeleteRegistryValue";
            const string testName01 = "TestValue01";
            const string testName02 = "TestValue02";

            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName01, "Test", RegistryValueKind.String));
            Assert.DoesNotThrow(() => SetValue(TestHive, testPath, testName02, "Test", RegistryValueKind.String));

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
        public void DeleteTree_RemovesKeyAndSubkeys()
        {
            const string testPath = TestPath + @"\DeleteTree";
            Assert.DoesNotThrow(() => SetValue(TestHive, $"{testPath}\\Subkey0", "Test", "Test", RegistryValueKind.String));
            Assert.DoesNotThrow(() => SetValue(TestHive, $"{testPath}\\Subkey1", "Test", "Test", RegistryValueKind.String));
            Assert.DoesNotThrow(() => SetValue(TestHive, $"{testPath}\\Subkey2", "Test", "Test", RegistryValueKind.String));
            Assert.DoesNotThrow(() => SetValue(TestHive, $"{testPath}\\Subkey3", "Test", "Test", RegistryValueKind.String));

            Assert.DoesNotThrow(() => DeleteTree(TestHive, testPath));
            Assert.That(GetSubKeyNames(TestHive, testPath), Is.Empty);
        }

        #endregion

        #region WinRegistry.ConvertStringToRegistryHive()

        [Test]
        public void ConvertStringToRegistryHive_ReturnsCorrectValue()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ConvertStringToRegistryHive("HKCR"), Is.EqualTo(RegistryHive.ClassesRoot));
                Assert.That(ConvertStringToRegistryHive("HKey_Classes_Root"), Is.EqualTo(RegistryHive.ClassesRoot));
                Assert.That(ConvertStringToRegistryHive("ClassesRoot"), Is.EqualTo(RegistryHive.ClassesRoot));

                Assert.That(ConvertStringToRegistryHive("HKCU"), Is.EqualTo(RegistryHive.CurrentUser));
                Assert.That(ConvertStringToRegistryHive("HKey_Current_User"), Is.EqualTo(RegistryHive.CurrentUser));
                Assert.That(ConvertStringToRegistryHive("currentuser"), Is.EqualTo(RegistryHive.CurrentUser));

                Assert.That(ConvertStringToRegistryHive("HKLM"), Is.EqualTo(RegistryHive.LocalMachine));
                Assert.That(ConvertStringToRegistryHive("HKey_Local_Machine"), Is.EqualTo(RegistryHive.LocalMachine));
                Assert.That(ConvertStringToRegistryHive("LocalMachine"), Is.EqualTo(RegistryHive.LocalMachine));

                Assert.That(ConvertStringToRegistryHive("HKU"), Is.EqualTo(RegistryHive.Users));
                Assert.That(ConvertStringToRegistryHive("HKey_users"), Is.EqualTo(RegistryHive.Users));
                Assert.That(ConvertStringToRegistryHive("Users"), Is.EqualTo(RegistryHive.Users));

                Assert.That(ConvertStringToRegistryHive("HKCC"), Is.EqualTo(RegistryHive.CurrentConfig));
                Assert.That(ConvertStringToRegistryHive("HKey_Current_Config"), Is.EqualTo(RegistryHive.CurrentConfig));
                Assert.That(ConvertStringToRegistryHive("CurrentConfig"), Is.EqualTo(RegistryHive.CurrentConfig));
            });
        }

        [Test]
        public void ConvertStringToRegistryHive_InvalidHiveNull_ThrowArgumentNullException()
        {
            Assert.That(() => ConvertStringToRegistryHive(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertStringToRegistryHive_InvalidHive_ThrowArgumentException()
        {
            Assert.That(() => ConvertStringToRegistryHive("InvalidHive"), Throws.ArgumentException);
        }

        #endregion

        #region WinRegistry.ConvertStringToRegistryValueKind()

        [Test]
        public void ConvertStringToRegistryValueKind_ReturnsCorrectValue()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ConvertStringToRegistryValueKind("string"), Is.EqualTo(RegistryValueKind.String));
                Assert.That(ConvertStringToRegistryValueKind("reg_sz"), Is.EqualTo(RegistryValueKind.String));

                Assert.That(ConvertStringToRegistryValueKind("dword"), Is.EqualTo(RegistryValueKind.DWord));
                Assert.That(ConvertStringToRegistryValueKind("reg_dword"), Is.EqualTo(RegistryValueKind.DWord));

                Assert.That(ConvertStringToRegistryValueKind("binary"), Is.EqualTo(RegistryValueKind.Binary));
                Assert.That(ConvertStringToRegistryValueKind("reg_binary"), Is.EqualTo(RegistryValueKind.Binary));

                Assert.That(ConvertStringToRegistryValueKind("qword"), Is.EqualTo(RegistryValueKind.QWord));
                Assert.That(ConvertStringToRegistryValueKind("reg_qword"), Is.EqualTo(RegistryValueKind.QWord));

                Assert.That(ConvertStringToRegistryValueKind("multistring"), Is.EqualTo(RegistryValueKind.MultiString));
                Assert.That(ConvertStringToRegistryValueKind("reg_multi_sz"), Is.EqualTo(RegistryValueKind.MultiString));

                Assert.That(ConvertStringToRegistryValueKind("expandstring"), Is.EqualTo(RegistryValueKind.ExpandString));
                Assert.That(ConvertStringToRegistryValueKind("reg_expand_sz"), Is.EqualTo(RegistryValueKind.ExpandString));
            });
        }

        [Test]
        public void ConvertStringToRegistryValueKind_InvalidHiveNull_ThrowArgumentNullException()
        {
            Assert.That(() => ConvertStringToRegistryValueKind(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ConvertStringToRegistryValueKind_InvalidHive_ThrowArgumentException()
        {
            Assert.That(() => ConvertStringToRegistryValueKind("InvalidKind"), Throws.ArgumentException);
        }

        #endregion

        #region WinRegistry.ConvertTypeToRegistryValueKind()

        [Test]
        public void ConvertTypeToRegistryValueKind_ReturnsCorrectValue()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ConvertTypeToRegistryValueKind(typeof(string)), Is.EqualTo(RegistryValueKind.String));
                Assert.That(ConvertTypeToRegistryValueKind(typeof(int)), Is.EqualTo(RegistryValueKind.DWord));
                Assert.That(ConvertTypeToRegistryValueKind(typeof(long)), Is.EqualTo(RegistryValueKind.QWord));
                Assert.That(ConvertTypeToRegistryValueKind(typeof(bool)), Is.EqualTo(RegistryValueKind.DWord));
                Assert.That(ConvertTypeToRegistryValueKind(typeof(byte)), Is.EqualTo(RegistryValueKind.Binary));
            });
        }

        [Test]
        public void ConvertTypeToRegistryValueKind_UnknownType_ReturnsString()
        {
            Assert.That(ConvertTypeToRegistryValueKind(typeof(Single)), Is.EqualTo(RegistryValueKind.String));
        }

        [Test]
        public void ConvertTypeToRegistryValueKind_WithNullValueType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConvertTypeToRegistryValueKind(null));
        }

        #endregion

        #region WinRegistry.ConvertRegistryValueKindToType()

        [Test]
        public void ConvertRegistryValueKindToType_ReturnsCorrectType()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.String), Is.EqualTo(typeof(string)));
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.ExpandString), Is.EqualTo(typeof(string)));
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.DWord), Is.EqualTo(typeof(int)));
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.QWord), Is.EqualTo(typeof(long)));
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.Binary), Is.EqualTo(typeof(byte[])));
                Assert.That(ConvertRegistryValueKindToType(RegistryValueKind.MultiString), Is.EqualTo(typeof(string[])));
                Assert.That(ConvertRegistryValueKindToType((RegistryValueKind)100), Is.EqualTo(typeof(object)));
            });
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
