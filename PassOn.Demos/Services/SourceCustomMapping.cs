using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace PassOn.Demos.Services
{
    internal class SourceCustomMapping : IHostedService
    {
        private static string AddToText(string txt)
        {
            return $"{txt} + !!";
        }

        class Source
        {
            public Guid Id { get; set; }

            [MapStrategy(Strategy.CustomMap)]
            public string? Text { get; set; }

            public string MapText() {
                return AddToText(this.Text);
            }
        }

        class Target
        {
            public Guid Id { get; set; }

            public string Text { get; set; }
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            var initialId = Guid.NewGuid();
            var initialText = Utilities.RandomString();

            var dto = new Source
            {
                Id = initialId,
                Text = initialText,
            };

            var expectedText = AddToText(initialText);

            var result = dto.To<Source, Target>();

            if (!result.Id.Equals(initialId)
                || !result.Text.Equals(expectedText)
            ) { throw new InvalidOperationException("This mapping was not successfull..."); }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { return Task.CompletedTask; }
    }
}
