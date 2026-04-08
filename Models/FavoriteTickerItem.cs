using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Aeonpulse.Attributes;

namespace Aeonpulse.Models
{
    /// <summary>
    /// Represents a single "Live Bookmark" tile in the Favorites section.
    ///
    /// <para>
    /// <b>Design note (Live Bookmark / Portal architecture):</b> each favorite
    /// is a lightweight tile that shows the ticker's live <c>BriefText</c> only.
    /// It does not duplicate the full card. Tapping the tile body triggers
    /// <see cref="JumpToTickerCommand"/>, which expands the ticker's parent section,
    /// expands the ticker card itself, and scrolls the main content to it.
    /// </para>
    /// <para>
    /// <b>Single Source of Truth:</b> <see cref="Data"/> is a live reference to the
    /// same <see cref="TickerData"/> instance updated by the ViewModel timer, so
    /// <c>BriefText</c> bindings update automatically without any extra wiring.
    /// </para>
    /// <para>
    /// <b>Side effects / Hidden dependencies:</b>
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="JumpToTickerCommand"/> is wired to a callback provided by
    ///     <c>MainViewModel</c> at construction time. It expands the section and card,
    ///     then raises <c>MainViewModel.ScrollToTickerRequested</c> so that
    ///     <c>MainPage.xaml.cs</c> can call <c>ScrollToAsync</c> on the named element.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="RemoveFromFavoritesCommand"/> removes this tile from the
    ///     Favorites collection and reverts the <c>to_favorites.png</c> button on the
    ///     main ticker card by setting its <c>IsFavorite</c> property to <c>false</c>.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AIContext("DataTransferObject")]
    public class FavoriteTickerItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Stable string identifier for this ticker (e.g., "TimeJubilees").
        /// Used as the persistence key for the Favorites list.
        /// </summary>
        public string TickerId { get; }

        /// <summary>Emoji glyph shown at the left of the bookmark tile header.</summary>
        public string EmojiGlyph { get; }

        // Delegates kept live so Title and Data always reflect current language / recalculation.
        private readonly Func<string> _titleGetter;
        private readonly Func<TickerData> _dataGetter;

        /// <summary>
        /// Localised title for this ticker. Evaluated via delegate on every read so
        /// language changes (which update <c>AppResources</c>) are reflected immediately
        /// when <see cref="Refresh"/> raises <see cref="PropertyChanged"/>.
        /// </summary>
        public string Title => _titleGetter();

        /// <summary>
        /// Live reference to the shared <see cref="TickerData"/> instance whose
        /// <c>BriefText</c> is updated by the ViewModel on every recalculation.
        /// Evaluated via delegate so that reassigning the ViewModel property (which
        /// replaces the object reference) is always reflected in the tile.
        /// </summary>
        public TickerData Data => _dataGetter();

        /// <summary>
        /// Command bound to the tile body tap gesture.
        /// Expands the ticker's parent section, expands the ticker card itself,
        /// and requests a scroll to the ticker in the main list.
        /// </summary>
        public ICommand JumpToTickerCommand { get; }

        /// <summary>
        /// Command wired to the "Remove from Favorites" button (in_favorites.png).
        /// Removes this tile and reverts the add-to-favorites button on the main card.
        /// </summary>
        public ICommand RemoveFromFavoritesCommand { get; }

        /// <summary>
        /// Initialises a new <see cref="FavoriteTickerItem"/> Live Bookmark tile.
        /// </summary>
        /// <param name="tickerId">Stable string key for persistence.</param>
        /// <param name="emojiGlyph">Emoji displayed in the tile header.</param>
        /// <param name="titleGetter">Delegate returning the localised ticker title; re-evaluated on <see cref="Refresh"/>.</param>
        /// <param name="dataGetter">Delegate returning the current <see cref="TickerData"/>; re-evaluated on <see cref="Refresh"/>.</param>
        /// <param name="jumpAction">Action invoked when the user taps the tile body; triggers section/card expand + scroll.</param>
        /// <param name="removeAction">Action invoked when the user taps "Remove from Favorites".</param>
        public FavoriteTickerItem(
            string tickerId,
            string emojiGlyph,
            Func<string> titleGetter,
            Func<TickerData> dataGetter,
            Action<FavoriteTickerItem> jumpAction,
            Action<FavoriteTickerItem> removeAction)
        {
            TickerId   = tickerId;
            EmojiGlyph = emojiGlyph;
            _titleGetter = titleGetter;
            _dataGetter  = dataGetter;
            JumpToTickerCommand        = new Command(() => jumpAction(this));
            RemoveFromFavoritesCommand = new Command(() => removeAction(this));
        }

        /// <summary>
        /// Notifies the UI that <see cref="Title"/> and <see cref="Data"/> should be
        /// re-read. Call this whenever the underlying ticker is recalculated or the
        /// display language changes so the tile updates its title and brief text.
        /// </summary>
        public void Refresh()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Data));
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raises <see cref="PropertyChanged"/> for the calling member.</summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
