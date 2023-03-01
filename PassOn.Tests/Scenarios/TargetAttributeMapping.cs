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
