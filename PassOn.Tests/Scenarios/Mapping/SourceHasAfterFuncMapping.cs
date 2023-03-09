namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class SourceHasAfterFuncMapping
    {
        private static string AddToText(string? txt)
        {
            return $"{txt} + !!";
        }

        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
            public Target After(Source src, Target tgt)
            {
                tgt.Text = AddToText(Text);
                return tgt;
            }
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
            };

            var result = src.Map<Source, Target>();

            var expectedText = AddToText(initialText);

            Assert.That(result.Id, Is.EqualTo(initialId));
            Assert.That(result.Text, Is.EqualTo(expectedText));
        }

        [TearDown]
        public void TearDown()
        {
            Pass.ClearCache();
        }
    }
}
