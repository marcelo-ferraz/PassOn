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
                new Inheritance.ComplexBase { Int = 1, String = "something" };
            
            var newValue =
                Pass.On(@base);

            Assert.That(newValue.String, Is.EqualTo(@base.String));
            Assert.That(newValue.Int, Is.EqualTo(@base.Int));
            Assert.That(newValue.GetHashCode(), Is.Not.EqualTo(@base.GetHashCode()));
        }

        [Test]
        public void ClonningWithNullParameter()
        {
            Assert.Throws<ArgumentNullException>(() => Pass.On<Inheritance.ComplexBase?>(null));            
        }

        [Test]
        public void ShallowClonningWithNullParameter()
        {
            var result = Pass.On<Inheritance.ComplexBase?>(null, Strategy.Shallow);
            Assert.IsNotNull(result);
        }


        [Test]
        public void CloneDifferentObjects()
        {
            var date =
                DateTime.Now;

            var inherited = new Inheritance.Simple
            {
                Int = 1,
                String = "something",
                Date = date,
                Numbers = new List<int> { 1, 2, 3 },
                List = new List<Inheritance.IntWrapper> { new Inheritance.IntWrapper() { Value = 1 } },
                SecondArray = new List<Inheritance.IntWrapper> { new Inheritance.IntWrapper() { Value = 2 } },
                Array = new Inheritance.IntWrapper[] { new Inheritance.IntWrapper() { Value = 3 } },
                SecondList = new Inheritance.IntWrapper[] { new Inheritance.IntWrapper() { Value = 4 } },
            };

            var engine = new PassOnEngine();

            var diffValue =
                engine.MapObjectWithILDeep<Inheritance.Simple, ComplexClass>(inherited);

            Assert.False(string.IsNullOrEmpty(diffValue.String));
            Assert.That(diffValue.String, Is.EqualTo(inherited.String));
            Assert.That(diffValue.Data, Is.EqualTo(inherited.Date));
        }

        [Test]
        public void CloneObjectWithCyclicalDependency() {
            var rdn = new Random();

            var obj =
                new CyclicalDependency.Parent()
                {
                    Id = rdn.Next(),
                };


            obj.Child = new CyclicalDependency.Child {
                Id = rdn.Next(),
                Parent = obj
            };

            var engine = new PassOnEngine();

            Assert.Throws<StackOverflowException>(
                () =>  engine.MapObjectWithILDeep<CyclicalDependency.Parent, CyclicalDependency.Parent>(obj)
            );
        }
    }
}
