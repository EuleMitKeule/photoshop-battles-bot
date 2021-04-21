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
        public ulong CommandChannelId { get; set; }
        public ulong GuildId { get; set; }
        public ulong PhotoCategoryId { get; set; }
        public ulong ArchiveCategoryId { get; set; }
        public ulong PhotoRoleId { get; set; }
        public ulong WinnerChannelId { get; set; }

        public List<ulong> PhotoUserIds { get; set; }

        public List<PhotoProposal> Proposals { get; set; }

        public ulong CurrentProposalsChannelId { get; set; }
        public Dictionary<ulong, ulong> UserIdToPhotoChannelId { get; set; }

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