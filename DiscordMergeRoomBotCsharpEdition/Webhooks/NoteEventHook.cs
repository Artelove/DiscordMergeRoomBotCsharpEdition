using Discord;
using Discord.WebSocket;
using DiscordMergeRoomBotCsharpEdition;
using MongoDB.Bson;
using MongoDB.Driver;

public class NoteEventHook
{
    private readonly DiscordSocketClient _client;
    private readonly GitLabService _gitLabService;
    private readonly IMongoClient _mongoClient;
    private string _guildId;

    public NoteEventHook(IMongoClient mongoClient, DiscordSocketClient client, GitLabService gitLabService)
    {
        _mongoClient = mongoClient;
        _client = client;
        _gitLabService = gitLabService;
    }

    public async Task ParseNoteAsync(BsonDocument body, string guildId)
    {
        _guildId = guildId;
        await SendNoteAsync(body);
    }

    private async Task SendNoteAsync(BsonDocument body)
    {
        try
        {
            var guild = _client.GetGuild(ulong.Parse(_guildId));
            var note = new Note
            {
                Data = body,
                Creator = await _gitLabService.GetUserAsync(body["user"]["id"].AsString),
                AdditionalDescription = "",
                Description = body["object_attributes"]["note"].AsString,
                Url = body["object_attributes"]["url"].AsString,
                ProjectName = body["project"]["name"].AsString,
                ProjectUrl = body["project"]["web_url"].AsString,
            };

            if (body["object_attributes"]["original_position"] != BsonNull.Value)
            {
                note.CodeArea = await GetRawFileFromBranchByNameAsync(
                    body["project_id"].AsString,
                    body["object_attributes"]["original_position"]["new_path"].AsString,
                    body["merge_request"]["source_branch"].AsString
                    );

                note.CodeArea = await GetLinesSectionFromCodeAsync(
                    note.CodeArea,
                    body["object_attributes"]["original_position"]["line_range"]["start"]["new_line"].AsInt32,
                    body["object_attributes"]["original_position"]["line_range"]["end"]["new_line"].AsInt32
                    );

                if (!string.IsNullOrEmpty(note.CodeArea))
                {
                    note.AdditionalDescription = $"```fix\n{note.CodeArea}```\n";
                }
            }

            var embed = GetEmbed(note);

            var mergeRequest = await _mongoClient.GetDatabase("mergeRoomBot")
                .GetCollection<BsonDocument>("merge_requests")
                .Find(Builders<BsonDocument>.Filter.Eq("gitlab_mr_id", body["merge_request"]["id"].AsString))
                .FirstOrDefaultAsync();

            var channel = guild.GetTextChannel(ulong.Parse(mergeRequest["channel_id"].AsString));
            if (!string.IsNullOrEmpty(note.Creator.Discord))
            {
                await channel.AddPermissionOverwriteAsync(
                    _client.GetUser(ulong.Parse(note.Creator.Discord)),
                    new OverwritePermissions(viewChannel: PermValue.Allow)
                    );
            }

            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private Embed GetEmbed(Note note)
    {
        var embedBuilder = new EmbedBuilder()
            .WithColor(new Color((uint)(note.Creator.Id % 16777215)))
            .WithAuthor(note.Creator.Name, note.Creator.AvatarUrl, note.Creator.WebUrl)
            .WithDescription($"{note.AdditionalDescription}{note.Description}\n[Note link]({note.Url})")
            .AddField("Project", $"[{note.ProjectName}]({note.ProjectUrl})", true)
            .AddField("Merge branch flow", $"{note.Data["merge_request"]["source_branch"]} -> {note.Data["merge_request"]["target_branch"]}", true)
            .WithTimestamp(DateTimeOffset.Now);

        return embedBuilder.Build();
    }

    private async Task<string> GetRawFileFromBranchByNameAsync(string projectId, string filePath, string branchName)
    {
        return await _gitLabService.GetRawFileFromBranchByNameAsync(projectId, filePath, branchName);
    }

    private async Task<string> GetLinesSectionFromCodeAsync(string code, int from, int to)
    {
        var countLines = 1;
        var lineSection = "";
        var stringRows = new string[to - from + 1];
        var index = 0;

        for (var i = 0; i < code.Length; i++)
        {
            if (countLines == to + 1)
            {
                break;
            }

            if (countLines >= from)
            {
                lineSection += code[i];
            }

            if (code[i] == '\n')
            {
                countLines++;
                if (countLines == to + 1)
                {
                    break;
                }

                if (countLines >= from)
                {
                    stringRows[index++] = lineSection;
                    lineSection = "";
                }
            }
        }

        return string.Join("", stringRows).TrimStart();
    }
}

public class Note
{
    public BsonDocument Data { get; set; }
    public User Creator { get; set; }
    public string AdditionalDescription { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string ProjectName { get; set; }
    public string ProjectUrl { get; set; }
    public string CodeArea { get; set; }
}

public class User
{
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string WebUrl { get; set; }
    public string Discord { get; set; }
    public int Id { get; set; }
}
