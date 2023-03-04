namespace PassOn.Tests.Scenarios.Merging
{
    [TestFixture]
    internal class SourceHasMoreFieldsMerging
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
            var initialNullableNumber = Utilities.NextRandomInt();
            var initialNumber = Utilities.NextRandomInt();
            var initialDate = DateTime.Now;
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
                Date = initialDate,
                NullableNumber = initialNullableNumber,
                Number = initialNumber,
            };
            
            var target = new Target
            {
                Id = otherId,
                Text = otherText,
            };

            var result = dto.To(target);

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
