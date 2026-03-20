using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aeonpulse.Attributes;

namespace Aeonpulse.Models
{
    /// <summary>
    /// Represents the two display variants produced by every <c>CalculationService</c> method:
    /// a concise ticker string for the collapsed card, and a richer narrative for the expanded view.
    /// Implements <see cref="INotifyPropertyChanged"/> so that live-updating calculations (driven
    /// by the 1-second timer in <see cref="ViewModels.MainViewModel"/>) flow directly to bound UI
    /// labels without replacing the entire object reference.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class TickerData : INotifyPropertyChanged
    {
        private string _briefText = string.Empty;

        /// <summary>
        /// Short, single-line text shown in the collapsed ticker card header.
        /// Updated every second for live tickers (Countdown, LifeOdometer, etc.).
        /// </summary>
        public string BriefText
        {
            get => _briefText;
            set { _briefText = value; OnPropertyChanged(); }
        }

        private string _fullText = string.Empty;

        /// <summary>
        /// Expanded narrative shown when the user opens a ticker card.
        /// Includes contextual details such as the base date and data source references.
        /// </summary>
        public string FullText
        {
            get => _fullText;
            set { _fullText = value; OnPropertyChanged(); }
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the calling member.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
