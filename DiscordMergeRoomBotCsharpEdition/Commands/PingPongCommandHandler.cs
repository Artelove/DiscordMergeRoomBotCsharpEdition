using Discord.WebSocket;
using DiscordMergeRoomBotCsharpEdition;

public class PingCommandHandler : ICommandHandler
{
    public string Name => "ping";

    public async Task HandleCommandAsync(SocketSlashCommand command)
    {
        if (command.CommandName == Name)
        {
            await command.RespondAsync("Pong!");
        }
    }
}
