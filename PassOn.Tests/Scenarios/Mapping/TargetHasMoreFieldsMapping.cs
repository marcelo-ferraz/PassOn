namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class TargetHasMoreFieldsMapping
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

            public DateTime? Date { get; set; }

            public int? NullableNumber { get; set; }

            public int Number { get; set; }
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

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.IsNull(result.Date);
            Assert.IsNull(result.NullableNumber);
            Assert.That(result.Number, Is.EqualTo(0));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
