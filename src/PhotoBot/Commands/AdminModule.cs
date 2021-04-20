using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace PhotoBot.Commands
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("start proposals")]
        public async Task StartProposalsAsync()
        {
            var photoBot = Service.PhotoBot;

            await ReplyAsync("Starting new photo proposal phase.");

            var proposalsChannel = await photoBot.Guild.CreateTextChannelAsync("proposals",
                prop => prop.CategoryId = photoBot.Config.PhotoCategoryId);

            if (photoBot.Config.CurrentProposalsChannelId != 0)
            {
                var oldProposalsChannel =
                    await photoBot.Guild.GetChannelAsync(photoBot.Config.CurrentProposalsChannelId);
                Archiver.ArchiveChannel(oldProposalsChannel);
            }

            photoBot.Config.CurrentProposalsChannelId = proposalsChannel.Id;

            await PhotoConfig.Save();
        }
    }
}