using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace PassOn.Demos.Services
{
    internal class TargetAttributeMapping : IHostedService
    {
        class Source
        {
            public Guid Id { get; set; }
            public string? Text { get; set; }
        }

        class Target
        {
            [MapStrategy(Alias = "Id")]
            public Guid Oid { get; set; }

            [MapStrategy(Alias = "Text")]
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
