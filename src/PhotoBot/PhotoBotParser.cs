using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PhotoBot
{
    public class PhotoBotParser
    {
        DiscordSocketClient Client { get; }
        CommandService Commands { get; }

        public PhotoBotParser(DiscordSocketClient client, CommandService commands)
        {
            Client = client;
            Commands = commands;

            Client.MessageReceived += OnMessageReceived;
        }

        public async Task Connect()
        {
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        async Task OnMessageReceived(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message) return;

            var argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(Client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(Client, message);

            await Commands.ExecuteAsync(context, argPos, null);
        }
    }
}