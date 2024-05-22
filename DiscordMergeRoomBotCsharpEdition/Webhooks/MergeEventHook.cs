using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DiscordMergeRoomBotCsharpEdition.Webhooks
{

    public class MergeEventHook
    {
        private readonly DiscordSocketClient _client;
        private readonly IMongoClient _mongoClient;

        public MergeEventHook(IMongoClient mongoClient, DiscordSocketClient client)
        {
            _mongoClient = mongoClient;
            _client = client;
        }

        public async Task ParseMergeAsync(BsonDocument data, string guildId)
        {
            var title = data["object_attributes"]["title"].AsString;
            var message = $"Merge request event: {title} in guild: {guildId}";
            await SendMessageToGuildChannel(guildId, message);
        }

        private async Task SendMessageToGuildChannel(string guildId, string message)
        {
            var id = ulong.Parse(guildId);
            var guild = _client.GetGuild(id);
            var defaultChannel = guild.DefaultChannel;
            if (defaultChannel != null)
            {
                await defaultChannel.SendMessageAsync(message);
            }
        }
    }
}
