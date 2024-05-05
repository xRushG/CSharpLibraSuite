using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest
{
    [SupportedOSPlatform("windows")]
    internal class WinRegistryECTests : WinRegistryEC
    {
        private const RegistryHive TestHive = RegistryHive.CurrentUser;
        private const string TestPath = @"Software\CSharpLibraSuite";

        public enum WinRegistryPlusEnum
        {
            None = 0,
            Huey = 1,
            Dewey = 2,
            Louie = 3,
            Donald = 4,
            Della = 5
        }

        private const string BoolAsString_True = "BoolAsString_True";
        private const string BoolAsDWord_False = "BoolAsDWord_False";

        private const string Integer_42 = "Integer_42";
        private const string IntegerEnum_3 = "IntegerEnum_3";

        private const string Long_2247483647 = "Long_2247483647";
        private const string Long_3 = "Long_3";

        private const string String_Apple = "String_Apple";
        private const string String_Dog = "String_Dog";
        private const string StringEnum_Donald = "StringEnum_Donald";

        private readonly string[] Fruits = { "Banana", "Strawberry", "Apple" };
        private readonly int[] PrimeNumbers = { 2, 3, 5, 7, 11 };
        private readonly long[] LongNumbers = { 2200000000, 2200000001, 2200000002, 2200000003, 2200000004 , 2247483647 };

        [SetUp]
        public void Setup()
        {
            SetValue(TestHive, TestPath, BoolAsString_True, "true", RegistryValueKind.String);
            SetValue(TestHive, TestPath, BoolAsDWord_False, 0, RegistryValueKind.DWord);

            SetValue(TestHive, TestPath, Integer_42, "42", RegistryValueKind.DWord);
            SetValue(TestHive, TestPath, IntegerEnum_3, "3", RegistryValueKind.DWord);

            SetValue(TestHive, TestPath, Long_2247483647, "2247483647", RegistryValueKind.QWord);
            SetValue(TestHive, TestPath, Long_3, "3", RegistryValueKind.QWord);

            SetValue(TestHive, TestPath, String_Apple, "Apple", RegistryValueKind.String);
            SetValue(TestHive, TestPath, String_Dog, "Dog", RegistryValueKind.String);
            SetValue(TestHive, TestPath, StringEnum_Donald, "Donald", RegistryValueKind.String);
        }

        #region WinRegistryEC.GetRegistryEntry()

        [Test]
        public void GetRegistryEntry_ReturnsCorrectEntry()
        {
            const string testPath = TestPath + @"\GetRegistryEntry";
            const string testName = "TestEntry";
            const int testValue = 2011;
            SetValue(TestHive, testPath, testName, testValue, RegistryValueKind.DWord);

            var entry = GetEntry(TestHive, testPath, testName);

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

        #region WinRegistryEC.GetRegistryEntries() 

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

            var entries = GetEntries(TestHive, testPath);


            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries, Is.InstanceOf<List<Entry>>());

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

        #region WinRegistryEC.GetRegistryEntriesRecursive()
        
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

            var entries = GetEntriesRecursive(TestHive, testPath);

            Assert.That(entries, Is.Not.Null);
            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries, Is.InstanceOf<List<Entry>>());

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

        #region WinRegistryEC.GetBoolean()

        [Test]
        public void GetBooleanFromString_ReturnsTrue()
        {
            var key = GetBoolean(TestHive, TestPath, BoolAsString_True, false);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(true));
                Assert.That(key.IsSet, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetBooleanFromDword_ReturnsFalse()
        {
            var key = GetBoolean(TestHive, TestPath, BoolAsDWord_False, true);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(false));
                Assert.That(key.IsSet, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetBooleanNotSet_ReturnsDefaultTrue()
        {
            var key = GetBoolean(TestHive, TestPath, "TestBoolNotSet", true);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(true));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        #endregion

        #region WinRegistryEC.GetInteger()

        [Test]
        public void GetInteger_returnsValue()
        {
            var key = GetInteger(TestHive, TestPath, Integer_42);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetInteger_DefaultValue_ReturnsDefault()
        {
            var key = GetInteger(TestHive, TestPath, "TestIntegerNotProvided");
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(0));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetInteger_SpecifiedDefaultValue_ReturnsSpecifiedDefault()
        {
            var key = GetInteger(TestHive, TestPath, "TestIntegerNotProvided", 2096);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(2096));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetInteger_Enum_ReturnsValue()
        {
            var key = GetInteger<WinRegistryPlusEnum>(TestHive, TestPath, IntegerEnum_3);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(3));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetInteger_EnumInvalid_ReturnsValue()
        {
            var key = GetInteger<WinRegistryPlusEnum>(TestHive, TestPath, Integer_42);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetInteger_ValidatedArray_Valid()
        {
            var key = GetInteger(TestHive, TestPath, IntegerEnum_3, PrimeNumbers);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(3));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetInteger_ValidatedArray_InValid()
        {
            var key = GetInteger(TestHive, TestPath, Integer_42, PrimeNumbers);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetInteger_ValidatedRange_Valid()
        {
            var key = GetInteger(TestHive, TestPath, Integer_42, 0, 50);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetInteger_ValidatedRange_InValid()
        {
            var key = GetInteger(TestHive, TestPath, Integer_42, 0, 20);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        #endregion

        #region WinRegistryEC.GetLongInt()

        [Test]
        public void GetLongInt_returnsValue()
        {
            var key = GetLongInt(TestHive, TestPath, Long_2247483647);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(2247483647));
                Assert.That(key.IsSet, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetLongInt_DefaultValue_ReturnsDefault()
        {
            var key = GetLongInt(TestHive, TestPath, "TestIntegerNotProvided");
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(0));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetLongInt_SpecifiedDefaultValue_ReturnsSpecifiedDefault()
        {
            var key = GetLongInt(TestHive, TestPath, "TestIntegerNotProvided", 4247483647);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(4247483647));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetLongInt_ValidatedArray_Valid()
        {
            var key = GetLongInt(TestHive, TestPath, Long_2247483647, LongNumbers);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(2247483647));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetLongInt_ValidatedArray_InValid()
        {
            var key = GetLongInt(TestHive, TestPath, Long_3, LongNumbers);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(3));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetLongInt_ValidatedRange_Valid()
        {
            var key = GetLongInt(TestHive, TestPath, Long_3, 0, 50);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(3));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetLongInt_ValidatedRange_InValid()
        {
            var key = GetLongInt(TestHive, TestPath, Long_3, 10, 20);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(3));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        #endregion

        #region WinRegistryEC.GetString()

        [Test]
        public void GetString_returnsValue()
        {
            var key = GetString(TestHive, TestPath, String_Apple);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Apple"));
                Assert.That(key.IsSet, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetString_NotProvided_ReturnsDefault()
        {
            var key = GetString(TestHive, TestPath, "TestStringNotProvided", "Banana");
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Banana"));
                Assert.That(key.IsSet, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetString_Validated_Valid()
        {
            var key = GetString(TestHive, TestPath, String_Apple, Fruits);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Apple"));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetString_Validated_NotValid()
        {
            var key = GetString(TestHive, TestPath, String_Dog, Fruits);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Dog"));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }

        [Test]
        public void GetString_Validated_NotValid_returnDefault()
        {
            var key = GetString(TestHive, TestPath, String_Dog, Fruits, "Banana");

            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Dog"));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
                Assert.That(key.DefaultValue, Is.EqualTo("Banana"));
            });
        }

        [Test]
        public void GetString_Enum_ReturnsValue()
        {
            var key = GetString<WinRegistryPlusEnum>(TestHive, TestPath, StringEnum_Donald);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Donald"));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(true));
            });
        }

        [Test]
        public void GetString_EnumInvalid_ReturnsDog()
        {
            var key = GetString<WinRegistryPlusEnum>(TestHive, TestPath, String_Dog);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("Dog"));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
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
