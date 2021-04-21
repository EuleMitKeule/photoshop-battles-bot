using System.Linq;
using Discord;
using Discord.WebSocket;

namespace PhotoBot
{
    public static class UserExtensions
    {
        public static bool IsPhotoUser(this SocketGuildUser user)
        {
            var photoBot = Service.PhotoBot;
            var photoRole = photoBot.SocketGuild.GetRole(photoBot.Config.PhotoRoleId);

            return user.Roles.Contains(photoRole);
        }

        public static bool IsPhotoUser(this IUser user)
        {
            var photoBot = Service.PhotoBot;
            var socketGuildUser = photoBot.SocketGuild.GetUser(user.Id);
            var photoRole = photoBot.SocketGuild.GetRole(photoBot.Config.PhotoRoleId);

            return socketGuildUser.Roles.Contains(photoRole);
        }
    }
}