using Discord.WebSocket;

namespace DiscordMergeRoomBotCsharpEdition.Commands
{
    public class EchoCommandHandler : ICommandHandler
    {
        public string Name => "echo";

        public async Task HandleCommandAsync(SocketSlashCommand command)
        {
            if (command.CommandName == Name)
            {
                var message = command.Data.Options.First().Value.ToString();
                await command.RespondAsync(message);
            }
        }
    }
}
