namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class TargetAttributeMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
            public string? Message { get; set; }
        }

        class Target
        {
            [MapStrategy(Alias = "Id")]
            public Guid Oid { get; set; }

            [MapStrategy("Message")]
            public string? Text { get; set; }

            [MapStrategy("Text")]
            public string? Message { get; set; }
        }


        [Test]
        public void Test()
        {
            var initialId = Guid.NewGuid();
            var initialText = Utilities.RandomString();
            var initialMessage = Utilities.RandomString();

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
                Message = initialMessage,
            };

            var result = src.Map<Source, Target>();

            Assert.That(result.Oid, Is.EqualTo(initialId));
            Assert.That(result.Message, Is.EqualTo(initialText));
            Assert.That(result.Text, Is.EqualTo(initialMessage));
        }

        [TearDown]
        public void TearDown()
        {
            PassOnEngine.ClearCache();
        }
    }
}
