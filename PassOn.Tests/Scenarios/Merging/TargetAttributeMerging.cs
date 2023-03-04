namespace PassOn.Tests.Scenarios.Merging
{
    [TestFixture]
    internal class TargetAttributeMerging
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
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();
            var otherMessage = Utilities.RandomString();

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
                Message = initialMessage,
            };

            var target = new Target
            {
                Oid = otherId,
                Text = otherText,
                Message = otherMessage,
            };

            var result = src.To(target);

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
