namespace PassOn.Tests.Scenarios
{
    [TestFixture]
    internal class SimpleMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
        }

        class Target
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
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

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            PassOnEngine.ClearCache();
        }
    }
}
