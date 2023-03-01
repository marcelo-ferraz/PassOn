using NUnit.Framework;
using System;

namespace PassOn.Tests.Scenarios
{
    [TestFixture]
    internal class TargetAttributeMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
        }

        class Target
        {
            [MapStrategy(Alias = "Id")]
            public Guid Oid { get; set; }

            [MapStrategy(Alias = "Text")]
            public string Message { get; set; }
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

            Assert.AreEqual(initialId, result.Oid);
            Assert.AreEqual(initialText, result.Message);
        }
    }
}
