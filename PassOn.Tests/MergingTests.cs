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
                new BaseClass { 
                    Int = 1,
                    String = "something",
                    // Numbers = new List<int> { 1, 2, 3 },
                    List = new List<BaseClass.SubClass> { new BaseClass.SubClass() { Value = 1 } },
                };

            var inherited =
                new InheritedClass { Date = date };

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
                new BaseClass { Int = 1, String = "something" };
            
            Assert.Throws<ArgumentNullException>(() => Pass.Onto<BaseClass, InheritedClass?>(@base, null));
        }

        [Test]
        public void MergingWithNullSourceWillReturnAnClonedDestination()
        {
            var expected =
                new InheritedClass { Date = DateTime.Now };

            Assert.Throws<ArgumentNullException>(
                () => Pass.Onto<BaseClass?, InheritedClass>(null, expected));

        }

        [Test]
        public void MergingWithNullsReturnsAnInstance()
        {
            Assert.Throws<ArgumentNullException>(
                () => Pass.Onto<BaseClass?, DifferentClass?>(null, null));
        }
    }
}

