namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class TargetCustomMapping
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

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var target = src.Map<Source, Target>();

            Assert.That(target.Oid, Is.EqualTo(initialId.ToString()));
            Assert.That(target.Message, Is.EqualTo(initialText));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
