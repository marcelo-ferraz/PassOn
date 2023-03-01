using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace PassOn.Demos.Services
{
    internal class TargetCustomMapping : IHostedService
    {
        class Source
        {
            public Guid Id { get; set; }

            public string? Text { get; set; }
        }

        class Target
        {
            [MapStrategy(Strategy.CustomMap, Alias = "Id")]
            public string Oid { get; set; }

            [MapStrategy(Strategy.CustomMap, "Text", Mapper = "MapText")]
            public string Message { get; set; }

            public void MapOid(object obj) 
            {
                this.Oid = obj.ToString() ?? String.Empty;
            }
            
            public void MapText(object obj)
            {
                this.Message = obj.ToString() ?? String.Empty;
            }
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

            if (!result.Oid.Equals(initialId.ToString())
                || !result.Message.Equals(initialText)
            ) { throw new InvalidOperationException("This mapping was not successfull..."); }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { return Task.CompletedTask; }
    }
}
