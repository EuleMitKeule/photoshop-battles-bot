using System;

namespace PhotoBot
{
    [Serializable]
    public class PhotoProposal
    {
        public ulong UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Topic { get; set; }
    }
}