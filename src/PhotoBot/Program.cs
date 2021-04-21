using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoBot
{
    internal static class Program
    {
        public static void Main(string[] args)
            => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            var token = await File.ReadAllTextAsync("token.txt");

            var config = await PhotoConfig.Load();

            var photoBot = new PhotoBot(config);
            await photoBot.ConnectAsync(token);

            await PhotoConfig.SaveAsync();

            await Task.Delay(-1);
        }
    }
}