namespace Aeonpulse.Models
{
    public class TickerCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = string.Empty; // FontAwesome or similar icon font
        public bool IsLive { get; set; }
        public bool IsExpanded { get; set; }
        public bool HasRefresh { get; set; }
        public bool HasCalendarSync { get; set; }
        public TickerData Data { get; set; } = new TickerData();
    }
}
