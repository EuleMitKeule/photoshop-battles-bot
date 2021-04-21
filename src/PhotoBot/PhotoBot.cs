using System;
using System.Collections.Generic;
using System.Linq;
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

        public DiscordSocketClient SocketClient { get; }
        public DiscordRestClient RestClient { get; }
        public CommandService Commands { get; }
        public PhotoBotParser Parser { get; }

        public SocketGuild SocketGuild { get; private set; }
        public RestGuild RestGuild { get; private set; }

        public PhotoBot(PhotoConfig config)
        {
            Service.PhotoBot = this;

            Config = config;

            SocketClient = new DiscordSocketClient();
            RestClient = new DiscordRestClient();
            Commands = new CommandService();
            Parser = new PhotoBotParser(SocketClient, Commands);

            SocketClient.Log += OnLog;
            SocketClient.Connected += OnSocketConnected;
        }

        public async Task GetPhotoUsersAsync()
        {
            await SocketGuild.DownloadUsersAsync();

            var users = SocketGuild.Users;
            Config.PhotoUserIds = new List<ulong>();

            foreach (var user in users)
            {
                var photoRole = SocketGuild.GetRole(Config.PhotoRoleId);

                if (!user.Roles.Contains(photoRole)) continue;

                Config.PhotoUserIds.Add(user.Id);
            }

            await PhotoConfig.SaveAsync();
        }

        public async Task Connect(string token)
        {
            await SocketClient.LoginAsync(TokenType.Bot, token);
            await SocketClient.StartAsync();

            await RestClient.LoginAsync(TokenType.Bot, token);

            await Parser.Connect();
        }

        async Task OnSocketConnected()
        {
            SocketGuild = SocketClient.GetGuild(Config.GuildId);
            RestGuild = await RestClient.GetGuildAsync(Config.GuildId);
        }

        static Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}