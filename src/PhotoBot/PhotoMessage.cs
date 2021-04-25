using System;

namespace PhotoBot
{
    [Serializable]
    public class PhotoMessage
    {
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Topic { get; set; }
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }

        public int Score => Upvotes - Downvotes;
    }
}