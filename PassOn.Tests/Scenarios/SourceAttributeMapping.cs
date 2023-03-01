namespace PassOn.Tests.Scenarios
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

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var result = dto.To<Source, Target>();

            Assert.That(result.Oid, Is.EqualTo(initialId));
            Assert.That(result.Message, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            PassOnEngine.ClearCache();
        }
    }
}
