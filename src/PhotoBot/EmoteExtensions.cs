using Discord;

namespace PhotoBot
{
    public static class EmoteExtensions
    {
        public static bool IsUpvote(this IEmote emote) =>
            emote.Name == "✅";

        public static bool IsDownvote(this IEmote emote) =>
            emote.Name == "❌";

        public static bool IsCancel(this IEmote emote) =>
            emote.Name == "🚫";
    }
}