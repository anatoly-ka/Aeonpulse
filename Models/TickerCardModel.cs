using Aeonpulse.Attributes;

namespace Aeonpulse.Models
{
    /// <summary>
    /// Describes the static structural metadata for a single ticker card as shown
    /// in the main scroll list. Separates display configuration (title, icon, capabilities)
    /// from live computed content, which is held in <see cref="TickerData"/>.
    ///
    /// <para>
    /// <b>Note:</b> this model is currently defined but not yet wired to a
    /// collection-based rendering pipeline. It exists to support a future
    /// refactor where ticker cards are driven by a bound <c>CollectionView</c>
    /// rather than individually templated XAML blocks.
    /// </para>
    /// </summary>
    [AIContext("DataTransferObject")]
    public class TickerCardModel
    {
        /// <summary>
        /// Gets or sets the localised display title shown in the card header
        /// (e.g., "Time Jubilees", "Countdown").
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon glyph character from a font icon set (e.g., FontAwesome).
        /// Displayed alongside the title to visually distinguish card types at a glance.
        /// </summary>
        public string IconGlyph { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this card's data updates in real time (every second via
        /// the <see cref="ViewModels.MainViewModel"/> timer). Live cards display a "LIVE"
        /// badge in the UI.
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        /// Gets or sets whether the card body is currently visible.
        /// Toggled by the per-card toggle commands on <see cref="ViewModels.MainViewModel"/>.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets or sets whether this card exposes a manual refresh button.
        /// Refresh commands are wired to <c>RefreshXxxCommand</c> on the ViewModel,
        /// which shows <see cref="Views.RefreshingPopup"/> before recalculating.
        /// </summary>
        public bool HasRefresh { get; set; }

        /// <summary>
        /// Gets or sets whether this card exposes a calendar-sync action
        /// (reserved for a future feature to export jubilee dates to the device calendar).
        /// </summary>
        public bool HasCalendarSync { get; set; }

        /// <summary>
        /// Gets or sets the live-updated content for this card, produced by
        /// <see cref="Services.CalculationService"/> and bound to the card body labels.
        /// </summary>
        public TickerData Data { get; set; } = new TickerData();
    }
}
