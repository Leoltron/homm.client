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
            Assert.AreEqual(d["two"], 7);
            Assert.AreEqual(d["three"], 7);
        }

        [Test]
        public void TestAddOrSumAdd()
        {
            var d = GetDictionary();
            d.AddOrSum("four", 4);
            d.AddOrSum("five", 5);
            Assert.AreEqual(d["four"], 4);
            Assert.AreEqual(d["five"], 5);
        }
    }
}