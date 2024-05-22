using Discord;
using Discord.WebSocket;
using DiscordMergeRoomBotCsharpEdition.Webhooks;
using MongoDB.Driver;
using Prometheus;

namespace DiscordMergeRoomBotCsharpEdition
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Регистрация IMongoClient
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(Configuration["MongoUri"]);
                return new MongoClient(settings);
            });

            services.AddSingleton<MergeEventHook>();
            services.AddSingleton<NoteEventHook>();
            services.AddSingleton<PrometheusService>();
            
            services.AddSingleton(sp =>
            {
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
                return new DiscordSocketClient(clientConfig);
            });

            // Регистрация GitLabService
            services.AddHttpClient<GitLabService>(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "DiscordBot");
            });
            services.AddSingleton(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var accessToken = Configuration["GitLab:AccessToken"];
                return new GitLabService(httpClient, accessToken);
            });

            services.AddHostedService<DiscordBotService>();
            services.AddHostedService<PrometheusService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHttpMetrics();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers();
            });
        }
    }
}
