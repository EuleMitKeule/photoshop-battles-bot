using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace PhotoBot.Commands
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("start battle")]
        public async Task StartBattleAsync()
        {
            var photoBot = Service.PhotoBot;

            await ReplyAsync("Starting new photo proposal phase.");

            var proposalsChannel = await ChannelCreator.CreateChannelAsync("proposals", photoBot.Config.PhotoCategoryId);

            if (photoBot.Config.CurrentProposalsChannelId != 0)
            {
                var oldProposalsChannel =
                    await photoBot.RestGuild.GetChannelAsync(photoBot.Config.CurrentProposalsChannelId);
                Archiver.ArchiveChannelAsync(oldProposalsChannel);
            }

            photoBot.Config.CurrentProposalsChannelId = proposalsChannel.Id;

            await photoBot.GetPhotoUsersAsync();

            await PhotoConfig.SaveAsync();
        }
    }
}