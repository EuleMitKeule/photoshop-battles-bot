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

            SocketClient.Log += OnLogAsync;
            SocketClient.Connected += OnSocketConnectedAsync;
            SocketClient.MessageReceived += OnMessageReceivedAsync;
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

        async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (message.Channel.Id == Config.CurrentProposalsChannelId)
            {
                await OnProposalReceivedAsync(message);
            }

            await Task.CompletedTask;
        }

        async Task OnProposalReceivedAsync(SocketMessage message)
        {
            var userId = message.Author.Id;

            if (!Config.PhotoUserIds.Contains(userId))
            {
                await message.DeleteAsync();
                return;
            }

            Config.Proposals ??= new List<PhotoProposal>();

            if (Config.Proposals.Any(element => element.UserId == userId))
            {
                await message.DeleteAsync();
                return;
            }

            var proposal = new PhotoProposal
            {
                UserId = userId,
                Topic = message.Content,
            };

            foreach (var attachment in message.Attachments)
            {
                proposal.ImageUrl = attachment.Url;
                break;
            }

            Config.Proposals.Add(proposal);

            Console.WriteLine($"Added new proposal from user {proposal.UserId} with topic {proposal.Topic} and image {proposal.ImageUrl}");

            await PhotoConfig.SaveAsync();

            await Task.CompletedTask;
        }

        public async Task ConnectAsync(string token)
        {
            await SocketClient.LoginAsync(TokenType.Bot, token);
            await SocketClient.StartAsync();

            await RestClient.LoginAsync(TokenType.Bot, token);

            await Parser.Connect();
        }

        async Task OnSocketConnectedAsync()
        {
            SocketGuild = SocketClient.GetGuild(Config.GuildId);
            RestGuild = await RestClient.GetGuildAsync(Config.GuildId);
        }

        static async Task OnLogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        }
    }
}