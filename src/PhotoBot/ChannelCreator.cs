using System.Threading.Tasks;
using Discord.Rest;

namespace PhotoBot
{
    public static class ChannelCreator
    {
        public static async Task<RestGuildChannel> CreateChannelAsync(string name, ulong categoryId) =>
            await Service.PhotoBot.RestGuild.CreateTextChannelAsync(name, prop => prop.CategoryId = categoryId);
    }
}