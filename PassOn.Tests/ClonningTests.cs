using PassOn.Tests.Models;
using PassOn;

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

            var input = new Inheritance.Simple
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

            var result =
                engine.MapObjectWithILDeep<Inheritance.Simple, ComplexClass>(input);

            Assert.False(string.IsNullOrEmpty(result.String));
            Assert.That(result.String, Is.EqualTo(input.String));
            Assert.That(result.Data, Is.EqualTo(input.Date));
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
        
        [Test]
        public void CloneObjectList()
        {
            var date =
                DateTime.Now;

            var input = new Inheritance.Simple
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

            IList<ComplexClass> result = engine
                .MapObjectWithILDeep<
                    IList<Inheritance.Simple>,
                    IList<ComplexClass>
                >(new List<Inheritance.Simple> { input });

            Assert.False(string.IsNullOrEmpty(result[0].String));
            Assert.That(result[0].String, Is.EqualTo(input.String));
            Assert.That(result[0].Data, Is.EqualTo(input.Date));
        }

        [Test]
        public void CloneIntList()
        {
            var date =
                DateTime.Now;

            var engine = new PassOnEngine();

            var input = new List<int> { 1, 2, 3, 4 };

            IList<int> result = engine
                .MapObjectWithILDeep<
                    IList<int>,
                    List<int>
                >(input);

            Assert.That(result, Is.EquivalentTo(input));            
        }

        [Test]
        public void CloneObjectArray()
        {
            var date =
                DateTime.Now;

            var input = new Inheritance.Simple
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

            var result = engine
                .MapObjectWithILDeep<
                    Inheritance.Simple[],
                    ComplexClass[]
                >(new Inheritance.Simple[] { input });

            Assert.False(string.IsNullOrEmpty(result[0].String));
            Assert.That(result[0].String, Is.EqualTo(input.String));
            Assert.That(result[0].Data, Is.EqualTo(input.Date));
        }

        [Test]
        public void CloneIntArray()
        {
            var date =
                DateTime.Now;

            var engine = new PassOnEngine();

            var input = new int[] { 1, 2, 3, 4 };

            IList<int> result = engine
                .MapObjectWithILDeep<
                    int[],
                    int[]
                >(input);

            Assert.That(result, Is.EquivalentTo(input));
        }
    }
}
