using Discord.WebSocket;

namespace DiscordMergeRoomBotCsharpEdition
{
    public interface ICommandHandler
    {
        string Name { get; }
        Task HandleCommandAsync(SocketSlashCommand command);
    }
}
