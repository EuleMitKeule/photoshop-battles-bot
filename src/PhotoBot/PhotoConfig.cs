using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhotoBot
{
    [Serializable]
    public class PhotoConfig
    {
        public ulong GuildId { get; } = 503583360476512276;
        public ulong PhotoCategoryId { get; } = 834181773914800149;
        public ulong ArchiveCategoryId { get; } = 834182201842335756;
        public ulong PhotoRoleId { get; } = 834196147777437706;

        public List<ulong> PhotoUserIds { get; set; }

        public ulong CurrentProposalsChannelId { get; set; }
        public ulong CurrentPhotosChannel { get; set; }

        public static async Task SaveAsync()
        {
            var photoConfig = Service.PhotoBot.Config;
            var json = JsonSerializer.Serialize(photoConfig);
            await File.WriteAllTextAsync("config.json", json);
        }

        public static async Task<PhotoConfig> Load()
        {
            var json = "";
            try
            {
                json = await File.ReadAllTextAsync("config.json");
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not read configuration! Creating new config file.");
            }

            var photoConfig = new PhotoConfig();

            try
            {
                photoConfig = JsonSerializer.Deserialize<PhotoConfig>(json);
            }
            catch (JsonException e)
            {
                Console.WriteLine("Could not read configuration! Creating new config file.");
            }

            return photoConfig;
        }
    }
}