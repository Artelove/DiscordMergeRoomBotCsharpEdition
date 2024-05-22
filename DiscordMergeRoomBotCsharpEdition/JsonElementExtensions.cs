using MongoDB.Bson;
using System.Text.Json;

namespace DiscordMergeRoomBotCsharpEdition
{
    public static class JsonElementExtensions
    {
        public static BsonDocument ToBD(this JsonElement element)
        {
            var jsonString = element.GetRawText();
            return BsonDocument.Parse(jsonString);
        }
    }
}
