namespace PassOn.Tests.Scenarios.Mapping
{
    [TestFixture]
    internal class ExactMapping
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
            public void Before(Source src, Target tgt)
            {
                System.Diagnostics.Debugger.Break();
            }

            public void After(Source src, Target tgt)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        class Target
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
            
            public void Before(Source src, object tgt)
            {
                System.Diagnostics.Debugger.Break();
            }

            public object After(Source src, Target tgt)
            {
                System.Diagnostics.Debugger.Break();                
                return tgt;
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
