using Discord;
using Discord.WebSocket;
using DiscordMergeRoomBotCsharpEdition;
using DiscordMergeRoomBotCsharpEdition.Commands;
using MongoDB.Bson;
using MongoDB.Driver;

public class Program
{
    private static DiscordSocketClient _client;
    private static IMongoClient _mongoClient;
    private static string _botToken;
    private static List<ICommandHandler> _commandHandlers;
    
    public static DiscordSocketClient DiscordSocketClient => _client;
    public static IMongoClient MongoClient => _mongoClient;

    public static Task Main(string[] args)
    {
        return CreateHostBuilder(args).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }

    public static async Task InitializeDiscordClient(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var botToken = config["BotToken"];
        var mongoClient = services.GetRequiredService<IMongoClient>();

        var clientConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildEmojis |
                             GatewayIntents.GuildIntegrations |
                             GatewayIntents.GuildWebhooks |
                             GatewayIntents.GuildInvites |
                             GatewayIntents.GuildVoiceStates |
                             GatewayIntents.GuildPresences |
                             GatewayIntents.MessageContent |
                             GatewayIntents.GuildMessageReactions |
                             GatewayIntents.GuildMessageTyping |
                             GatewayIntents.DirectMessages |
                             GatewayIntents.DirectMessageReactions |
                             GatewayIntents.DirectMessageTyping
        };
        _client = services.GetRequiredService<DiscordSocketClient>();
        
        _client.Log += LogAsync;
        _client.Ready += async () =>
        {
            Console.WriteLine($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");

            var guildIdStr = config["GuildId"];
            if (string.IsNullOrEmpty(guildIdStr))
            {
                throw new ArgumentNullException("GuildId", "GuildId is not set in the configuration.");
            }

            if (!ulong.TryParse(guildIdStr, out var guildId))
            {
                throw new ArgumentException("GuildId is not a valid ulong.");
            }

            var guild = _client.GetGuild(guildId);
            if (guild == null)
            {
                Console.WriteLine($"Guild with ID {guildId} not found. Make sure the bot is invited to the server.");
                return;
            }
            else
            {
                Console.WriteLine($"Guild with ID {guildId} found.");
            }

            // Регистрация команд
            var commands = new List<SlashCommandBuilder>
            {
                new SlashCommandBuilder().WithName("ping").WithDescription("Responds with Pong!"),
                new SlashCommandBuilder().WithName("help").WithDescription("Shows help message"),
                new SlashCommandBuilder().WithName("echo").WithDescription("Echoes a message").AddOption("message", ApplicationCommandOptionType.String, "Message to echo", true)
            };

            foreach (var command in commands)
            {
                await guild.CreateApplicationCommandAsync(command.Build());
            }
        };
        _client.InteractionCreated += HandleInteraction;

        // Регистрация командных обработчиков напрямую
        var commandHandlers = new List<ICommandHandler>
        {
            new PingCommandHandler(),
            new HelpCommandHandler(),
            new EchoCommandHandler()
        };

        foreach (var handler in commandHandlers)
        {
            _client.SlashCommandExecuted += handler.HandleCommandAsync;
        }

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();
    }


    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static Task ReadyAsync()
    {
        Console.WriteLine($"Connected as {_client.CurrentUser}");
        return Task.CompletedTask;
    }

    private static async Task HandleInteraction(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var commandHandler = _commandHandlers.FirstOrDefault(h => h.Name == command.Data.Name);
            if (commandHandler != null)
            {
                await commandHandler.HandleCommandAsync(command);
            }
            else
            {
                await command.RespondAsync("Command not found.");
            }
        }
    }
    
    private async Task<string> GetGuildId(string url)
    {
        try
        {
            var database = _mongoClient.GetDatabase("mergeRoomBot");
            var collection = database.GetCollection<BsonDocument>("projects");
            var filter = Builders<BsonDocument>.Filter.Eq("gitlab_link", url);
            var result = await collection.Find(filter).FirstOrDefaultAsync();

            return result?["guild_id"].AsString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving guild id: {ex.Message}");
            return null;
        }
    }
}
