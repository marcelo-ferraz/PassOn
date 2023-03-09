namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class SourceHasMoreFieldsMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
            public DateTime? Date { get; set; }
            public int? NullableNumber { get; set; }
            public int Number { get; set; }
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

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
                Date = DateTime.Now,
                NullableNumber = Utilities.NextRandomInt(),
                Number = Utilities.NextRandomInt(),
            };

            var result = src.Map<Source, Target>();

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
