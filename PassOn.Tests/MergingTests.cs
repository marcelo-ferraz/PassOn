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
                new BaseClass { Int = 1, String = "something" };

            var inherited =
                new InheritedClass { Date = date };

            var inheritedHashCode =
                inherited.GetHashCode();

            Assert.True(string.IsNullOrEmpty(inherited.String));

            var newValue =
                Pass.Onto(@base, inherited);

            Assert.False(string.IsNullOrEmpty(inherited.String));
            Assert.That(inherited.Int, Is.EqualTo(1));
            Assert.That(inherited.Date, Is.EqualTo(date));
            Assert.That(inherited.GetHashCode(), Is.EqualTo(inheritedHashCode));
            Assert.That(newValue.GetHashCode(), Is.EqualTo(inheritedHashCode));
        }


        [Test]
        public void MergingWithNullDestinationWillReturnAnClonedDestination()
        {
            var @base =
                new BaseClass { Int = 1, String = "something" };
                     
            var mergedValue = Pass.Onto<BaseClass, InheritedClass>(@base, null);

            Assert.That(mergedValue.String, Is.EqualTo(@base.String));
            Assert.That(mergedValue.Int, Is.EqualTo(@base.Int));
            Assert.That(mergedValue.GetHashCode(), Is.Not.EqualTo(@base.GetHashCode()));
        }

        [Test]
        public void MergingWithNullSourceWillReturnAnClonedDestination()
        {
            var expected =
                new InheritedClass { Date = DateTime.Now };

            var result = Pass
                .Onto<BaseClass?, InheritedClass>(null, expected);                

            Assert.That(result.GetHashCode(), Is.Not.EqualTo(expected.GetHashCode()));
            Assert.That(result.Date, Is.EqualTo(expected.Date));
        }

        [Test]
        public void MergingWithNullsReturnsAnInstance()
        {
            var result = Pass.Onto<BaseClass?, DifferentClass?>(null, null);
            Assert.IsNotNull(result);           
        }
    }
}

