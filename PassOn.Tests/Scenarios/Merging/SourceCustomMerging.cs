namespace PassOn.Tests.Scenarios.Merging
{
    [TestFixture]
    internal class SourceCustomMerging
    {
        private static string AddToText(string? txt)
        {
            return $"{txt} + !!";
        }

        class Source
        {
            public Guid Id { get; set; }

            [MapStrategy(Strategy.CustomMap)]
            public string? Text { get; set; }

            public string MapText()
            {
                return AddToText(Text);
            }
        }

        class Target
        {
            public Guid Id { get; set; }

            public string Text { get; set; } = string.Empty;
        }


        [Test]
        public void Test()
        {
            var initialId = Guid.NewGuid();
            var initialText = Utilities.RandomString();
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };
            
            var target = new Target
            {
                Id = otherId,
                Text = otherText,
            };

            var expectedText = AddToText(initialText);

            var result = dto.To(target);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(expectedText));
        }

        [TearDown]
        public void TearDown()
        {
            PassOnEngine.ClearCache();
        }
    }
}
