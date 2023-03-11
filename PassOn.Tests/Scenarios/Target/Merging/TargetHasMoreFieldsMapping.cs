namespace PassOn.Tests.Scenarios.Target.Merging
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

            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var targetNullableNumber = Utilities.NextRandomInt();
            var targetNumber = Utilities.NextRandomInt();
            var targetDate = DateTime.Now;

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var target = new Target
            {
                Id = otherId,
                Text = otherText,
                NullableNumber = targetNullableNumber,
                Number = targetNumber,
                Date = targetDate,
            };

            var result = dto.Merge(target);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(initialText));
            Assert.That(result.Date, Is.EqualTo(targetDate));
            Assert.That(result.NullableNumber, Is.EqualTo(targetNullableNumber));
            Assert.That(result.Number, Is.EqualTo(targetNumber));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
