namespace PassOn.Tests.Scenarios.Merging
{
    [TestFixture]
    internal class TargetCustomMerging
    {
        class Source
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
        }

        class Target
        {
            [MapStrategy(Strategy.CustomMap, Alias = "Id")]
            public string? Oid { get; set; }

            [MapStrategy(Strategy.CustomMap, "Text", Mapper = "MapText")]
            public string? Message { get; set; }

            public void MapOid(object obj)
            {
                Oid = obj.ToString() ?? string.Empty;
            }

            public void MapText(object obj)
            {
                Message = obj.ToString() ?? string.Empty;
            }
        }


        [Test]
        public void Test()
        {
            var initialId = Guid.NewGuid();
            var initialText = Utilities.RandomString();
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var target = new Target
            {
                Oid = otherId.ToString(),
                Message = otherText,
            };

            var result = src.Merge(target);

            Assert.That(result.Oid, Is.EqualTo(initialId.ToString()));
            Assert.That(result.Message, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            PassOnEngine.ClearCache();
        }
    }
}
