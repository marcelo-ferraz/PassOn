using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace PassOn.Tests.Scenarios
{
    [TestFixture]
    internal class SimpleMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string Text { get; set; }
        }

        class Target
        {
            public Guid Id { get; set; }

            public string Text { get; set; }
        }


        [Test]
        public void Test()
        {
            var initialId = Guid.NewGuid();
            var initialText = Utilities.RandomString();

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var result = dto.To<Source, Target>();

            Assert.AreEqual(initialId, result.Id);
            Assert.AreEqual(initialText, result.Text);
        }
    }
}
