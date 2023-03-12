using PassOn.Tests.Models;

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
                Pass.On(@base);

            Assert.That(newValue.String, Is.EqualTo(@base.String));
            Assert.That(newValue.Int, Is.EqualTo(@base.Int));
            Assert.That(newValue.GetHashCode(), Is.Not.EqualTo(@base.GetHashCode()));
        }

        [Test]
        public void ClonningWithNullParameter()
        {
            var result = Pass.On<BaseClass?>(null);
            Assert.IsNotNull(result);            
        }

        [Test]
        public void ShallowClonningWithNullParameter()
        {
            var result = Pass.On<BaseClass?>(null, Strategy.Shallow);
            Assert.IsNotNull(result);
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
                Numbers = new List<int> { 1, 2, 3 },
                List = new List<BaseClass.SubClass> { new BaseClass.SubClass() { Value = 1 } },
                List2Array = new List<BaseClass.SubClass> { new BaseClass.SubClass() { Value = 2 } },
                Array = new BaseClass.SubClass[] { new BaseClass.SubClass() { Value = 3 } },
                Array2List = new BaseClass.SubClass[] { new BaseClass.SubClass() { Value = 4 } },
            };

            var engine = new PassOnEngine();

            var diffValue =
                engine.MapObjectWithILDeep<InheritedClass, DifferentClass>(inherited);

            Assert.False(string.IsNullOrEmpty(diffValue.String));
            Assert.That(diffValue.String, Is.EqualTo(inherited.String));
            Assert.That(diffValue.Data, Is.EqualTo(inherited.Date));
        }
    }
}
