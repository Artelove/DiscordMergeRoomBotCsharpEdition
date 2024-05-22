namespace DiscordMergeRoomBotCsharpEdition
{

    public class DiscordBotService : IHostedService
    {
        private readonly IServiceProvider _services;

        public DiscordBotService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Program.InitializeDiscordClient(_services);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
