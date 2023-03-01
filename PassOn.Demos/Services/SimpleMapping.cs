using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace PassOn.Demos.Services
{
    internal class SimpleMapping : IHostedService
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

            if (!result.Id.Equals(initialId)
                || !result.Text.Equals(initialText)
            ) { throw new InvalidOperationException("This mapping was not successfull..."); }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { return Task.CompletedTask; }
    }
}
