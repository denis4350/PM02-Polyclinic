using HospitalSystem.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HospitalSystem.Tests
{
    [TestClass]
    public class NameHelperTests
    {
        [TestMethod]
        public void FullName_WithMiddleName_JoinsAllThreeParts()
        {
            string result = NameHelper.FullName("Иванов", "Иван", "Иванович");

            Assert.AreEqual("Иванов Иван Иванович", result);
        }

        [TestMethod]
        public void FullName_NullMiddleName_JoinsOnlyLastAndFirst()
        {
            string result = NameHelper.FullName("Иванов", "Иван", null);

            Assert.AreEqual("Иванов Иван", result);
        }

        [TestMethod]
        public void FullName_EmptyMiddleName_TreatedSameAsNull()
        {
            string result = NameHelper.FullName("Иванов", "Иван", "");

            Assert.AreEqual("Иванов Иван", result);
        }
    }
}