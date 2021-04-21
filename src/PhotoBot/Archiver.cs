using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace PhotoBot
{
    public static class Archiver
    {
        public static async Task ArchiveChannelAsync(SocketTextChannel channel)
        {
            if (channel == null) return;

            var photoBot = Service.PhotoBot;

            if (channel.Category.Id == photoBot.Config.ArchiveCategoryId) return;

            await channel.ModifyAsync(prop => prop.CategoryId = photoBot.Config.ArchiveCategoryId);
            await channel.ModifyAsync(prop =>
                prop.Name = $"-{GetWeekOfMonth(DateTime.Now)}-{DateTime.Now.Month}-{DateTime.Now.Year}-" + channel.Name);
        }

        static int GetWeekOfMonth(DateTime date)
        {
            var beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate(date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }
    }
}