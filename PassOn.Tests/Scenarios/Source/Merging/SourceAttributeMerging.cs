namespace PassOn.Tests.Scenarios.Source.Merging
{
    [TestFixture]
    internal class SourceAttributeMerging
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
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var target = new Target
            {
                Oid = otherId,
                Message = otherText,
            };

            var result = dto.Merge(target);

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
