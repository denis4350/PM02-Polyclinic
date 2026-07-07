using HospitalSystem.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HospitalSystem.Tests
{
    [TestClass]
    public class PasswordHelperTests
    {
        [TestMethod]
        public void Hash_ThenVerify_SamePassword_ReturnsTrue()
        {
            string hash = PasswordHelper.Hash("MySecret123");

            bool result = PasswordHelper.Verify("MySecret123", hash);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string hash = PasswordHelper.Hash("MySecret123");

            bool result = PasswordHelper.Verify("НеТотПароль", hash);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Verify_LegacyPlainTextPassword_CorrectValue_ReturnsTrue()
        {
            // Так пароли хранились в базе ДО включения хеширования
            string legacyStoredValue = "OldPlainPassword";

            bool result = PasswordHelper.Verify("OldPlainPassword", legacyStoredValue);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Verify_LegacyPlainTextPassword_WrongValue_ReturnsFalse()
        {
            string legacyStoredValue = "OldPlainPassword";

            bool result = PasswordHelper.Verify("НеверныйПароль", legacyStoredValue);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsBcryptHash_RealHash_ReturnsTrue()
        {
            string hash = PasswordHelper.Hash("что-угодно");

            Assert.IsTrue(PasswordHelper.IsBcryptHash(hash));
        }

        [TestMethod]
        public void IsBcryptHash_PlainTextPassword_ReturnsFalse()
        {
            Assert.IsFalse(PasswordHelper.IsBcryptHash("обычныйПароль123"));
        }
    }
}