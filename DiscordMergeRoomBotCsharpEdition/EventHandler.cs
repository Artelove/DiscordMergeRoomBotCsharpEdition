using Discord.WebSocket;

namespace DiscordMergeRoomBotCsharpEdition
{

    public class ExampleEventHandler
    {
        private readonly DiscordSocketClient _client;

        public ExampleEventHandler(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += OnMessageReceived;
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            if (message.Content == "!hello")
            {
                return message.Channel.SendMessageAsync("Hello, world!");
            }

            return Task.CompletedTask;
        }
    }
}
