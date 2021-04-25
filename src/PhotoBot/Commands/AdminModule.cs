using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PhotoBot.Commands
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("start battle", RunMode = RunMode.Async)]
        public async Task StartBattleAsync()
        {
            var photoBot = Service.PhotoBot;

            await ReplyAsync("Starting new photoshop battle.");

            var proposalsChannel = await ChannelCreator.CreateChannelAsync("proposals", photoBot.Config.PhotoCategoryId);

            if (photoBot.Config.CurrentProposalsChannelId != 0)
            {
                var oldProposalsChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.CurrentProposalsChannelId);
                await Archiver.ArchiveChannelAsync(oldProposalsChannel);
            }
            
            if (photoBot.Config.CurrentVotingChannelId != 0)
            {
                var oldVotingChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.CurrentVotingChannelId);
                await Archiver.ArchiveChannelAsync(oldVotingChannel);
            }

            if (photoBot.Config.UserIdToPhotoChannelId != null)
            {
                foreach (var (_, value) in photoBot.Config.UserIdToPhotoChannelId)
                {
                    if (value == 0) continue;
                    var channel = photoBot.SocketGuild.GetTextChannel(value);
                    if (channel != null) await channel.DeleteAsync();
                }
            }

            photoBot.Config.CurrentProposalsChannelId = proposalsChannel.Id;

            await photoBot.GetPhotoUsersAsync();

            photoBot.Config.Proposals = new List<PhotoMessage>();
            photoBot.Config.Photos = new List<PhotoMessage>();
            photoBot.Config.UserIdToPhotoChannelId = new Dictionary<ulong, ulong>();

            await PhotoConfig.SaveAsync();
        }

        [Command("reset proposal", RunMode = RunMode.Async)]
        public async Task ResetProposalAsync(IUser user)
        {
            var photoBot = Service.PhotoBot;
            var socketUser = photoBot.SocketGuild.GetUser(user.Id);

            await photoBot.DeleteProposalAsync(socketUser);
        }

        [Command("stop proposals", RunMode = RunMode.Async)]
        public async Task StopProposalsAsync()
        {
            var photoBot = Service.PhotoBot;

            var winnerProposal = photoBot.Config.Proposals
                .OrderBy(element => Guid.NewGuid())
                .ThenByDescending(element => element.Score)
                .First();

            var proposalsChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.CurrentProposalsChannelId);
            var winnersChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.WinnerChannelId);

            using var client = new WebClient();
            await client.DownloadFileTaskAsync(new Uri(winnerProposal.ImageUrl), "winner_proposal.png");

            try
            {
                await winnersChannel.SendFileAsync("winner_proposal.png", $"Thema: {winnerProposal.Topic}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var users = photoBot.Config.PhotoUserIds;
            photoBot.Config.UserIdToPhotoChannelId = new Dictionary<ulong, ulong>();

            foreach (var userId in users)
            {
                var user = photoBot.SocketGuild.GetUser(userId);
                var photoChannel = await ChannelCreator.CreateChannelAsync($"photo-{user.Username}", photoBot.Config.PhotoCategoryId);

                var denyAllPermissions = new Overwrite(photoBot.Config.EveryoneRoleId, PermissionTarget.Role,
                    OverwritePermissions.DenyAll(photoChannel));

                var allowUserPermissions = new Overwrite(userId, PermissionTarget.User,
                    OverwritePermissions.AllowAll(photoChannel));

                var permissions = new List<Overwrite> {denyAllPermissions, allowUserPermissions};

                await photoChannel.ModifyAsync(prop => prop.PermissionOverwrites = permissions);

                photoBot.Config.UserIdToPhotoChannelId.Add(userId, photoChannel.Id);
            }

            await Archiver.ArchiveChannelAsync(proposalsChannel);

            await PhotoConfig.SaveAsync();
        }
        
        [Command("start voting", RunMode = RunMode.Async)]
        public async Task StartVotingAsync()
        {
            var photoBot = Service.PhotoBot;
            var photoVotingChannel = await ChannelCreator.CreateChannelAsync("photo-voting", photoBot.Config.PhotoCategoryId);
            photoBot.Config.CurrentVotingChannelId = photoVotingChannel.Id;

            photoBot.Config.Photos = photoBot.Config.Photos.OrderBy(element => Guid.NewGuid()).ToList();
            foreach (var photo in photoBot.Config.Photos)
            {
                using var webClient = new WebClient();
                await webClient.DownloadFileTaskAsync(photo.ImageUrl, "photo.png");
                var photoMessage = await photoVotingChannel.SendFileAsync("photo.png", "");
                await photoMessage.AddReactionAsync(new Emoji("✅"));
                await photoMessage.AddReactionAsync(new Emoji("❌"));
                photo.MessageId = photoMessage.Id;
            }
            
            foreach (var (_, value) in photoBot.Config.UserIdToPhotoChannelId)
            {
                if (value == 0) continue;
                var channel = photoBot.SocketGuild.GetTextChannel(value);
                if (channel != null) await channel.DeleteAsync();
            }
            
            await PhotoConfig.SaveAsync();
        }
        
        [Command("stop voting", RunMode = RunMode.Async)]
        public async Task StopVotingAsync()
        {
            var photoBot = Service.PhotoBot;
            var winner = photoBot.Config.Photos
                .OrderBy(element => Guid.NewGuid())
                .ThenByDescending(element => element.Score)
                .First();
            var winnerChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.WinnerChannelId);
            var winnerUser = photoBot.SocketGuild.GetUser(winner.UserId);
            
            using var webClient = new WebClient();
            await webClient.DownloadFileTaskAsync(winner.ImageUrl, "winner.png");
            await winnerChannel.SendFileAsync("winner.png", $"The winner is: {winnerUser.Username}");

            var votingChannel = photoBot.SocketGuild.GetTextChannel(photoBot.Config.CurrentVotingChannelId);
            await Archiver.ArchiveChannelAsync(votingChannel);
        }
    }
}