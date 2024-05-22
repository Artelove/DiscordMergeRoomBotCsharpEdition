using DiscordMergeRoomBotCsharpEdition.Webhooks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace DiscordMergeRoomBotCsharpEdition.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly MergeEventHook _mergeEventHook;
        private readonly IMongoClient _mongoClient;
        private readonly NoteEventHook _noteEventHook;

        public RequestController(IMongoClient mongoClient, MergeEventHook mergeEventHook, NoteEventHook noteEventHook)
        {
            _mongoClient = mongoClient;
            _mergeEventHook = mergeEventHook;
            _noteEventHook = noteEventHook;
        }

        [HttpPost("parse-point")]
        public async Task<IActionResult> ParsePoint([FromBody] JsonElement request)
        {
            var json = request.ToBD();
            var url = json["project"]["web_url"].AsString;
            var guildId = await GetGuildId(url);

            if (guildId == null)
            {
                return NotFound("Project not found!");
            }

            switch (json["event_type"].AsString)
            {
                case "merge_request":
                    await _mergeEventHook.ParseMergeAsync(json, guildId);
                    break;
                case "note":
                    await _noteEventHook.ParseNoteAsync(json, guildId);
                    break;
            }

            return Ok("WebHook parsed");
        }

        private async Task<string> GetGuildId(string url)
        {
            var database = _mongoClient.GetDatabase("mergeRoomBot");
            var collection = database.GetCollection<BsonDocument>("projects");
            var filter = Builders<BsonDocument>.Filter.Eq("gitlab_link", url);
            var result = await collection.Find(filter).FirstOrDefaultAsync();
            return result?["guild_id"].AsString;
        }
    }
}
