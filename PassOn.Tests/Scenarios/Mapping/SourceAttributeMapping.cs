namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class SourceAttributeMapping
    {
        class Source
        {
            [MapStrategy(Alias = "Oid")]
            public Guid Id { get; set; }

            [MapStrategy("Message")]
            public string? Text { get; set; }
        }

        class Target
        {
            public Guid Oid { get; set; }

            public string? Message { get; set; }
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

            var result = src.Map<Source, Target>();

            Assert.That(result.Oid, Is.EqualTo(initialId));
            Assert.That(result.Message, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
