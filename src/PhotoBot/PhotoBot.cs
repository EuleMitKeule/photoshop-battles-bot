using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace PhotoBot
{
    public class PhotoBot
    {
        public PhotoConfig Config { get; }

        DiscordSocketClient Client { get; }
        DiscordRestClient RestClient { get; }
        CommandService Commands { get; }
        PhotoBotParser Parser { get; }

        public RestGuild Guild { get; set; }

        public PhotoBot(PhotoConfig config)
        {
            Service.PhotoBot = this;

            Config = config;

            Client = new DiscordSocketClient();
            RestClient = new DiscordRestClient();
            Commands = new CommandService();
            Parser = new PhotoBotParser(Client, Commands);

            Client.Log += OnLog;
        }

        public async Task Connect(string token)
        {
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await RestClient.LoginAsync(TokenType.Bot, token);

            await Parser.Connect();

            Guild = await RestClient.GetGuildAsync(Config.GuildId);
        }

        static Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}