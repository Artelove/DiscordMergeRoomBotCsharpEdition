using Discord.WebSocket;

namespace DiscordMergeRoomBotCsharpEdition.Commands
{
    public class HelpCommandHandler : ICommandHandler
    {
        public string Name => "help";

        public async Task HandleCommandAsync(SocketSlashCommand command)
        {
            if (command.CommandName == Name)
            {
                var helpMessage = "Available commands:\n" +
                                  "/ping - Responds with 'Pong!'\n" +
                                  "/help - Shows this help message";
                await command.RespondAsync(helpMessage);
            }
        }
    }
}
