using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace PassOn.Demos.Services
{
    internal class SourceAttributeMapping : IHostedService
    {
        class Source
        {
            [MapStrategy(Alias = "Oid")]
            public Guid Id { get; set; }

            [MapStrategy(Alias = "Message")]
            public string? Text { get; set; }
        }

        class Target
        {
            public Guid Oid { get; set; }

            public string Message { get; set; }
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

            var result = dto.To<Source, Target>();

            if (!result.Oid.Equals(initialId)
                || !result.Message.Equals(initialText)
            ) { throw new InvalidOperationException("This mapping was not successfull..."); }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { return Task.CompletedTask; }
    }
}
