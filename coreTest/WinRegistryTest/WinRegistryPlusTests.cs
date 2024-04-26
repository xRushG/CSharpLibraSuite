
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace coreTest.WinRegistryTest
{
    [SupportedOSPlatform("windows")]
    internal class WinRegistryPlusTests : WinRegistryPlus
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

        private const string String_Apple = "String_Apple";
        private const string String_Dog = "String_Dog";
        private const string StringEnum_Donald = "StringEnum_Donald";
        private readonly string[] Fruits = { "Banana", "Strawberry", "Apple" };
        private readonly int[] PrimeNumbers = { 2, 3, 5, 7, 11 };

        [SetUp]
        public void Setup()
        {
            SetValue(TestHive, TestPath, BoolAsString_True, "true", RegistryValueKind.String);
            SetValue(TestHive, TestPath, BoolAsDWord_False, 0, RegistryValueKind.DWord);

            SetValue(TestHive, TestPath, Integer_42, "42", RegistryValueKind.DWord);
            SetValue(TestHive, TestPath, IntegerEnum_3, "3", RegistryValueKind.DWord);

            SetValue(TestHive, TestPath, String_Apple, "Apple", RegistryValueKind.String);
            SetValue(TestHive, TestPath, String_Dog, "Dog", RegistryValueKind.String);
            SetValue(TestHive, TestPath, StringEnum_Donald, "Donald", RegistryValueKind.String);
        }

        #region GetBoolean() Tests
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

        #region GetInteger()
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
                Assert.That(key.Value, Is.EqualTo(-1));
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
        public void GetInteger_Validated_Valid()
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
        public void GetInteger_Validated_InValid()
        {
            var key = GetInteger(TestHive, TestPath, Integer_42, PrimeNumbers);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo(42));
                Assert.That(key.IsSet, Is.EqualTo(true));
                Assert.That(key.IsValid, Is.EqualTo(false));
            });
        }
        #endregion

        #region GetString()
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
        public void GetString_EnumInvalid_ReturnsValue()
        {
            var key = GetString<WinRegistryPlusEnum>(TestHive, TestPath, Integer_42);
            Assert.Multiple(() =>
            {
                Assert.That(key.Value, Is.EqualTo("42"));
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
