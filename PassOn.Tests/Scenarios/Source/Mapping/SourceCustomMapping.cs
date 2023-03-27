namespace PassOn.Tests.Scenarios.Source.Mapping
{
    [TestFixture]
    internal class SourceCustomMapping
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

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var expectedText = AddToText(initialText);

            var result = src.Map<Source, Target>();

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(expectedText));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
