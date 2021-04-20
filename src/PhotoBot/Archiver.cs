using System;
using System.Globalization;
using Discord.Rest;

namespace PhotoBot
{
    public static class Archiver
    {
        public static async void ArchiveChannel(RestGuildChannel channel)
        {
            var photoBot = Service.PhotoBot;

            await channel.ModifyAsync(prop => prop.CategoryId = photoBot.Config.ArchiveCategoryId);
            await channel.ModifyAsync(prop =>
                prop.Name = $"-{GetWeekOfMonth(DateTime.Now)}-{DateTime.Now.Month}-{DateTime.Now.Year}-" + channel.Name);
        }

        static int GetWeekOfMonth(DateTime date)
        {
            var beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int) Math.Truncate((double) date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }
    }
}