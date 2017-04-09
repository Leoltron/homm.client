using System.Collections.Generic;
using NUnit.Framework;

namespace Homm.Client.Tests
{
    [TestFixture]
    internal class DictExtTests
    {
        private static IDictionary<string, int> GetDictionary()
        {
            return new Dictionary<string, int> {{"one", 1}, {"two", 2}, {"three", 3}};
        }

        [Test]
        public void TestAddOrSumSum()
        {
            var d = GetDictionary();
            d.AddOrSum("two", 5);
            d.AddOrSum("three", 4);
            Assert.AreEqual(7, d["two"]);
            Assert.AreEqual(7, d["three"]);
        }

        [Test]
        public void TestAddOrSumAdd()
        {
            var d = GetDictionary();
            d.AddOrSum("four", 4);
            d.AddOrSum("five", 5);
            Assert.AreEqual(4, d["four"]);
            Assert.AreEqual(5, d["five"]);
        }
    }
}