namespace PassOn.Tests.Scenarios.Merging
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

            [AfterMapping]
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
            var otherId = Guid.NewGuid();
            var otherText = Utilities.RandomString();

            var src = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var target = new Target
            {
                Id = otherId,
                Text = otherText,
            };

            var result = src.Merge(target);

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
