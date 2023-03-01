namespace PassOn.Tests.Scenarios
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
                this.Oid = obj.ToString() ?? String.Empty;
            }

            public void MapText(object obj)
            {
                this.Message = obj.ToString() ?? String.Empty;
            }
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
