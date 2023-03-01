using NUnit.Framework;
using PassOn.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Tests
{
    [TestFixture]
    public class MergingTests
    {
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
            Assert.AreEqual(1, inherited.Int);
            Assert.AreEqual(date, inherited.Date);
            Assert.AreEqual(inheritedHashCode, inherited.GetHashCode());
            Assert.AreEqual(inheritedHashCode, newValue.GetHashCode());
        }


        [Test]
        public void MergingWithNullDestinationWillReturnAnClonedDestination()
        {
            var @base =
                new BaseClass { Int = 1, String = "something" };
                     
            var mergedValue = Pass.Onto<BaseClass, InheritedClass>(@base, null);

            Assert.AreEqual(@base.String, mergedValue.String);
            Assert.AreEqual(@base.Int, mergedValue.Int);
            Assert.AreNotEqual(@base.GetHashCode(), mergedValue.GetHashCode());
        }

        [Test]
        public void MergingWithNullSourceWillReturnAnClonedDestination()
        {
            var expected =
                new InheritedClass { Date = DateTime.Now };

            var result = Pass
                .Onto<BaseClass, InheritedClass>(null, expected);                

            Assert.AreNotEqual(expected.GetHashCode(), result.GetHashCode());
            Assert.AreEqual(expected.Date, result.Date);
        }

        [Test]
        public void MergingWithNullsReturnsAnInstance()
        {
            var result = Pass.Onto<BaseClass, DifferentClass>(null, null);
            Assert.IsNotNull(result);           
        }
    }
}

