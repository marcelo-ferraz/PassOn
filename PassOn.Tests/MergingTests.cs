using PassOn.Tests.Models;

namespace PassOn.Tests
{
    [TestFixture]
    public class MergingTests
    {
        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }

        [Test]
        public void MergingTest()
        {
            var date =
                DateTime.Now;

            var @base =
                new Inheritance.ComplexBase { 
                    Int = 1,
                    String = "something",
                    // Numbers = new List<int> { 1, 2, 3 },
                    List = new List<Inheritance.IntWrapper> { new Inheritance.IntWrapper() { Value = 1 } },
                };

            var inherited =
                new Inheritance.Simple { Date = date };

            var inheritedHashCode =
                inherited.GetHashCode();

            var newValue =
                Pass.Onto(@base, inherited);

            Assert.True(string.IsNullOrEmpty(inherited.String));
            Assert.That(newValue.Int, Is.EqualTo(1));
            Assert.That(newValue.Date, Is.EqualTo(date));
            Assert.That(newValue.GetHashCode(), Is.Not.EqualTo(inheritedHashCode));
        }


        [Test]
        public void MergingWithNullDestinationWillReturnAnClonedDestination()
        {
            var @base =
                new Inheritance.ComplexBase { Int = 1, String = "something" };
            
            Assert.Throws<ArgumentNullException>(() => Pass.Onto<Inheritance.ComplexBase, Inheritance.Simple?>(@base, null));
        }

        [Test]
        public void MergingWithNullSourceWillReturnAnClonedDestination()
        {
            var expected =
                new Inheritance.Simple { Date = DateTime.Now };

            Assert.Throws<ArgumentNullException>(
                () => Pass.Onto<Inheritance.ComplexBase?, Inheritance.Simple>(null, expected));

        }

        [Test]
        public void MergingWithNullsReturnsAnInstance()
        {
            Assert.Throws<ArgumentNullException>(
                () => Pass.Onto<Inheritance.ComplexBase?, ComplexClass?>(null, null));
        }
    }
}

