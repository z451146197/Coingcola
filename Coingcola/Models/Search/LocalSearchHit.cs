using System;

namespace Coingcola.Models.Search
{
    public sealed class LocalSearchHit
    {
        public string Title { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
    }
}
