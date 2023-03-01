using NUnit.Framework;
using System;

namespace PassOn.Tests.Scenarios
{
    [TestFixture]
    internal class SourceCustomMapping
    {
        private static string AddToText(string txt)
        {
            return $"{txt} + !!";
        }

        class Source
        {
            public Guid Id { get; set; }

            [MapStrategy(Strategy.CustomMap)]
            public string? Text { get; set; }

            public string MapText() {
                return AddToText(this.Text);
            }
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

            var expectedText = AddToText(initialText);

            var result = dto.To<Source, Target>();

            Assert.AreEqual(initialId, result.Id);
            Assert.AreEqual(initialText, result.Text);
        }
    }
}
