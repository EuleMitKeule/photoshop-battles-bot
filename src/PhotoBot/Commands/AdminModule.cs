using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            photoBot.Config.CurrentProposalsChannelId = proposalsChannel.Id;

            await photoBot.GetPhotoUsersAsync();

            photoBot.Config.Proposals = new List<PhotoProposal>();

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
                .OrderByDescending(element => element.Score)
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

            await Archiver.ArchiveChannelAsync(proposalsChannel);
        }
    }
}