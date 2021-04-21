using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace PhotoBot
{
    public static class ChannelCreator
    {
        public static async Task<RestTextChannel> CreateChannelAsync(string name, ulong categoryId) =>
            await Service.PhotoBot.SocketGuild.CreateTextChannelAsync(name, prop => prop.CategoryId = categoryId);
    }
}