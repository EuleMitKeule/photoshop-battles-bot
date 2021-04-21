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
            SocketClient.ReactionAdded += OnReactionAddedAsync;
            SocketClient.ReactionRemoved += OnReactionRemovedAsync;
        }

        async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel.Id == Config.CurrentProposalsChannelId)
            {
                await OnProposalVoteReceivedAsync(cacheable, channel, reaction);
            }
        }

        async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (channel.Id == Config.CurrentProposalsChannelId)
            {
                await OnProposalVoteRevokedAsync(cacheable, channel, reaction);
            }
        }

        async Task OnProposalVoteReceivedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await cacheable.DownloadAsync();

            if (!message.Author.IsPhotoUser()) return;

            var proposal = Config.Proposals.Find(element => element.UserId == message.Author.Id);

            if (proposal == null) return;

            if (reaction.Emote.IsUpvote()) proposal.Upvotes += 1;
            if (reaction.Emote.IsDownvote()) proposal.Downvotes += 1;

            if (reaction.Emote.IsCancel())
            {
                if (reaction.User.Value.Id == message.Author.Id)
                {
                    var socketUser = SocketGuild.GetUser(reaction.User.Value.Id);
                    await DeleteProposalAsync(socketUser);
                }
            }

            await PhotoConfig.SaveAsync();
        }

        async Task OnProposalVoteRevokedAsync(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var message = await cacheable.DownloadAsync();

            if (!message.Author.IsPhotoUser()) return;

            var proposal = Config.Proposals.Find(element => element.UserId == message.Author.Id);

            if (proposal == null) return;

            if (reaction.Emote.IsUpvote()) proposal.Upvotes -= 1;
            if (reaction.Emote.IsDownvote()) proposal.Downvotes -= 1;

            await PhotoConfig.SaveAsync();
        }

        public async Task GetPhotoUsersAsync()
        {
            await SocketGuild.DownloadUsersAsync();

            var users = SocketGuild.Users;
            Config.PhotoUserIds = new List<ulong>();

            foreach (var user in users)
            {
                if (!user.IsPhotoUser()) continue;

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
                MessageId = message.Id,
                UserId = userId,
                Topic = message.Content,
                ImageUrl = message.Attachments.ElementAt(0).Url,
            };

            Config.Proposals.Add(proposal);

            Console.WriteLine($"Added new proposal from user {proposal.UserId} with topic {proposal.Topic} and image {proposal.ImageUrl}");

            await message.AddReactionAsync(new Emoji("✅"));
            await message.AddReactionAsync(new Emoji("❌"));
            await message.AddReactionAsync(new Emoji("🚫"));

            await PhotoConfig.SaveAsync();
        }

        public async Task DeleteProposalAsync(SocketGuildUser user)
        {
            var proposal = Config.Proposals.Find(element => element.UserId == user.Id);

            if (proposal == null) return;

            var proposalsChannel = SocketGuild.GetTextChannel(Config.CurrentProposalsChannelId);

            try
            {
                await proposalsChannel.DeleteMessageAsync(proposal.MessageId);
            }
            catch (ArgumentException e) { }

            Config.Proposals.RemoveAll(element => element.UserId == user.Id);

            await PhotoConfig.SaveAsync();
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