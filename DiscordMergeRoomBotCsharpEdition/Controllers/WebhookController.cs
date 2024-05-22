using DiscordMergeRoomBotCsharpEdition.Webhooks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace DiscordMergeRoomBotCsharpEdition.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly MergeEventHook _mergeEventHook;
        private readonly IMongoClient _mongoClient;
        private readonly NoteEventHook _noteEventHook;

        public WebhookController(IMongoClient mongoClient, MergeEventHook mergeEventHook, NoteEventHook noteEventHook)
        {
            _mongoClient = mongoClient;
            _mergeEventHook = mergeEventHook;
            _noteEventHook = noteEventHook;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var json = JObject.Parse(body);
            var bsonDocument = BsonDocument.Parse(json.ToString()); // Преобразование JObject в BsonDocument
            var url = json["project"]["web_url"].ToString();
            var guildId = await GetGuildId(url);

            if (guildId != null)
            {
                switch (json["event_type"].ToString())
                {
                    case "merge_request":
                        await _mergeEventHook.ParseMergeAsync(bsonDocument, guildId); // Используем BsonDocument
                        break;
                    case "note":
                        await _noteEventHook.ParseNoteAsync(bsonDocument, guildId); // Используем BsonDocument
                        break;
                    default:
                        return BadRequest("Unknown event type");
                }

                return Ok("Webhook parsed");
            }

            return NotFound("Project not found!");
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
