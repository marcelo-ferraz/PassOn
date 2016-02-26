using NUnit.Framework;
using PassOn.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests
{
    [TestFixture]
    public class ClonningTests
    {
        [Test]
        public void ClonningTest()
        {
            var @base =
                new BaseClass { Int = 1, String = "something" };
            
            var newValue =
                Pass.On<BaseClass>(@base);

            Assert.AreEqual(@base.String, newValue.String);
            Assert.AreEqual(@base.Int, newValue.Int);
            Assert.AreNotEqual(@base.GetHashCode(), newValue.GetHashCode());
        }

        [Test]
        public void ClonningWithNullParameter()
        {
            // Both will just return a new instance
            Assert.IsNotNull(Pass.On<BaseClass>(null));
            Assert.IsNotNull(Pass.On<BaseClass>(null, Inspection.Shallow));
        }

        [Test]
        public void CloneDifferentObjects()
        {
            var date =
                DateTime.Now;

            var inherited = new InheritedClass
            {
                Int = 1,
                String = "something",
                Date = date,
                //Numbers = new List<int> { 1, 2, 3 }, // <-- problem there
                List = new List<BaseClass.SubClass> { new BaseClass.SubClass() { Value = 1 } },
                List2Array = new List<BaseClass.SubClass> { new BaseClass.SubClass() { Value = 2 } },
                Array = new BaseClass.SubClass[] { new BaseClass.SubClass() { Value = 3 } },
                Array2List = new BaseClass.SubClass[] { new BaseClass.SubClass() { Value = 4 } }, 
            };

            var inheritedHashCode =
                inherited.GetHashCode();

            var diffValue =
                Pass.On<DifferentClass>(inherited);

            Assert.False(string.IsNullOrEmpty(diffValue.String));
            Assert.AreEqual(inherited.String, diffValue.String);

            Assert.AreEqual(inherited.Array.Length, diffValue.Array.Length);
            Assert.AreNotEqual(inherited.Array[0].GetHashCode(), diffValue.Array[0].GetHashCode());

            Assert.AreEqual(inherited.Array2List.Length, diffValue.Array2List.Count);
            Assert.AreNotEqual(inherited.Array2List[0].GetHashCode(), diffValue.Array2List[0].GetHashCode());

            Assert.AreEqual(inherited.List.Count, diffValue.List.Count);
            Assert.AreNotEqual(inherited.List[0].GetHashCode(), diffValue.List[0].GetHashCode());

            Assert.AreEqual(inherited.List2Array.Count, diffValue.List2Array.Length);
            Assert.AreNotEqual(inherited.List2Array[0].GetHashCode(), diffValue.List2Array[0].GetHashCode());
            
            Assert.AreEqual(date, diffValue.Data);
        }
    }
}
